using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using BattleSystem.Units;

namespace BattleSystem.CardSystem
{

    public class MainCardController : MonoBehaviour
    {
        public static MainCardController Instance { get; private set; }

        [Header("References")]
        [SerializeField] private DeckController deckController;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Canvas canvas;

        [Header("Targeting Line")]
        [SerializeField] private CardTargetingLine targetingLinePrefab;

        [Header("AOE Indicator")]
        [SerializeField] private GameObject aoeIndicatorPrefab;

        [Header("Targeting")]
        [SerializeField] private LayerMask unitLayerMask;

        [Header("Settings")]
        [SerializeField] private float unitDetectionRadius = 50f;

        [Header("Time Settings")]
        [SerializeField] private bool useSlowMotion = true;
        [SerializeField] private float slowMotionScale = 0.75f;

        private CardTargetingLine _targetingLine;
        private GameObject _aoeIndicator;
        private UICardPrefab _draggedCard;
        private UnitStateMachine _hoveredUnit;
        private Vector2 _targetWorldPoint;
        private bool _isValidTarget;
        private bool _isDragging;
        private float _originalTimeScale;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (targetingLinePrefab != null)
            {
                _targetingLine = Instantiate(targetingLinePrefab, transform);
                _targetingLine.Initialize(mainCamera, canvas);
            }

            if (aoeIndicatorPrefab != null)
            {
                _aoeIndicator = Instantiate(aoeIndicatorPrefab, transform);
            }
            else
            {

                _aoeIndicator = CreateDefaultAOEIndicator();
            }
            _aoeIndicator.SetActive(false);
        }

        private GameObject CreateDefaultAOEIndicator()
        {
            var indicator = new GameObject("AOEIndicator");
            indicator.transform.SetParent(transform);

            var lineRenderer = indicator.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.loop = true;
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.startColor = new Color(1f, 0.3f, 0.3f, 0.8f);
            lineRenderer.endColor = new Color(1f, 0.3f, 0.3f, 0.8f);
            lineRenderer.sortingLayerName = "UI";
            lineRenderer.sortingOrder = 100;

            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

            int segments = 32;
            lineRenderer.positionCount = segments;

            for (int i = 0; i < segments; i++)
            {
                float angle = i * 2 * Mathf.PI / segments;

                lineRenderer.SetPosition(i, new Vector3(Mathf.Cos(angle) * 0.5f, Mathf.Sin(angle) * 0.5f, 0));
            }

            return indicator;
        }

        private void Update()
        {

            if (_isDragging && Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                CancelDrag();
            }
        }

        public void OnCardBeginDrag(UICardPrefab card)
        {
            _draggedCard = card;
            _hoveredUnit = null;
            _isValidTarget = false;
            _isDragging = true;

            if (useSlowMotion)
            {
                _originalTimeScale = Time.timeScale;
                Time.timeScale = slowMotionScale;
            }
        }

        public void OnCardDrag(UICardPrefab card, PointerEventData eventData)
        {
            if (_draggedCard == null || _draggedCard.BoundCard == null) return;

            var config = _draggedCard.BoundCard.Config;
            Vector3 cardScreenPos = card.RectTransform.position;
            Vector3 cursorScreenPos = eventData.position;

            _targetingLine?.Show(cardScreenPos, cursorScreenPos);

            switch (config.TargetType)
            {
                case CardTargetType.Unit_All:
                case CardTargetType.Unit_Enemy:
                case CardTargetType.Unit_Ally:
                    HandleUnitTargeting(eventData, config.TargetType);
                    break;

                case CardTargetType.AOE_All:
                case CardTargetType.AOE_Enemy:
                case CardTargetType.AOE_Ally:
                    HandleAOETargeting(eventData, config.AOERadius);
                    break;

                case CardTargetType.Self:
                    _isValidTarget = true;
                    break;
            }
        }

        public void OnCardEndDrag(UICardPrefab card, PointerEventData eventData)
        {

            RestoreTimeScale();

            _targetingLine?.Hide();
            _aoeIndicator?.SetActive(false);

            if (!_isDragging || _draggedCard == null || _draggedCard.BoundCard == null)
            {
                card.ReturnToHand();
                ResetDragState();
                return;
            }

            var cardInstance = _draggedCard.BoundCard;
            var config = cardInstance.Config;

            if (_isValidTarget && TryUseCard(cardInstance, config))
            {
                deckController.DiscardCard(cardInstance);
            }
            else
            {
                card.ReturnToHand();
            }

            ResetDragState();
        }

