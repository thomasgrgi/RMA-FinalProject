using System;

namespace ARVRMultiplayer.Services
{
    public static class GameEvents
    {
        public static event Action<int> OnRoomSolved;

        public static void RoomSolved(int roomIndex)
        {
            OnRoomSolved?.Invoke(roomIndex);
        }
    }
}