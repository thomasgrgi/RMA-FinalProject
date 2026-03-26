using UnityEngine;
using ARVRMultiplayer.Services;

namespace ARVRMultiplayer.Controllers
{
    public class RoomManager : MonoBehaviour
    {
        [Header("Room Identity")]
        [SerializeField] private int roomIndex;
        [SerializeField] private string roomName;

        [Header("References")]
        [SerializeField] private TeleporterActivatorController teleporter;

        private bool _isSolved = false;
        private bool _isActive = false;

        // ── Called by RoomProgressionManager ─────────────────────────────
        public void ActivateRoom()
        {
            _isActive = true;
            Debug.Log($"[RoomManager] '{roomName}' is now active.");
            // Optional: light up keypad, play a sound, etc.
        }

        // ── Wire this to KeyPadService._onSuccess in the Inspector ────────
        public void OnPuzzleSolved()
        {
            if (_isSolved) return;
            _isSolved = true;

            teleporter.Activate();
            GameEvents.RoomSolved(roomIndex);

            Debug.Log($"[RoomManager] '{roomName}' solved!");
        }
    }
}