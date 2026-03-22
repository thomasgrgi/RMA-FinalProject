using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using ARVRMultiplayer.Models;

namespace ARVRMultiplayer.Controllers
{
    /// <summary>
    /// Fait le lien entre le joueur (qui clique sur le XR Interactable) et le Modèle (Réseau).
    /// </summary>
    [RequireComponent(typeof(NetworkEnvironmentState))]
    public class EnvironmentController : MonoBehaviour
    {
        private NetworkEnvironmentState _state;
        private XRBaseInteractable _interactable;

        private void Awake()
        {
            _state = GetComponent<NetworkEnvironmentState>();
            _interactable = GetComponent<XRBaseInteractable>();

            if (_interactable != null)
            {
                // S'abonne à l'événement de clic / grab
                _interactable.selectEntered.AddListener(HandleInteraction);
            }
            else
            {
                Debug.LogWarning("[EnvironmentController] Aucun composant XRBaseInteractable trouvé. Assurez-vous d'avoir un XRSimpleInteractable sur cet objet.");
            }
        }

        private void OnDestroy()
        {
            if (_interactable != null)
            {
                _interactable.selectEntered.RemoveListener(HandleInteraction);
            }
        }

        private void HandleInteraction(SelectEnterEventArgs args)
        {
            string interactorName = args.interactorObject.transform.name;
            Debug.Log($"[EnvironmentController] Interrupteur actionné par {interactorName}. Envoi de la requête réseau.");
            
            // On demande au réseau de faire la bascule. 
            // La Vue s'occupera de changer les lumières automatiquement.
            _state.RequestToggleEnvironmentRpc();
        }
    }
}