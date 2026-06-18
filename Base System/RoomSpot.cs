using UnityEngine;

namespace BaseSystem
{
    public class RoomSpot : MonoBehaviour
    {
        [SerializeField] private RoomSpotType _type;
        public RoomSpotType Type => _type;

        public bool IsOccupied { get; private set; }

        public void SetOccupied(bool occupied)
        {
            IsOccupied = occupied;
        }
    }
}
