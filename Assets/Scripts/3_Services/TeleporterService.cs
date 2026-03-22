using UnityEngine;

/// <summary>
/// Attach to the Teleporter object (the pad / zone in the world).
///
/// HOW IT WORKS
/// ─────────────
/// 1.  The TeleporterActivator fires OnTeleporterActivate → call SetActive(true).
///     Wire this in the Inspector, or call it directly from code.
/// 2.  The player steps on the pad (OnTriggerEnter detects the player).
/// 3.  While the player is on the pad, teleports the player to 'destination'.
///
/// SETUP
/// ─────
/// • Give this GameObject a Collider (set Is Trigger = true).
/// • Assign the 'destination' Transform in the Inspector.
/// • Assign the 'playerRig' Transform (your XR Origin / Camera Rig root).
/// • Wire TeleporterActivator.OnTeleporterActivate  → this.SetActive(true)
///   and  TeleporterActivator.OnTeleporterDeactivate → this.SetActive(false)
///   in the Inspector (or call them from code).
/// </summary>
[RequireComponent(typeof(Collider))]
public class TeleporterService : MonoBehaviour
{
    // ── Inspector fields ──────────────────────────────────────────────────
    [Header("Teleport Destination")]
    [Tooltip("Where the player will be sent.")]
    [SerializeField] private Transform destination;

    [Header("Player Reference")]
    [Tooltip("The XR Origin / Camera Rig root GameObject.")]
    [SerializeField] private Transform playerRig;

    [Header("Settings")]
    [Tooltip("Enable this to test teleporting without needing the Activator events wired up.")]
    [SerializeField] private bool startActive = false;

    [Header("Visual Feedback (optional)")]
    [Tooltip("Child objects to show/hide when the pad is active.")]
    [SerializeField] private GameObject[] activeVisuals;

    // ── Private state ─────────────────────────────────────────────────────
    private bool _isActive = false;
    private bool _playerOnPad = false;
    private bool _onCooldown = false;
    private float _cooldown = 1f;   // seconds before pad can fire again

    // ─────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        _isActive = startActive;
        SetVisuals(_isActive);
    }

    private void Update()
    {
        // Catches the case where the player is already standing on the pad
        // when the cooldown expires — OnTriggerEnter won't re-fire in that case
        if (_isActive && _playerOnPad && !_onCooldown)
            DoTeleport();
    }
    private void DoTeleport()
    {
        if (destination == null)
        {
            Debug.LogError("[Teleporter] Destination is NOT assigned in the Inspector!", this);
            return;
        }
        if (playerRig == null)
        {
            Debug.LogError("[Teleporter] Player Rig is NOT assigned in the Inspector!", this);
            return;
        }

        // CharacterController blocks transform.position — disable, move, re-enable
        CharacterController cc = playerRig.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        playerRig.position = destination.position;
        playerRig.rotation = destination.rotation;

        if (cc != null) cc.enabled = true;

        Debug.Log($"[Teleporter] Player teleported to '{destination.name}' at {destination.position}");
        StartCoroutine(TeleportCooldown());
    }

    private System.Collections.IEnumerator TeleportCooldown()
    {
        _onCooldown = true;
        _playerOnPad = false;               // reset so Update doesn't re-fire immediately
        GetComponent<Collider>().enabled = false;   // block ALL colliders on the XR rig
        yield return new WaitForSeconds(_cooldown);
        GetComponent<Collider>().enabled = true;
        _onCooldown = false;
    }

    // ── Pad activation (called by TeleporterActivator events) ─────────────
    /// <summary>Activate this teleport pad.</summary>
    public void SetActive(bool active)
    {
        _isActive = active;
        SetVisuals(active);
        Debug.Log($"[Teleporter] Pad '{name}' is now {(active ? "ACTIVE" : "INACTIVE")}");
    }

    // ── Trigger detection ─────────────────────────────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other))
        {
            _playerOnPad = true;
            Debug.Log($"[Teleporter] Player stepped ON pad. _isActive = {_isActive}");
            if (_isActive && !_onCooldown)
                DoTeleport();
            else if (!_isActive)
                Debug.LogWarning("[Teleporter] Pad is NOT active — wire TeleporterActivator.OnTeleporterActivate → Teleporter.SetActive(true), or tick 'Start Active' in the Inspector.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other))
        {
            _playerOnPad = false;
            Debug.Log("[Teleporter] Player stepped OFF pad.");
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    private bool IsPlayer(Collider other)
    {
        // Matches by tag (default Unity tag) or by reference to the rig.
        return other.CompareTag("Player") ||
               (playerRig != null && other.transform.IsChildOf(playerRig));
    }

    private void SetVisuals(bool show)
    {
        foreach (var go in activeVisuals)
            if (go != null) go.SetActive(show);
    }

#if UNITY_EDITOR
    // Draw destination gizmo in the editor
    private void OnDrawGizmosSelected()
    {
        if (destination == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(destination.position, 0.15f);
        Gizmos.DrawLine(transform.position, destination.position);
    }
#endif
}