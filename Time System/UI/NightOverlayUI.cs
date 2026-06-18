using UnityEngine;

namespace TimeSystem.UI
{
    public class NightOverlayUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField, Range(0f, 1f)] private float _maxAlpha = 0.5882353f;
        [SerializeField] private float _fadeInStartHour = 0f;
        [SerializeField] private float _fadeInEndHour = 3f;
        [SerializeField] private float _fadeOutEndHour = 6f;

        private void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponentInChildren<CanvasGroup>();
            }

            if (_canvasGroup == null)
            {
                Debug.LogError($"[NightOverlayUI] CanvasGroup not found on {name} or its children!");
            }
        }

        private void OnEnable()
        {
            if (GameTimeManager.Instance != null)
            {
                GameTimeManager.Instance.OnTimeChanged += HandleTimeChanged;
                HandleTimeChanged(GameTimeManager.Instance.CurrentTimeHours);
            }
        }

        private void OnDisable()
        {
            if (GameTimeManager.Instance != null)
            {
                GameTimeManager.Instance.OnTimeChanged -= HandleTimeChanged;
            }
        }

        private void HandleTimeChanged(float timeHours)
        {
            if (_canvasGroup == null) return;

            float alpha = 0f;

            if (timeHours >= _fadeInStartHour && timeHours <= _fadeInEndHour)
            {
                float t = Mathf.InverseLerp(_fadeInStartHour, _fadeInEndHour, timeHours);
                alpha = Mathf.Lerp(0f, _maxAlpha, t);
            }
            else if (timeHours > _fadeInEndHour && timeHours <= _fadeOutEndHour)
            {
                float t = Mathf.InverseLerp(_fadeInEndHour, _fadeOutEndHour, timeHours);
                alpha = Mathf.Lerp(_maxAlpha, 0f, t);
            }

            if (Mathf.Abs(_canvasGroup.alpha - alpha) > 0.001f)
            {
                _canvasGroup.alpha = alpha;

            }
        }
    }
}
