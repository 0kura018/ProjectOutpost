using UnityEngine;
using System.Collections.Generic;
using BuildingSystem;

public class RoomBuildUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Canvas _mainCanvas;
    [SerializeField] private RoomBuildTimerUI _timerPrefab;
    [SerializeField] private Vector3 _timerOffset = new Vector3(0, 1.5f, 0);

    private Dictionary<Room, RoomBuildTimerUI> _activeTimers = new Dictionary<Room, RoomBuildTimerUI>();

    private void Awake()
    {
        if (_mainCanvas == null)
        {
            _mainCanvas = GetComponent<Canvas>();
            if (_mainCanvas == null)
            {
                Debug.LogError("[RoomBuildUI] Canvas не найден!");
            }
        }
    }

    public void ShowTimerForRoom(Room room)
    {
        if (room == null)
        {
            Debug.LogWarning("[RoomBuildUI] Попытка создать таймер для null комнаты");
            return;
        }

        if (_activeTimers.ContainsKey(room))
        {
            Debug.LogWarning($"[RoomBuildUI] Таймер для {room.name} уже существует");
            return;
        }

        if (_timerPrefab == null)
        {
            Debug.LogError("[RoomBuildUI] Timer Prefab не назначен!");
            return;
        }

        RoomBuildTimerUI timer = Instantiate(_timerPrefab, _mainCanvas.transform);
        timer.Initialize(room);

        UpdateTimerPosition(room, timer);

        _activeTimers.Add(room, timer);

        Debug.Log($"[RoomBuildUI] Таймер создан для {room.name}");
    }

    public void HideTimerForRoom(Room room)
    {
        if (_activeTimers.TryGetValue(room, out RoomBuildTimerUI timer))
        {
            if (timer != null)
            {
                Destroy(timer.gameObject);
            }
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

        List<Room> roomsToRemove = new List<Room>();

        foreach (var kvp in _activeTimers)
        {
            Room room = kvp.Key;
            RoomBuildTimerUI timer = kvp.Value;

            if (room == null || timer == null)
            {
                roomsToRemove.Add(room);
                continue;
            }

            UpdateTimerPosition(room, timer);
        }

        foreach (var room in roomsToRemove)
        {
            _activeTimers.Remove(room);
        }
    }

    private void UpdateTimerPosition(Room room, RoomBuildTimerUI timer)
    {
        if (Camera.main == null) return;

        Vector3 worldPos = room.transform.position + _timerOffset;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        timer.transform.position = screenPos;
    }

    private void OnDestroy()
    {

        foreach (var timer in _activeTimers.Values)
        {
            if (timer != null)
            {
                Destroy(timer.gameObject);
            }
        }
        _activeTimers.Clear();
    }
}
