using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem.Visual
{
    [RequireComponent(typeof(RectTransform))]
    public class HealthBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Slider _slider;
        [SerializeField] private Image _fillImage;

        [Header("Colors")]
        [SerializeField] private Color _allyColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color _enemyColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color _lowHealthColor = new Color(1f, 0.5f, 0f);

        [Header("Settings")]
        [SerializeField] private float _lowHealthThreshold = 0.3f;

        public Transform TargetUnit { get; private set; }
        public RectTransform RectTransform { get; private set; }
        public float BaseScreenY { get; set; }
        public bool IsAlive { get; private set; } = true;
        public float WorldOffsetY { get; private set; } = 1.5f;

        private int _maxHealth;
        private int _currentHealth;
        private bool _isAlly;

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();

            if (_slider == null)
                _slider = GetComponentInChildren<Slider>();

            if (_fillImage == null && _slider != null)
                _fillImage = _slider.fillRect?.GetComponent<Image>();
        }

        public void Initialize(Transform unit, int maxHealth, int currentHealth, bool isAlly, float worldOffsetY = 1.5f)
        {
            TargetUnit = unit;
            _maxHealth = maxHealth;
            _currentHealth = currentHealth;
            _isAlly = isAlly;
            IsAlive = true;
            WorldOffsetY = worldOffsetY;

            if (_slider != null)
            {
                _slider.maxValue = maxHealth;
                _slider.value = currentHealth;
            }

            UpdateColor();
        }

        public void UpdateValue(int currentHealth, int maxHealth)
        {
            _currentHealth = currentHealth;
            _maxHealth = maxHealth;
            IsAlive = currentHealth > 0;

            if (_slider != null)
            {
                _slider.maxValue = maxHealth;
                _slider.value = currentHealth;
            }

            UpdateColor();
        }

        public void UpdateValue(int currentHealth)
        {
            _currentHealth = currentHealth;
            IsAlive = currentHealth > 0;

            if (_slider != null)
                _slider.value = currentHealth;

            UpdateColor();
        }

        private void UpdateColor()
        {
            if (_fillImage == null) return;

            float healthPercent = _maxHealth > 0 ? (float)_currentHealth / _maxHealth : 0f;
            Color baseColor = _isAlly ? _allyColor : _enemyColor;

            if (healthPercent <= _lowHealthThreshold)
            {
                float t = healthPercent / _lowHealthThreshold;
                _fillImage.color = Color.Lerp(_lowHealthColor, baseColor, t);
            }
            else
            {
                _fillImage.color = baseColor;
            }
        }

        public void Clear()
        {
            TargetUnit = null;
            BaseScreenY = 0f;
            IsAlive = true;
        }
    }
}
