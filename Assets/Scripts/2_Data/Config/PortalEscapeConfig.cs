using UnityEngine;

namespace PortalEscape.Data
{
    public enum AppRole { VR_Host, AR_Client }

    [CreateAssetMenu(fileName = "PortalEscapeConfig", menuName = "XR/Portal Escape Config")]
    public class PortalEscapeConfig : ScriptableObject
    {
        [Header("Network Settings")]
        public AppRole currentRole = AppRole.VR_Host;
        public string serverIP = "127.0.0.1"; // À remplacer par l'IP locale du Quest 2 en test
        
        [Header("Debug")]
        public bool showDebugOverlay = true;
        
        [Header("Environment")]
        public float defaultWallMoveSpeed = 2.0f;
    }
}