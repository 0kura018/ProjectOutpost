using UnityEngine;
using System;
using TimeSystem;
using BaseSystem;

namespace BuildingSystem
{

    public class RoomWorkHandler : MonoBehaviour
    {
        private Room _room;
        private NonCombatUnit _currentWorker;
        private float _hoursRemaining;
        private bool _isWorking;

        public event Action<float> OnProgressChanged;
        public event Action OnWorkCompleted;
        public event Action<NonCombatUnit> OnWorkerAssigned;

        public NonCombatUnit CurrentWorker => _currentWorker;
        public bool IsWorking => _isWorking;

        public int TicksRemaining
        {
            get
            {
                if (!_isWorking || _room.Config == null || GameTimeManager.Instance == null) return 0;

                float offset = GameTimeManager.Instance.CompletionOffset;
                if (_hoursRemaining <= offset) return 0;

                float hoursPerTick = GameTimeManager.Instance.HoursPerTick;
                return Mathf.CeilToInt(_hoursRemaining / hoursPerTick);
            }
        }

        public float Progress => (_room.Config != null && _room.Config.WorkTimeHours > 0)
            ? 1f - _hoursRemaining / _room.Config.WorkTimeHours
            : 0f;

        public float SmoothProgress
        {
            get
            {
                if (!_isWorking || _room.Config == null || _room.Config.WorkTimeHours <= 0) return 0f;
                return Mathf.Clamp01(1f - _hoursRemaining / _room.Config.WorkTimeHours);
            }
        }

        public float RemainingGameHours
        {
            get
            {
                if (!_isWorking || _room.Config == null) return 0f;
                return Mathf.Max(0, _hoursRemaining);
            }
        }

        private void Awake()
        {
            _room = GetComponent<Room>();
        }

        private void OnEnable()
        {
            if (GameTimeManager.Instance != null)
            {
                GameTimeManager.Instance.OnNightTimeAdvanced += HandleNightTimeAdvanced;
            }
        }

        private void OnDisable()
        {
            if (GameTimeManager.Instance != null)
            {
                GameTimeManager.Instance.OnNightTimeAdvanced -= HandleNightTimeAdvanced;
            }
        }

        private void Update()
        {
            if (!enabled || !_isWorking || _currentWorker == null || GameTimeManager.Instance == null) return;
            if (GameTimeManager.Instance.IsNightActive || GameTimeManager.Instance.IsTickActive)
            {

            }

            if (GameTimeManager.Instance.IsNightActive) return;

            float gameHoursPassed = GameTimeManager.Instance.CurrentGameHoursDelta;
            _hoursRemaining -= gameHoursPassed;

            OnProgressChanged?.Invoke(Progress);

            float offset = GameTimeManager.Instance.CompletionOffset;
            if (_hoursRemaining <= offset)
            {
                CompleteWork();
            }
        }

        private void HandleNightTimeAdvanced(float nightHours)
        {
            if (!enabled || !_isWorking || _room.Config == null) return;

            if (GameTimeManager.Instance == null || !GameTimeManager.Instance.NightPenaltyEnabled) return;

            float rate = GameTimeManager.Instance.NightPenaltyRatePerHour;
            if (rate <= 0f) return;

            float penalty = nightHours * rate;
            _hoursRemaining = Mathf.Min(_room.Config.WorkTimeHours, _hoursRemaining + penalty);
            OnProgressChanged?.Invoke(Progress);
        }

        public void AssignWorker(NonCombatUnit unit)
        {
            if (_room.Config == null || !_room.Config.CanProvideWork) return;
            if (_room.State != RoomState.Completed) return;

            if (_currentWorker == unit && unit != null) return;

            if (_currentWorker != null)
            {
                _currentWorker.ClearWorkAssignment();
            }

            _currentWorker = unit;

            if (_currentWorker != null)
            {
                _hoursRemaining = _room.Config.WorkTimeHours;
                _isWorking = true;

                _currentWorker.AssignToWork(_room);

                OnWorkerAssigned?.Invoke(_currentWorker);
                OnProgressChanged?.Invoke(Progress);

                if (RoomWorkTimerManagerUI.Instance != null)
                {
                    RoomWorkTimerManagerUI.Instance.ShowTimerForRoom(_room);
                }

                Debug.Log($"[RoomWork] Unit {unit.name} assigned to work in {_room.Config.RoomName}");
            }
            else
            {
                _isWorking = false;
                _hoursRemaining = 0;
                OnWorkerAssigned?.Invoke(null);
                OnProgressChanged?.Invoke(0);

                if (RoomWorkTimerManagerUI.Instance != null)
                {
                    RoomWorkTimerManagerUI.Instance.HideTimerForRoom(_room);
                }
            }
        }

        private void CompleteWork()
        {
            if (_room.Config == null) return;

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddResource(_room.Config.ProducedResource, _room.Config.ProducedAmount);
            }

            Debug.Log($"[RoomWork] Work cycle completed in {_room.Config.RoomName}. Produced {_room.Config.ProducedAmount} {_room.Config.ProducedResource}");

            _hoursRemaining = _room.Config.WorkTimeHours;
            OnWorkCompleted?.Invoke();
            OnProgressChanged?.Invoke(Progress);
        }

        private void OnDestroy()
        {
            if (_currentWorker != null)
            {
                _currentWorker.ClearWorkAssignment();
            }
        }
    }
}

