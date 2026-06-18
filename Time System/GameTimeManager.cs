using UnityEngine;
using System;

namespace TimeSystem
{
    [DefaultExecutionOrder(-100)]
    public class GameTimeManager : MonoBehaviour
    {
        public static GameTimeManager Instance { get; private set; }

        [Header("Time Settings")]
        [Tooltip("Длительность смены в игровых часах")]
        [SerializeField] private float _shiftDurationHours = 18f;

        [Tooltip("1 игровой час = сколько реальных секунд (для нормальной скорости)")]
        [SerializeField] private float _realSecondsPerGameHour = 60f;

        [Tooltip("Скорость при тиках работы (1 игровой час за X секунд реальных)")]
        [SerializeField] private float _tickSpeedRealSeconds = 5f;

        [Header("Day/Night Settings")]
        [SerializeField] private float _dayStartHour = 6f;
        [SerializeField] private float _nightDurationHours = 6f;

        [Header("Tick Settings")]
        [SerializeField] private int _baseTicksPerShift = 10;
        [SerializeField] private int _ticksPerResident = 2;
        [SerializeField] private int _maxTicksPerShift = 36;
        [Tooltip("Оффсет для завершения прогресса (если осталось меньше этого времени в часах, считается завершенным)")]
        [SerializeField] private float _completionOffset = 0.01f;

        [Header("Night Penalty")]
        [Tooltip("Штраф за ночную работу (в игровых часах)")]
        [SerializeField] private float _nightPenaltyHours = 1f;
        [SerializeField] private bool _nightPenaltyEnabled = true;

        [Header("Current State")]
        [SerializeField] private int _currentDay = 0;
        [SerializeField] private float _currentTimeHours = 6f;
        [SerializeField] private int _ticksRemainingToday;
        [SerializeField] private int _residentCount = 0;
        [SerializeField] private bool _isTickActive = false;

        public event Action<int> OnDayChanged;
        public event Action<float> OnTimeChanged;
        public event Action<int> OnTicksChanged;
        public event Action OnTickStarted;
        public event Action OnTickEnded;
        public event Action OnSingleTickPassed;
        public event Action OnShiftEnded;
        public event Action OnNightStarted;
        public event Action OnNightEnded;
        public event Action<float> OnNightTimeAdvanced;

        public int CurrentDay => _currentDay;
        public float CurrentTimeHours => _currentTimeHours;
        public int TicksRemaining => _ticksRemainingToday;
        public int MaxTicksToday => CalculateMaxTicks();
        public bool IsTickActive => _isTickActive;
        public float ShiftDurationHours => _shiftDurationHours;
        public float CompletionOffset => _completionOffset;
        public float TickSpeedRealSeconds => _tickSpeedRealSeconds;
        public float RealSecondsPerGameHour => _realSecondsPerGameHour;
        public bool NightPenaltyEnabled
        {
            get => _nightPenaltyEnabled;
            set => _nightPenaltyEnabled = value;
        }
        public bool IsNightActive => _isNightActive;
        public float NightDurationHours => _nightDurationHours;
        public float NightPenaltyHours => _nightPenaltyHours;
        public float NightPenaltyRatePerHour => _nightDurationHours > 0f ? _nightPenaltyHours / _nightDurationHours : 0f;
        public float DayStartHour => _dayStartHour;

        public float HoursPerTick => _shiftDurationHours / MaxTicksToday;

        private float _tickTimeScale;
        private float _originalMaxDeltaTime;

        private float _currentTickHoursRemaining;
        private float _hoursInCurrentTickPassed;
        private int _ticksBeingSpent;
        private bool _isNightActive;
        private float _nightHoursRemaining;

        public float CurrentTickHoursRemaining => _currentTickHoursRemaining;
        public float HoursInCurrentTickPassed => _hoursInCurrentTickPassed;
        public int TicksBeingSpent => _ticksBeingSpent;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _originalMaxDeltaTime = Time.maximumDeltaTime;

            StartNewDay();
        }

        public float CurrentGameHoursDelta => _currentGameHoursDelta;

        private float _currentGameHoursDelta;

        private void Update()
        {
            float logicalMultiplier = 1f;

            if (_isTickActive || _isNightActive)
            {

                float targetScale = _realSecondsPerGameHour / Mathf.Max(0.01f, _tickSpeedRealSeconds);

                float baseScale = Mathf.Min(100f, targetScale);

                logicalMultiplier = Mathf.Pow(targetScale / Mathf.Max(0.01f, baseScale), 2f);

                _currentGameHoursDelta = (Time.deltaTime * logicalMultiplier * Time.timeScale) / Mathf.Max(0.01f, _realSecondsPerGameHour);
            }
            else
            {

                _currentGameHoursDelta = 0f;
            }

            if (_isNightActive)
            {
                ProcessNightTime(_currentGameHoursDelta);
            }
            else if (_isTickActive)
            {
                ProcessActiveTick(_currentGameHoursDelta);
            }
        }

        public bool SpendTicks(int tickCount)
        {
            if (_isNightActive)
            {
                Debug.LogWarning("[GameTime] Cannot spend ticks during night.");
                return false;
            }

            if (_isTickActive)
            {
                Debug.LogWarning("[GameTime] Tick already active!");
                return false;
            }

            if (tickCount <= 0 || tickCount > _ticksRemainingToday)
            {
                Debug.LogWarning($"[GameTime] Invalid tick count: {tickCount}, remaining: {_ticksRemainingToday}");
                return false;
            }

            _ticksBeingSpent = tickCount;
            _currentTickHoursRemaining = tickCount * HoursPerTick;
            _ticksRemainingToday -= tickCount;

            StartTick();

            OnTicksChanged?.Invoke(_ticksRemainingToday);

            Debug.Log($"[GameTime] Spending {tickCount} ticks ({_currentTickHoursRemaining:F1}h), remaining: {_ticksRemainingToday}");
            return true;
        }

