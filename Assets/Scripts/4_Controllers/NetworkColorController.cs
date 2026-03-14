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
                Debug.Log($"[DEBUG AR] Interactable trouvé sur {gameObject.name} de type : {_interactable.GetType().Name}");
                // On s'abonne à l'événement "Select" (qui correspond à un Tap en AR ou un Grab en VR)
                _interactable.selectEntered.AddListener(HandleSelectEntered);
                _interactable.hoverEntered.AddListener(HandleHoverEntered);
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
                _interactable.hoverEntered.RemoveListener(HandleHoverEntered);
            }
        }

        private void HandleHoverEntered(HoverEnterEventArgs args)
        {
            Debug.Log($"[DEBUG AR] HOVER ENTER : Le rayon de {args.interactorObject.transform.name} touche le cube !");
        }

        private void HandleSelectEntered(SelectEnterEventArgs args)
        {
            Debug.Log($"[DEBUG AR] SELECT ENTER : Clic validé par {args.interactorObject.transform.name} !");
            
            if (_state == null)
            {
                Debug.LogError("[DEBUG AR] ERREUR : Le NetworkColorState est NULL.");
                return;
            }
            
            // Génère une couleur aléatoire
            Color randomColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            
            // Demande au réseau de valider cette nouvelle couleur
            _state.RequestColorChangeRpc(randomColor);
            
            Debug.Log($"Interaction déclenchée par {args.interactorObject.transform.name}. Nouvelle couleur {randomColor} demandée.");
        }
    }
}