using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System;

namespace ARVRMultiplayer.Controllers
{
    /// <summary>
    /// Détecte quand le joueur VR attrape ou lâche le uncolor_cube.
    /// Expose des événements pour découpler la logique.
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class CubeGrabHandler : MonoBehaviour
    {
        public event Action OnCubeGrabbed;
        public event Action OnCubeReleased;

        private XRGrabInteractable _grabInteractable;

        private void Awake()
        {
            _grabInteractable = GetComponent<XRGrabInteractable>();

            if (_grabInteractable == null)
            {
                Debug.LogError($"[CubeGrabHandler] XRGrabInteractable manquant sur {gameObject.name}");
                return;
            }

            _grabInteractable.selectEntered.AddListener(OnGrabbed);
            _grabInteractable.selectExited.AddListener(OnReleased);
        }

        private void OnDestroy()
        {
            if (_grabInteractable == null) return;
            _grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            _grabInteractable.selectExited.RemoveListener(OnReleased);
        }

        private void OnGrabbed(SelectEnterEventArgs args)
        {
            Debug.Log($"[CubeGrabHandler] {gameObject.name} attrapé par {args.interactorObject}");
    
            // Log ownership NGO
            var netObj = GetComponent<Unity.Netcode.NetworkObject>();
            if (netObj != null)
                Debug.Log($"[CubeGrabHandler] NetworkObject owner : ClientId={netObj.OwnerClientId} | IsOwner={netObj.IsOwner}");
            else
                Debug.LogWarning("[CubeGrabHandler] Pas de NetworkObject sur SharedCube !");

            OnCubeGrabbed?.Invoke();
        }

        private void OnReleased(SelectExitEventArgs args)
        {
            Debug.Log($"[CubeGrabHandler] {gameObject.name} lâché par {args.interactorObject}");
            OnCubeReleased?.Invoke();
        }
    }
}