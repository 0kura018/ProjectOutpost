using UnityEngine;
using BuildingSystem;
using System.Collections.Generic;
using System.Linq;

namespace BaseSystem
{

    public class NonCombatSpawner : MonoBehaviour
    {
        private static NonCombatSpawner _instance;
        public static NonCombatSpawner Instance => _instance;

        [Header("Team Configuration")]
        [SerializeField] private NonCombatTeam _team;

        [Header("Spawn Settings")]
        [Tooltip("Спавнить юнитов при завершении первой комнаты")]
        [SerializeField] private bool _spawnOnFirstRoom = true;

        [Header("Debug")]
        [SerializeField] private List<NonCombatUnit> _spawnedUnits = new List<NonCombatUnit>();
        public IReadOnlyList<NonCombatUnit> SpawnedUnits => _spawnedUnits;

        private bool _hasSpawned = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            if (_spawnOnFirstRoom)
            {

                SubscribeToRoomEvents();

                if (BuildingManager.Instance != null)
                {

                    CheckExistingRooms();
                }
            }

            SubscribeToNightEvents();
        }

        private void Update()
        {

            if (_spawnOnFirstRoom && !_hasSpawned)
            {
                CheckForNewRooms();
            }
        }

        private void SubscribeToRoomEvents()
        {

            var existingRooms = FindObjectsOfType<Room>();
            foreach (var room in existingRooms)
            {
                SubscribeToRoom(room);
            }
        }

        private void SubscribeToRoom(Room room)
        {
            room.BuildCompleted += () => OnRoomCompleted(room);
            Debug.Log($"[NonCombatSpawner] Subscribed to room {room.Config?.RoomName ?? "Unknown"}");
        }

        private void CheckExistingRooms()
        {
            var completedRooms = FindObjectsOfType<Room>()
                .Where(r => r.State == RoomState.Completed)
                .ToList();

            if (completedRooms.Count > 0 && !_hasSpawned)
            {
                Debug.Log($"[NonCombatSpawner] Found {completedRooms.Count} completed rooms at start. Spawning team...");
                OnRoomCompleted(completedRooms[0]);
            }
        }

        private List<Room> _checkedRooms = new List<Room>();

        private void CheckForNewRooms()
        {
            var allRooms = FindObjectsOfType<Room>();

            foreach (var room in allRooms)
            {
                if (!_checkedRooms.Contains(room))
                {
                    _checkedRooms.Add(room);
                    SubscribeToRoom(room);

                    if (room.State == RoomState.Completed && !_hasSpawned)
                    {
                        OnRoomCompleted(room);
                    }
                }
            }
        }

        private void SubscribeToNightEvents()
        {
            if (TimeSystem.GameTimeManager.Instance == null) return;

            TimeSystem.GameTimeManager.Instance.OnNightStarted += HandleNightStarted;
            TimeSystem.GameTimeManager.Instance.OnNightEnded += HandleNightEnded;
        }

        private void HandleNightStarted()
        {
            foreach (var unit in _spawnedUnits)
            {
                if (unit != null)
                {
                    unit.StartNightSleep();
                }
            }
        }

        private void HandleNightEnded()
        {
            foreach (var unit in _spawnedUnits)
            {
                if (unit != null)
                {
                    unit.EndNightSleep();
                }
            }
        }

        private void OnRoomCompleted(Room room)
        {
            Debug.Log($"[NonCombatSpawner] OnRoomCompleted called for room: {room.Config?.RoomName ?? "Unknown"}");

            if (_hasSpawned)
            {
                Debug.Log("[NonCombatSpawner] Already spawned, ignoring.");
                return;
            }

            if (_team == null)
            {
                Debug.LogError("[NonCombatSpawner] Team is null!");
                return;
            }

            if (_team.Members == null || _team.Members.Count == 0)
            {
                Debug.LogWarning($"[NonCombatSpawner] Team '{_team.name}' has no members!");
                return;
            }

            Debug.Log($"[NonCombatSpawner] First room completed: {room.Config.RoomName}. Spawning {_team.Members.Count} team members...");

            SpawnTeam(room);
            _hasSpawned = true;
        }

        public void SpawnTeam(Room room)
        {
            if (_team == null || room == null) return;

            foreach (var member in _team.Members)
            {
                SpawnMember(member, room);
            }

            Debug.Log($"[NonCombatSpawner] Spawned {_team.Members.Count} team members in {room.Config.RoomName}");
        }

        public NonCombatUnit SpawnMember(NonCombatTeamMember member, Room room)
        {
            if (member == null || member.NonCombatPrefab == null)
            {
                Debug.LogError($"[NonCombatSpawner] Member or prefab is null!");
                return null;
            }

            Vector3 spawnPos = room.transform.position;
            GameObject unitObj = Instantiate(member.NonCombatPrefab, spawnPos, Quaternion.identity);
            unitObj.name = $"NonCombat_{member.MemberName}";

            var unit = unitObj.GetComponent<NonCombatUnit>();
            if (unit == null)
            {
                unit = unitObj.AddComponent<NonCombatUnit>();
            }

            unit.Initialize(member, room);

            if (TimeSystem.GameTimeManager.Instance != null && TimeSystem.GameTimeManager.Instance.IsNightActive)
            {
                unit.StartNightSleep();
            }
            _spawnedUnits.Add(unit);

            if (TimeSystem.GameTimeManager.Instance != null)
            {
                TimeSystem.GameTimeManager.Instance.SetResidentCount(_spawnedUnits.Count);
            }

            Debug.Log($"[NonCombatSpawner] Spawned {member.MemberName} in {room.Config.RoomName}");

            return unit;
        }

        [ContextMenu("Spawn Team Manually")]
        public void SpawnTeamManually()
        {

            var completedRooms = FindObjectsOfType<Room>()
                .Where(r => r.State == RoomState.Completed)
                .ToList();

            if (completedRooms.Count == 0)
            {
                Debug.LogWarning("[NonCombatSpawner] No completed rooms found!");
                return;
            }

            SpawnTeam(completedRooms[0]);
        }

        [ContextMenu("Clear All Units")]
        public void ClearAllUnits()
        {
            foreach (var unit in _spawnedUnits)
            {
                if (unit != null)
                {
                    Destroy(unit.gameObject);
                }
            }

            _spawnedUnits.Clear();
            _hasSpawned = false;

            if (TimeSystem.GameTimeManager.Instance != null)
            {
                TimeSystem.GameTimeManager.Instance.SetResidentCount(0);
            }

            Debug.Log("[NonCombatSpawner] Cleared all units");
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }

            if (TimeSystem.GameTimeManager.Instance != null)
            {
                TimeSystem.GameTimeManager.Instance.OnNightStarted -= HandleNightStarted;
                TimeSystem.GameTimeManager.Instance.OnNightEnded -= HandleNightEnded;
            }
        }
    }
}

