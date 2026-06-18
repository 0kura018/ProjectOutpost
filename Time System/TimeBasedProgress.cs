using UnityEngine;
using System;

namespace TimeSystem
{

    public class TimeBasedProgress : MonoBehaviour
    {
        [Header("Progress Settings")]
        [Tooltip("Общее время выполнения в игровых часах")]
        [SerializeField] private float _totalGameHours;

        [Tooltip("Текущий прогресс в игровых часах")]
        [SerializeField] private float _progressHours;

        [SerializeField] private bool _isActive = false;
        [SerializeField] private bool _isPaused = false;

        public event Action<float> OnProgressChanged;
        public event Action OnCompleted;
        public event Action<float> OnNightPenaltyApplied;

        public float TotalGameHours => _totalGameHours;
        public float ProgressHours => _progressHours;
        public float ProgressNormalized => _totalGameHours > 0 ? _progressHours / _totalGameHours : 0;
        public float RemainingHours => Mathf.Max(0, _totalGameHours - _progressHours);
        public bool IsActive => _isActive;
        public bool IsCompleted => RemainingHours <= (GameTimeManager.Instance != null ? GameTimeManager.Instance.CompletionOffset : 0.001f);
        public bool IsPaused => _isPaused;

        private void Start()
        {
            if (GameTimeManager.Instance != null)
            {
                GameTimeManager.Instance.OnNightTimeAdvanced += HandleNightTimeAdvanced;
            }
        }

        private void OnDestroy()
        {
            if (GameTimeManager.Instance != null)
            {
                GameTimeManager.Instance.OnNightTimeAdvanced -= HandleNightTimeAdvanced;
            }
        }

        private void Update()
        {
            if (!_isActive || _isPaused || IsCompleted) return;

            float gameHoursPassed = GameTimeManager.Instance.CurrentGameHoursDelta;
            AddProgress(gameHoursPassed);
        }

        public void Initialize(float totalGameHours, bool startActive = true)
        {
            _totalGameHours = totalGameHours;
            _progressHours = 0f;
            _isActive = startActive;
            _isPaused = false;

            OnProgressChanged?.Invoke(0f);
        }

        public void AddProgress(float gameHours)
        {
            if (!_isActive || IsCompleted) return;

            _progressHours = Mathf.Min(_progressHours + gameHours, _totalGameHours);
            OnProgressChanged?.Invoke(ProgressNormalized);

            if (IsCompleted)
            {
                Complete();
            }
        }

        public void RemoveProgress(float gameHours)
        {
            if (!_isActive || IsCompleted) return;

            float removed = Mathf.Min(gameHours, _progressHours);
            _progressHours = Mathf.Max(0, _progressHours - gameHours);
            OnProgressChanged?.Invoke(ProgressNormalized);
            OnNightPenaltyApplied?.Invoke(removed);
        }

        public void Complete()
        {
            _progressHours = _totalGameHours;
            _isActive = false;
            OnProgressChanged?.Invoke(1f);
            OnCompleted?.Invoke();
        }

        public void ForceComplete()
        {
            Complete();
        }

        public void Pause() => _isPaused = true;
        public void Resume() => _isPaused = false;
        public void Stop() => _isActive = false;

        private void HandleNightTimeAdvanced(float nightHours)
        {
            if (!_isActive || IsCompleted) return;

            if (GameTimeManager.Instance == null || !GameTimeManager.Instance.NightPenaltyEnabled) return;

            float rate = GameTimeManager.Instance.NightPenaltyRatePerHour;
            if (rate <= 0f) return;

            RemoveProgress(nightHours * rate);
        }

        public int EstimateTicksToComplete()
        {
            if (GameTimeManager.Instance == null || IsCompleted) return 0;

            float hoursRemaining = RemainingHours;
            float hoursPerTick = GameTimeManager.Instance.HoursPerTick;

            return Mathf.CeilToInt(hoursRemaining / hoursPerTick);
        }
    }
}

