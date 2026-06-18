using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace BuildingSystem
{

    public class BuildingManager : MonoBehaviour
    {
        private static BuildingManager _instance;
        public static BuildingManager Instance => _instance;

        private List<Room> _allRooms = new List<Room>();
        public IReadOnlyList<Room> AllRooms => _allRooms;

        public List<Room> GetCompletedRooms()
        {
            return _allRooms.Where(r => r != null && r.State == RoomState.Completed).ToList();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        public int GetRoomCountByType(RoomConfig config)
        {
            if (config == null) return 0;

            int count = 0;
            foreach (var room in _allRooms)
            {
                if (room != null && room.Config == config)
                {
                    count++;
                }
            }

            return count;
        }

        public void RegisterRoom(Room room)
        {
            if (room == null) return;

            if (!_allRooms.Contains(room))
            {
                _allRooms.Add(room);
                Debug.Log($"[BuildingManager] Registered room '{room.Config.RoomName}' at {room.GridPosition}");
            }
        }

        public void UnregisterRoom(Room room)
        {
            if (_allRooms.Contains(room))
            {
                _allRooms.Remove(room);
                Debug.Log($"[BuildingManager] Unregistered room '{room.Config.RoomName}' at {room.GridPosition}");
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
