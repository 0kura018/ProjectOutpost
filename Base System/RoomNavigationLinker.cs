using UnityEngine;
using BuildingSystem;
using BaseSystem;
using System.Collections.Generic;
using System.Linq;

namespace BaseSystem
{

    public class RoomNavigationLinker : MonoBehaviour
    {
        public static RoomNavigationLinker Instance { get; private set; }

        private readonly List<Room> _processedRooms = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void Update()
        {

            var allRooms = Object.FindObjectsByType<Room>(FindObjectsSortMode.None);

            foreach (var room in allRooms)
            {
                if (!_processedRooms.Contains(room))
                {
                    _processedRooms.Add(room);

                    room.BuildCompleted += () => LinkRoom(room);

                    if (room.State == RoomState.Completed)
                    {
                        LinkRoom(room);
                    }
                }
            }
        }

        public void LinkRoom(Room newRoom)
        {
            if (newRoom == null || BuildingGrid.Instance == null) return;

            var completedRooms = Object.FindObjectsByType<Room>(FindObjectsSortMode.None)
                .Where(r => r != newRoom && r.State == RoomState.Completed);

            foreach (var other in completedRooms)
            {
                if (AreRoomsAdjacent(newRoom, other))
                {
                    CreateTransition(newRoom, other);
                }
            }
        }

        private bool AreRoomsAdjacent(Room a, Room b)
        {
            var posA = a.GridPosition;
            var posB = b.GridPosition;
            var configA = a.Config;
            var configB = b.Config;

            if (configA == null || configB == null) return false;

            if (posA.x + configA.Width == posB.x)
            {
                return HasMatchingDoors(configA.RightConnections, posA.y, configB.LeftConnections, posB.y);
            }

            if (posB.x + configB.Width == posA.x)
            {
                return HasMatchingDoors(configB.RightConnections, posB.y, configA.LeftConnections, posA.y);
            }

            if (posA.y + configA.Height == posB.y)
            {
                return HasMatchingDoors(configA.TopConnections, posA.x, configB.BottomConnections, posB.x);
            }

            if (posB.y + configB.Height == posA.y)
            {
                return HasMatchingDoors(configB.TopConnections, posB.x, configA.BottomConnections, posA.x);
            }

            return false;
        }

        private bool HasMatchingDoors(int[] connectionsA, int baseA, int[] connectionsB, int baseB)
        {
            if (connectionsA == null || connectionsB == null) return false;

            foreach (int i in connectionsA)
            {
                int worldA = baseA + i;
                foreach (int j in connectionsB)
                {
                    int worldB = baseB + j;
                    if (worldA == worldB) return true;
                }
            }
            return false;
        }

        private void CreateTransition(Room a, Room b)
        {
            if (a.GetTransitionTo(b) != null) return;

            GameObject go = new GameObject($"Transition_{a.Config.RoomName}_{b.Config.RoomName}");
            go.transform.SetParent(a.transform);

            var transition = go.AddComponent<RoomTransition>();
            transition.From = a;
            transition.To = b;

            var posA = a.GridPosition;
            var posB = b.GridPosition;
            var configA = a.Config;
            var configB = b.Config;

            if (posA.x + configA.Width == posB.x || posB.x + configB.Width == posA.x)
            {
                transition.Type = TransitionType.HorizontalDoor;
            }
            else
            {
                transition.Type = TransitionType.Elevator;
            }

            transition.EntryPoint = a.transform;
            transition.ExitPoint = b.transform;

            a.Transitions.Add(transition);
            b.Transitions.Add(transition);

            Debug.Log($"[Navigation] Linked {a.Config.RoomName} <-> {b.Config.RoomName} ({transition.Type})");
        }
    }
}