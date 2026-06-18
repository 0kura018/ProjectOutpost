using UnityEngine;
using System.Collections.Generic;
using BuildingSystem;

namespace BaseSystem
{

    public class RoomWorkTimerManagerUI : MonoBehaviour
    {
        public static RoomWorkTimerManagerUI Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private Canvas _mainCanvas;
        [SerializeField] private RoomWorkTimerUI _timerPrefab;
        [SerializeField] private Vector3 _timerOffset = new Vector3(0, 1.2f, 0);

        private Dictionary<Room, RoomWorkTimerUI> _activeTimers = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            if (_mainCanvas == null)
            {
                _mainCanvas = GetComponentInParent<Canvas>();
            }
        }

        public void ShowTimerForRoom(Room room)
        {
            if (room == null || _activeTimers.ContainsKey(room)) return;
            if (_timerPrefab == null || _mainCanvas == null) return;

            RoomWorkTimerUI timer = Instantiate(_timerPrefab, _mainCanvas.transform);
            timer.Initialize(room);
            _activeTimers.Add(room, timer);

            UpdateTimerPosition(room, timer);
        }

        public void HideTimerForRoom(Room room)
        {
            if (_activeTimers.TryGetValue(room, out RoomWorkTimerUI timer))
            {
                if (timer != null) Destroy(timer.gameObject);
                _activeTimers.Remove(room);
            }
        }

        private void Update()
        {
            UpdateAllTimerPositions();
        }

        private void UpdateAllTimerPositions()
        {
            if (Camera.main == null) return;

            List<Room> roomsToRemove = null;

            foreach (var kvp in _activeTimers)
            {
                Room room = kvp.Key;
                RoomWorkTimerUI timer = kvp.Value;

                if (room == null || timer == null)
                {
                    roomsToRemove ??= new List<Room>();
                    roomsToRemove.Add(room);
                    continue;
                }

                UpdateTimerPosition(room, timer);
            }

            if (roomsToRemove != null)
            {
                foreach (var r in roomsToRemove) _activeTimers.Remove(r);
            }
        }

        private void UpdateTimerPosition(Room room, RoomWorkTimerUI timer)
        {
            if (Camera.main == null) return;
            Vector3 worldPos = room.transform.position + _timerOffset;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            timer.transform.position = screenPos;
        }
    }
}