        public bool SpendAllTicks()
        {
            return SpendTicks(_ticksRemainingToday);
        }

        private void StartTick()
        {
            _isTickActive = true;

            float targetScale = _realSecondsPerGameHour / Mathf.Max(0.01f, _tickSpeedRealSeconds);
            float visualScale = Mathf.Min(100f, targetScale);

            Time.timeScale = visualScale;
            Time.maximumDeltaTime = Mathf.Max(_originalMaxDeltaTime, visualScale * 0.05f);

            OnTickStarted?.Invoke();
        }

        private void EndTick()
        {
            _isTickActive = false;
            Time.timeScale = 1f;
            Time.maximumDeltaTime = _originalMaxDeltaTime;
            OnTickEnded?.Invoke();

            if (_ticksRemainingToday <= 0)
            {
                EndShift();
            }
        }

        private void ProcessActiveTick(float gameHoursPassed)
        {

            float actualPassed = Mathf.Min(gameHoursPassed, _currentTickHoursRemaining);

            _currentTickHoursRemaining -= actualPassed;
            _currentTimeHours += actualPassed;

            _hoursInCurrentTickPassed += actualPassed;

            while (_hoursInCurrentTickPassed >= HoursPerTick - _completionOffset)
            {
                _hoursInCurrentTickPassed -= HoursPerTick;
                if (_hoursInCurrentTickPassed < 0) _hoursInCurrentTickPassed = 0;
                OnSingleTickPassed?.Invoke();
            }

            OnTimeChanged?.Invoke(_currentTimeHours);

            if (_currentTickHoursRemaining <= 0)
            {
                _hoursInCurrentTickPassed = 0;
                EndTick();
            }
        }

        public void EndShift()
        {
            OnShiftEnded?.Invoke();

            if (_nightDurationHours > 0f)
            {
                StartNight();
            }
            else
            {
                StartNewDay();
            }
        }

        private void StartNewDay()
        {
            _currentDay++;
            _currentTimeHours = _dayStartHour;
            _ticksRemainingToday = CalculateMaxTicks();

            OnDayChanged?.Invoke(_currentDay);
            OnTicksChanged?.Invoke(_ticksRemainingToday);
            OnTimeChanged?.Invoke(_currentTimeHours);

            Debug.Log($"[GameTime] Day {_currentDay} started. Ticks: {_ticksRemainingToday}");
        }

        private void StartNight()
        {
            if (_isNightActive) return;

            _isNightActive = true;
            _nightHoursRemaining = _nightDurationHours;
            _currentTimeHours = 0f;

            float targetScale = _realSecondsPerGameHour / Mathf.Max(0.01f, _tickSpeedRealSeconds);
            float visualScale = Mathf.Min(100f, targetScale);

            Time.timeScale = visualScale;
            Time.maximumDeltaTime = Mathf.Max(_originalMaxDeltaTime, visualScale * 0.05f);

            OnNightStarted?.Invoke();
            OnTimeChanged?.Invoke(_currentTimeHours);
            Debug.Log("[GameTime] Night started.");
        }

        private void EndNight()
        {
            if (!_isNightActive) return;

            _isNightActive = false;
            Time.timeScale = 1f;
            Time.maximumDeltaTime = _originalMaxDeltaTime;

            OnNightEnded?.Invoke();
            Debug.Log("[GameTime] Night ended.");
            StartNewDay();
        }

        private void ProcessNightTime(float gameHoursPassed)
        {
            float actualPassed = Mathf.Min(gameHoursPassed, _nightHoursRemaining);

            _nightHoursRemaining -= actualPassed;
            _currentTimeHours += actualPassed;

            OnNightTimeAdvanced?.Invoke(actualPassed);
            OnTimeChanged?.Invoke(_currentTimeHours);

            if (_nightHoursRemaining <= 0f)
            {
                EndNight();
            }
        }

        public void SetResidentCount(int count)
        {
            int oldMax = CalculateMaxTicks();
            _residentCount = Mathf.Max(0, count);
            int newMax = CalculateMaxTicks();

            int diff = newMax - oldMax;
            if (diff != 0)
            {
                _ticksRemainingToday = Mathf.Max(0, _ticksRemainingToday + diff);
            }

            OnTicksChanged?.Invoke(_ticksRemainingToday);
        }

        public void AddResident(int count = 1)
        {
            SetResidentCount(_residentCount + count);
        }

        private int CalculateMaxTicks()
        {
            int ticks = _baseTicksPerShift + (_residentCount * _ticksPerResident);
            return Mathf.Min(ticks, _maxTicksPerShift);
        }

        public float GameHoursToRealSeconds(float gameHours)
        {
            return gameHours * _realSecondsPerGameHour;
        }

        public float RealSecondsToGameHours(float realSeconds)
        {
            return realSeconds / _realSecondsPerGameHour;
        }

        public string GetFormattedTime()
        {
            float timeOfDay = Mathf.Repeat(_currentTimeHours, 24f);
            int hours = Mathf.FloorToInt(timeOfDay);
            int minutes = Mathf.FloorToInt((timeOfDay - hours) * 60);
            return $"{hours:D2}:{minutes:D2}";
        }

        public void ForceStopTick()
        {
            if (_isTickActive)
            {
                _currentTickHoursRemaining = 0;
                EndTick();
            }
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}

