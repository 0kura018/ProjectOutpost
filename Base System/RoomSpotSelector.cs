using System.Collections.Generic;
using BuildingSystem;

namespace BaseSystem
{
    public static class RoomSpotSelector
    {

        public static RoomSpot SelectSpot(Room room, RoomSpotType type)
        {
            RoomSpot fallback = null;

            foreach (var spot in room.Spots)
            {
                if (spot == null) continue;
                if (spot.IsOccupied) continue;
                if (spot.Type == type) return spot;
                fallback ??= spot;
            }

            return fallback;
        }

        public static RoomSpot SelectSpotStrict(Room room, RoomSpotType type)
        {
            if (room == null) return null;

            foreach (var spot in room.Spots)
            {
                if (spot == null) continue;
                if (spot.IsOccupied) continue;
                if (spot.Type == type) return spot;
            }

            return null;
        }
    }
}
