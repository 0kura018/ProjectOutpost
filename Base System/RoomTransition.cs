using UnityEngine;
using BuildingSystem;

namespace BaseSystem
{
    public enum TransitionType
    {
        HorizontalDoor,
        Elevator
    }

    public class RoomTransition : MonoBehaviour
    {
        public Room From;
        public Room To;
        public TransitionType Type = TransitionType.HorizontalDoor;

        public Transform EntryPoint;
        public Transform ExitPoint;

        public float ElevatorTravelDuration = 0.8f;

        public bool Connects(Room a, Room b)
        {
            return (From == a && To == b) || (From == b && To == a);
        }

        public Transform GetEntryFor(Room from)
        {
            if (From == from) return EntryPoint;
            return ExitPoint;
        }

        public Transform GetExitFor(Room to)
        {
            if (To == to) return ExitPoint;
            return EntryPoint;
        }
    }
}
