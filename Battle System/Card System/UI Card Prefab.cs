using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace BattleSystem.CardSystem
{

    public class UICardPrefab : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("UI References")]
        [SerializeField] private Image cardImage;
        [SerializeField] private Image cardBackground;
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI energyCostText;

        [Header("Colors")]
        [SerializeField] private Color affordableColor = Color.white;
        [SerializeField] private Color unaffordableColor = new Color(0.5f, 0.5f, 0.5f);

        public CardInstance BoundCard { get; private set; }
        public RectTransform RectTransform { get; private set; }

        private bool _isDragging;
        public bool IsDragging => _isDragging;

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            if (EnergyController.Instance != null)
                EnergyController.Instance.OnEnergyChanged += OnEnergyChanged;
        }

        private void OnDisable()
        {
            if (EnergyController.Instance != null)
                EnergyController.Instance.OnEnergyChanged -= OnEnergyChanged;
        }

        private void OnEnergyChanged(int current, int max)
        {
            UpdateAffordability();
        }

        public void Bind(CardInstance cardInstance)
        {
            BoundCard = cardInstance;
            Refresh();
        }

        public void Refresh()
        {
            if (BoundCard == null) return;

            var config = BoundCard.Config;

            if (cardImage != null)
                cardImage.sprite = config.CardImage;
            if (cardNameText != null)
                cardNameText.text = config.CardName;
            if (descriptionText != null)
                descriptionText.text = config.Description;
            if (energyCostText != null)
                energyCostText.text = BoundCard.CurrentEnergyCost.ToString();

            UpdateAffordability();
        }

        private void UpdateAffordability()
        {
            if (BoundCard == null || cardBackground == null) return;

            bool canAfford = EnergyController.Instance == null ||
                             EnergyController.Instance.HasEnergy(BoundCard.CurrentEnergyCost);

            cardBackground.color = canAfford ? affordableColor : unaffordableColor;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            if (BoundCard != null && EnergyController.Instance != null)
            {
                if (!EnergyController.Instance.HasEnergy(BoundCard.CurrentEnergyCost))
                {
                    return;
                }
            }

            _isDragging = true;
            MainCardController.Instance?.OnCardBeginDrag(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            MainCardController.Instance?.OnCardDrag(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            _isDragging = false;
            MainCardController.Instance?.OnCardEndDrag(this, eventData);
        }

        public void ForceEndDrag()
        {
            _isDragging = false;
        }

        public void ReturnToHand()
        {
        }
    }
}