using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using ARVRMultiplayer.Models;

namespace ARVRMultiplayer.Controllers
{
    /// <summary>
    /// Écoute les événements du XR Interaction Toolkit (ex: un tap sur l'écran AR ou un clic VR).
    /// Envoie l'ordre de changement de couleur au modèle réseau.
    /// </summary>
    [RequireComponent(typeof(NetworkColorState))]
    public class NetworkColorController : MonoBehaviour
    {
        private NetworkColorState _state;
        private XRBaseInteractable _interactable;

        private void Awake()
        {
            _state = GetComponent<NetworkColorState>();
            
            // On récupère le composant d'interaction (XRGrabInteractable ou XRSimpleInteractable)
            _interactable = GetComponent<XRBaseInteractable>();

            if (_interactable != null)
            {
                // On s'abonne à l'événement "Select" (qui correspond à un Tap en AR ou un Grab en VR)
                _interactable.selectEntered.AddListener(HandleSelectEntered);
            }
            else
            {
                Debug.LogWarning("NetworkColorController: Aucun composant interactable (XRGrab/XRSimple) trouvé sur l'objet.");
            }
        }

        private void OnDestroy()
        {
            if (_interactable != null)
            {
                _interactable.selectEntered.RemoveListener(HandleSelectEntered);
            }
        }

        private void HandleSelectEntered(SelectEnterEventArgs args)
        {
            // Génère une couleur aléatoire
            Color randomColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            
            // Demande au réseau de valider cette nouvelle couleur
            _state.RequestColorChangeRpc(randomColor);
            
            Debug.Log($"Interaction déclenchée par {args.interactorObject.transform.name}. Nouvelle couleur demandée.");
        }
    }
}