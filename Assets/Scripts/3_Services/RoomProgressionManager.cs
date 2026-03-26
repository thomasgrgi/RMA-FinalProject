using UnityEngine;
using ARVRMultiplayer.Controllers;

namespace ARVRMultiplayer.Services
{
    public class RoomProgressionManager : MonoBehaviour
    {
        public static RoomProgressionManager Instance { get; private set; }

        [Header("Rooms — drag them in order, starting from Room 0")]
        [SerializeField] private RoomManager[] rooms;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable() => GameEvents.OnRoomSolved += HandleRoomSolved;
        private void OnDisable() => GameEvents.OnRoomSolved -= HandleRoomSolved;

        private void Start()
        {
            // Activate the first room automatically
            if (rooms.Length > 0)
                rooms[0].ActivateRoom();
        }

        private void HandleRoomSolved(int solvedIndex)
        {
            int next = solvedIndex + 1;

            if (next < rooms.Length)
            {
                rooms[next].ActivateRoom();
                Debug.Log($"[Progression] Room {solvedIndex} done → activating room {next}");
            }
            else
            {
                Debug.Log("[Progression] All rooms complete!");
                // TODO: trigger your win / end screen here
            }
        }
    }
}