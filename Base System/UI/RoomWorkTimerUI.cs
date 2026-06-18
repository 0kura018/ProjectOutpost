using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BuildingSystem;

namespace BaseSystem
{

    public class RoomWorkTimerUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private TextMeshProUGUI _workerNameText;
        [SerializeField] private TextMeshProUGUI _workNameText;
        [SerializeField] private TextMeshProUGUI _ticksText;
        [SerializeField] private TextMeshProUGUI _timeLeftText;
        [SerializeField] private TextMeshProUGUI _resourcesText;

        private Room _room;
        private RoomWorkHandler _workHandler;

        public void Initialize(Room room)
        {
            _room = room;
            _workHandler = room.GetComponent<RoomWorkHandler>();

            if (_workHandler != null)
            {
                _workHandler.OnWorkerAssigned += UpdateWorkerInfo;
                UpdateWorkerInfo(_workHandler.CurrentWorker);

                if (_workNameText != null && room.Config != null)
                {
                    _workNameText.text = string.IsNullOrEmpty(room.Config.WorkName) ? room.Config.RoomName : room.Config.WorkName;
                }

                if (_resourcesText != null && room.Config != null)
                {
                    _resourcesText.text = $"+{room.Config.ProducedAmount} {room.Config.ProducedResource}";
                }
            }
        }

        private void Update()
        {
            if (_workHandler == null || !_workHandler.IsWorking) return;

            float smoothProgress = _workHandler.SmoothProgress;
            if (_progressSlider != null) _progressSlider.value = smoothProgress;
            if (_progressText != null) _progressText.text = $"{smoothProgress * 100f:F0}%";

            if (_ticksText != null)
            {
                _ticksText.text = $"{_workHandler.TicksRemaining} тактов";
            }

            if (_timeLeftText != null)
            {
                float hoursLeft = _workHandler.RemainingGameHours;
                if (hoursLeft < 1f)
                {
                    _timeLeftText.text = $"{hoursLeft * 60f:F0}м";
                }
                else
                {
                    _timeLeftText.text = $"{hoursLeft:F1}ч";
                }
            }
        }

        private void UpdateWorkerInfo(NonCombatUnit worker)
        {
            if (_workerNameText != null)
            {
                _workerNameText.text = worker != null ? worker.name : "Waiting...";
            }
        }

        private void OnDestroy()
        {
            if (_workHandler != null)
            {
                _workHandler.OnWorkerAssigned -= UpdateWorkerInfo;
            }
        }
    }
}
