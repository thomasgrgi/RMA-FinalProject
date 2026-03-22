using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Attach to any object (e.g. a trigger zone, button, or manager) that
/// should activate / deactivate teleporters in the scene.
/// Call Activate() / Deactivate() from inspector events, other scripts,
/// animations, or directly wire them to XR Interactable events.
/// </summary>
public class TeleporterActivatorController : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent OnTeleporterActivate;
    public UnityEvent OnTeleporterDeactivate;

    // ── Optional: auto-activate on start ──────────────────────────────────
    [Header("Settings")]
    [SerializeField] private bool activateOnStart = false;

    private bool _isActive = false;
    public bool IsActive => _isActive;

    // ─────────────────────────────────────────────────────────────────────
    private void Start()
    {
        if (activateOnStart)
            Activate();
    }

    // ─────────────────────────────────────────────────────────────────────
    /// <summary>Fires the "Teleporter Activate" event.</summary>
    public void Activate()
    {
        if (_isActive) return;           // already on – no double-fire
        _isActive = true;
        Debug.Log("[TeleporterActivator] Teleporter ACTIVATED");
        OnTeleporterActivate?.Invoke();
    }

    /// <summary>Fires the "Teleporter Deactivate" event.</summary>
    public void Deactivate()
    {
        if (!_isActive) return;          // already off
        _isActive = false;
        Debug.Log("[TeleporterActivator] Teleporter DEACTIVATED");
        OnTeleporterDeactivate?.Invoke();
    }

    /// <summary>Convenience toggle – useful for a single button/event.</summary>
    public void Toggle()
    {
        if (_isActive) Deactivate();
        else Activate();
    }
}