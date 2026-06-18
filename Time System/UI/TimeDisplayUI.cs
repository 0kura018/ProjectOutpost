using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TimeSystem.UI
{

    public class TimeDisplayUI : MonoBehaviour
    {
        [Header("References")]

        [SerializeField] private TextMeshProUGUI _dayText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private TextMeshProUGUI _ticksText;
        [SerializeField] private Slider _ticksSlider;
        [SerializeField] private Image _tickActiveIndicator;

        [Header("Tick Spending")]
        [SerializeField] private Button _spendTickButton;
        [SerializeField] private Button _spendAllTicksButton;

        [SerializeField] private InputField _tickAmountInput;

        [Header("Colors")]
        [SerializeField] private Color _tickActiveColor = Color.green;
        [SerializeField] private Color _tickInactiveColor = Color.gray;

        private void Start()
        {
            if (GameTimeManager.Instance != null)
            {
                GameTimeManager.Instance.OnDayChanged += UpdateDayDisplay;
                GameTimeManager.Instance.OnTimeChanged += UpdateTimeDisplay;
                GameTimeManager.Instance.OnTicksChanged += UpdateTicksDisplay;
                GameTimeManager.Instance.OnTickStarted += OnTickStarted;
                GameTimeManager.Instance.OnTickEnded += OnTickEnded;

                UpdateDayDisplay(GameTimeManager.Instance.CurrentDay);
                UpdateTimeDisplay(GameTimeManager.Instance.CurrentTimeHours);
                UpdateTicksDisplay(GameTimeManager.Instance.TicksRemaining);
            }

            if (_spendTickButton != null)
                _spendTickButton.onClick.AddListener(OnSpendTickClicked);

            if (_spendAllTicksButton != null)
                _spendAllTicksButton.onClick.AddListener(OnSpendAllTicksClicked);
        }

        private void OnDestroy()
        {
            if (GameTimeManager.Instance != null)
            {
                GameTimeManager.Instance.OnDayChanged -= UpdateDayDisplay;
                GameTimeManager.Instance.OnTimeChanged -= UpdateTimeDisplay;
                GameTimeManager.Instance.OnTicksChanged -= UpdateTicksDisplay;
                GameTimeManager.Instance.OnTickStarted -= OnTickStarted;
                GameTimeManager.Instance.OnTickEnded -= OnTickEnded;
            }
        }

        private void UpdateDayDisplay(int day)
        {
            if (_dayText != null)
                _dayText.text = $"День {day}";
        }

        private void UpdateTimeDisplay(float hours)
        {
            if (_timeText != null && GameTimeManager.Instance != null)
                _timeText.text = GameTimeManager.Instance.GetFormattedTime();
        }

        private void UpdateTicksDisplay(int ticksRemaining)
        {
            if (GameTimeManager.Instance == null) return;

            int maxTicks = GameTimeManager.Instance.MaxTicksToday;

            if (_ticksText != null)
                _ticksText.text = $"Тактов: {ticksRemaining}/{maxTicks}";

            if (_ticksSlider != null)
            {
                _ticksSlider.maxValue = maxTicks;
                _ticksSlider.value = ticksRemaining;
            }
        }

        private void OnTickStarted()
        {
            if (_tickActiveIndicator != null)
                _tickActiveIndicator.color = _tickActiveColor;

            SetButtonsInteractable(false);
        }

        private void OnTickEnded()
        {
            if (_tickActiveIndicator != null)
                _tickActiveIndicator.color = _tickInactiveColor;

            SetButtonsInteractable(true);
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (_spendTickButton != null)
                _spendTickButton.interactable = interactable;

            if (_spendAllTicksButton != null)
                _spendAllTicksButton.interactable = interactable;
        }

        private void OnSpendTickClicked()
        {
            if (GameTimeManager.Instance == null) return;

            int amount = 1;
            if (_tickAmountInput != null && int.TryParse(_tickAmountInput.text, out int parsed))
            {
                amount = Mathf.Max(1, parsed);
            }

            GameTimeManager.Instance.SpendTicks(amount);
        }

        private void OnSpendAllTicksClicked()
        {
            GameTimeManager.Instance?.SpendAllTicks();
        }
    }
}
