using UnityEngine;
using Unity.Netcode;

namespace ARVRMultiplayer.Services
{
    /// <summary>
    /// Service : applique une rotation de +90° au Rig_AR sur l'axe Y
    /// lorsque le joueur VR utilise un téléporteur.
    /// La rotation s'applique sur XR Origin pour ne pas interférer
    /// avec le tracking physique du téléphone.
    /// </summary>
    public class ARRotationService : NetworkBehaviour
    {
        [Header("Références - Injectées via Inspecteur")]
        [SerializeField] private Transform _xrOrigin; // Rig_AR > XR Origin

        private const float RotationAngle = 90f;

        private void Awake()
        {
            ValidateDependencies();
        }

        /// <summary>
        /// Appelée par le téléporteur VR pour déclencher la rotation AR.
        /// </summary>
        public void ApplyRotationToAR()
        {
            if (IsServer)
            {
                RotateARClientRpc();
            }
            else
            {
                ApplyRotationServerRpc();
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void ApplyRotationServerRpc()
        {
            RotateARClientRpc();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void RotateARClientRpc()
        {
            // On applique la rotation uniquement sur le client AR
            // Le client VR ignore cet appel car il n'a pas de _xrOrigin AR
            if (_xrOrigin == null)
            {
                Debug.LogWarning("[ARRotationService] _xrOrigin null sur ce client — ignoré.");
                return;
            }

            Vector3 currentRotation = _xrOrigin.eulerAngles;
            _xrOrigin.eulerAngles = new Vector3(
                currentRotation.x,
                currentRotation.y + RotationAngle,
                currentRotation.z
            );

            Debug.Log($"[ARRotationService] Rotation AR appliquée : {_xrOrigin.eulerAngles.y}°");
        }

        private void ValidateDependencies()
        {
            if (_xrOrigin == null)
                Debug.LogError("[ARRotationService] _xrOrigin non assigné dans l'inspecteur !");
        }
    }
}