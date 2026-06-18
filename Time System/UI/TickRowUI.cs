using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace TimeSystem.UI
{

    public class TickRowUI : MonoBehaviour
    {
        [Header("Container")]
        [SerializeField] private Transform _ticksContainer;
        [SerializeField] private GameObject _tickPrefab;

        [Header("Colors")]
        [SerializeField] private Color _availableColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private Color _hoverColor = new Color(1f, 0.55f, 0.1f, 1f);
        [SerializeField] private Color _spentColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        [SerializeField] private Color _skipColor = new Color(0.3f, 0.7f, 1f, 1f);

        [Header("Layout")]
        [SerializeField] private float _tickSize = 30f;
        [SerializeField] private float _tickSpacing = 5f;

        [Header("Behavior")]

        [SerializeField] private bool _index0IsLeftmost = true;

        [Header("Info Display")]
        [SerializeField] private TextMeshProUGUI _tickInfoText;

        private readonly List<TickSlotUI> _tickSlots = new();

        private int _hoveredPosFromLeft = -1;

        private void Start()
        {
            if (_tickPrefab == null)
                CreateDefaultTickPrefab();

            if (GameTimeManager.Instance != null)
            {
                GameTimeManager.Instance.OnTicksChanged += OnTicksChanged;
                GameTimeManager.Instance.OnDayChanged += OnDayChanged;
                GameTimeManager.Instance.OnTickStarted += OnTickStarted;
                GameTimeManager.Instance.OnTickEnded += OnTickEnded;
            }

            Rebuild();
        }

        private void OnDestroy()
        {
            if (GameTimeManager.Instance != null)
            {
                GameTimeManager.Instance.OnTicksChanged -= OnTicksChanged;
                GameTimeManager.Instance.OnDayChanged -= OnDayChanged;
                GameTimeManager.Instance.OnTickStarted -= OnTickStarted;
                GameTimeManager.Instance.OnTickEnded -= OnTickEnded;
            }
        }

        private void CreateDefaultTickPrefab()
        {
            _tickPrefab = new GameObject("TickSlot");
            var image = _tickPrefab.AddComponent<Image>();
            image.color = _availableColor;
            var rt = _tickPrefab.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(_tickSize, _tickSize);
            _tickPrefab.SetActive(false);
        }

        private void Rebuild()
        {
            foreach (var slot in _tickSlots)
                if (slot != null && slot.gameObject != null)
                    Destroy(slot.gameObject);
            _tickSlots.Clear();

            if (GameTimeManager.Instance == null) return;
            int maxTicks = GameTimeManager.Instance.MaxTicksToday;

            for (int i = 0; i < maxTicks; i++)
            {
                var go = Instantiate(_tickPrefab, _ticksContainer);
                go.SetActive(true);
                go.name = $"Tick_{i}";
                var slot = go.AddComponent<TickSlotUI>();
                slot.Initialize(i, this);
                _tickSlots.Add(slot);
            }
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (GameTimeManager.Instance == null) return;
            int total = GameTimeManager.Instance.MaxTicksToday;

            if (_tickSlots.Count != total)
            {
                Rebuild();
                return;
            }

            int spent = total - GameTimeManager.Instance.TicksRemaining;

            for (int i = 0; i < total; i++)
            {
                var slot = _tickSlots[i];
                if (slot == null) continue;

                int posFromLeft = _index0IsLeftmost ? i : (total - 1 - i);

                bool isSpent = posFromLeft >= total - spent;

                if (GameTimeManager.Instance.IsTickActive)
                {
                    slot.SetColor(isSpent ? _spentColor : _skipColor);
                }
                else if (isSpent)
                {
                    slot.SetColor(_spentColor);
                }
                else if (_hoveredPosFromLeft >= 0 && posFromLeft >= _hoveredPosFromLeft)
                {
                    slot.SetColor(_hoverColor);
                }
                else
                {
                    slot.SetColor(_availableColor);
                }
            }
        }

        public void OnTickHovered(int index)
        {
            if (GameTimeManager.Instance == null) return;
            if (GameTimeManager.Instance.IsTickActive) return;

            int total = GameTimeManager.Instance.MaxTicksToday;
            int spent = total - GameTimeManager.Instance.TicksRemaining;

            int posFromLeft = _index0IsLeftmost ? index : (total - 1 - index);

            if (posFromLeft < total - spent)
            {
                _hoveredPosFromLeft = posFromLeft;
                UpdateVisuals();

                int available = total - spent;
                int ticksToSpend = Mathf.Clamp(available - posFromLeft, 0, GameTimeManager.Instance.TicksRemaining);
                float hoursToPass = ticksToSpend * GameTimeManager.Instance.HoursPerTick;
                if (_tickInfoText != null)
                    _tickInfoText.text = $"{ticksToSpend} {GetTaktDeclension(ticksToSpend)} ({hoursToPass:F1}ч)";
                else
                    Debug.Log($"[TickRowUI] Hover: will skip {ticksToSpend} ticks ({hoursToPass:F1}h)");
            }
        }

        public void OnTickUnhovered()
        {
            if (GameTimeManager.Instance != null && GameTimeManager.Instance.IsTickActive) return;
            _hoveredPosFromLeft = -1;
            UpdateVisuals();
            if (_tickInfoText != null)
            {
                if (GameTimeManager.Instance != null)
                {
                    int remaining = GameTimeManager.Instance.TicksRemaining;
                    int total = GameTimeManager.Instance.MaxTicksToday;
                    _tickInfoText.text = $"Тактов: {remaining}/{total}";
                }
                else
                {
                    _tickInfoText.text = "";
                }
            }
        }

        public void OnTickRightClicked(int index)
        {
            if (GameTimeManager.Instance == null) return;
            if (GameTimeManager.Instance.IsTickActive) return;

            int total = GameTimeManager.Instance.MaxTicksToday;
            int spent = total - GameTimeManager.Instance.TicksRemaining;

            int posFromLeft = _index0IsLeftmost ? index : (total - 1 - index);

            if (posFromLeft < total - spent)
            {
                int available = total - spent;
                int ticksToSpend = available - posFromLeft;
                ticksToSpend = Mathf.Clamp(ticksToSpend, 0, GameTimeManager.Instance.TicksRemaining);
                if (ticksToSpend > 0)
                {
                    Debug.Log($"[TickRowUI] Spending {ticksToSpend} ticks (posFromLeft {posFromLeft}, index {index})");
                    GameTimeManager.Instance.SpendTicks(ticksToSpend);
                }
            }
        }

        private void OnTicksChanged(int remaining)
        {
            UpdateVisuals();
        }

        private void OnDayChanged(int day)
        {

            Rebuild();
        }

        private void OnTickStarted()
        {
            Debug.Log("[TickRowUI] OnTickStarted - entering skip/tick active state");
            _hoveredPosFromLeft = -1;
            if (_tickInfoText != null)
                _tickInfoText.text = "";
            UpdateVisuals();
        }

        private void OnTickEnded()
        {
            Debug.Log("[TickRowUI] OnTickEnded - exiting skip/tick active state");
            _hoveredPosFromLeft = -1;
            if (_tickInfoText != null && GameTimeManager.Instance != null)
            {
                int remaining = GameTimeManager.Instance.TicksRemaining;
                int total = GameTimeManager.Instance.MaxTicksToday;
                _tickInfoText.text = $"Тактов: {remaining}/{total}";
            }
            UpdateVisuals();
        }

        private string GetTaktDeclension(int count)
        {
            int lastDigit = count % 10;
            int lastTwoDigits = count % 100;

            if (lastTwoDigits >= 11 && lastTwoDigits <= 19)
                return "тактов";

            if (lastDigit == 1)
                return "такт";

            if (lastDigit >= 2 && lastDigit <= 4)
                return "такта";

            return "тактов";
        }

    }

    public class TickSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private int _index;
        private TickRowUI _parent;
        private Image _image;

        public void Initialize(int index, TickRowUI parent)
        {
            _index = index;
            _parent = parent;
            _image = GetComponent<Image>();
        }

        public void SetColor(Color color)
        {
            if (_image != null)
                _image.color = color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _parent?.OnTickHovered(_index);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _parent?.OnTickUnhovered();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
                _parent?.OnTickRightClicked(_index);
        }
    }
}