        public void CancelDrag()
        {
            if (!_isDragging) return;

            RestoreTimeScale();

            _targetingLine?.Hide();
            _aoeIndicator?.SetActive(false);

            _draggedCard?.ReturnToHand();
            _draggedCard?.ForceEndDrag();

            ResetDragState();
        }

        private void RestoreTimeScale()
        {
            if (useSlowMotion)
            {
                Time.timeScale = _originalTimeScale > 0 ? _originalTimeScale : 1f;
            }
        }

        private void ResetDragState()
        {
            _draggedCard = null;
            _hoveredUnit = null;
            _isValidTarget = false;
            _isDragging = false;
        }

        private void HandleUnitTargeting(PointerEventData eventData, CardTargetType targetType)
        {
            _hoveredUnit = null;
            _isValidTarget = false;

            Vector2 worldPos = mainCamera.ScreenToWorldPoint(eventData.position);
            var hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, unitLayerMask);

            if (hit.collider != null)
            {
                var unit = hit.collider.GetComponentInParent<UnitStateMachine>();
                if (unit != null && IsValidUnitTarget(unit, targetType))
                {
                    _hoveredUnit = unit;
                    _isValidTarget = true;
                }
            }

            if (_hoveredUnit == null)
            {
                _hoveredUnit = FindClosestUnitToCursor(eventData.position, targetType);
                _isValidTarget = _hoveredUnit != null;
            }
        }

        private void HandleAOETargeting(PointerEventData eventData, float aoeRadius)
        {
            _targetWorldPoint = mainCamera.ScreenToWorldPoint(eventData.position);
            _isValidTarget = true;

            if (_aoeIndicator != null)
            {
                _aoeIndicator.SetActive(true);
                _aoeIndicator.transform.position = (Vector3)_targetWorldPoint;
                _aoeIndicator.transform.localScale = Vector3.one * aoeRadius * 2f;
            }
        }

        private bool TryUseCard(CardInstance cardInstance, CardConfig config)
        {

            int cost = cardInstance.CurrentEnergyCost;
            if (EnergyController.Instance != null && !EnergyController.Instance.HasEnergy(cost))
            {
                Debug.Log($"Not enough energy! Need {cost}, have {EnergyController.Instance.CurrentEnergy}");
                return false;
            }

            EnergyController.Instance?.SpendEnergy(cost);

            Debug.Log($"[MainCardController] Using card: {config.CardName}, TargetType: {config.TargetType}");

            switch (config.TargetType)
            {
                case CardTargetType.Unit_All:
                case CardTargetType.Unit_Enemy:
                case CardTargetType.Unit_Ally:
                    if (_hoveredUnit != null)
                    {
                        Debug.Log($"[MainCardController] Applying to unit: {_hoveredUnit.name}");
                        config.ApplyEffects(_hoveredUnit);
                        return true;
                    }
                    Debug.LogWarning("[MainCardController] No hovered unit for unit-targeting card!");
                    break;

                case CardTargetType.Self:
                    config.ApplyEffectsAtPoint(_targetWorldPoint);
                    return true;

                case CardTargetType.AOE_All:
                case CardTargetType.AOE_Enemy:
                case CardTargetType.AOE_Ally:

                    config.ApplyEffectsAtPoint(_targetWorldPoint);
                    return true;
            }

            return false;
        }

        private bool IsValidUnitTarget(UnitStateMachine unit, CardTargetType targetType)
        {
            var affiliation = unit.UnitCurrentStats.UnitAffiliation;

            return targetType switch
            {
                CardTargetType.Unit_All => true,
                CardTargetType.Unit_Enemy => affiliation == UnitAffiliation.Enemy,
                CardTargetType.Unit_Ally => affiliation == UnitAffiliation.Ally,
                _ => false
            };
        }

        private UnitStateMachine FindClosestUnitToCursor(Vector2 screenPos, CardTargetType targetType)
        {
            UnitStateMachine closest = null;
            float closestDist = unitDetectionRadius;

            var allUnits = FindObjectsByType<UnitStateMachine>(FindObjectsSortMode.None);

            foreach (var unit in allUnits)
            {
                if (!IsValidUnitTarget(unit, targetType)) continue;

                Vector3 unitScreenPos = mainCamera.WorldToScreenPoint(unit.transform.position);
                if (unitScreenPos.z < 0) continue;

                float dist = Vector2.Distance(screenPos, unitScreenPos);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = unit;
                }
            }

            return closest;
        }

        private void OnDestroy()
        {

            if (_isDragging)
                RestoreTimeScale();

            if (Instance == this) Instance = null;
        }
    }
}