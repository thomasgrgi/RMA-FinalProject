using Unity.Netcode;
using UnityEngine;

namespace PortalEscape.Services
{
    /// <summary>
    /// Synchronise la position du téléphone (AR) sur le réseau pour que le joueur VR puisse le voir.
    /// </summary>
    public class ARAvatarSync : NetworkBehaviour
    {
        [Header("Élément traqué par l'AR (Source locale)")]
        [Tooltip("Si vide, le script utilisera automatiquement Camera.main au démarrage.")]
        public Transform arCamera;

        [Header("Élément visuel (Ce que voit le joueur VR)")]
        public Transform visualDevice;

        // Variables réseau (Seul le téléphone - Owner - écrit dedans)
        private NetworkVariable<Vector3> devicePos = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<Quaternion> deviceRot = new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Owner);

        private void Awake()
        {
            // Au démarrage, on cache le modèle 3D du téléphone pour le joueur AR
            // (Il ne doit pas voir un faux téléphone en plein milieu de son écran !)
            if (visualDevice != null) visualDevice.gameObject.SetActive(false);
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                // Si on est le téléphone, on cherche automatiquement la caméra AR locale si elle n'est pas assignée
                if (arCamera == null && Camera.main != null)
                {
                    arCamera = Camera.main.transform;
                }
            }
            else
            {
                // Si on est le casque VR (pas le propriétaire de cet objet), on affiche l'avatar AR
                if (visualDevice != null) visualDevice.gameObject.SetActive(true);
            }
        }

        void Update()
        {
            // L'Update est toléré ici car il s'agit d'un besoin continu (interpolation visuelle)
            if (!IsSpawned) return;

            if (IsOwner)
            {
                // Le téléphone lit sa position réelle et l'envoie sur le réseau
                if (arCamera != null)
                {
                    devicePos.Value = transform.InverseTransformPoint(arCamera.position);
                    deviceRot.Value = Quaternion.Inverse(transform.rotation) * arCamera.rotation;
                }
            }
            else
            {
                // Le casque VR lit les valeurs réseau et déplace l'avatar visuel
                if (visualDevice != null)
                {
                    visualDevice.localPosition = devicePos.Value;
                    visualDevice.localRotation = deviceRot.Value;
                }
            }
        }
    }
}