using UnityEngine;
using Unity.Netcode;
using ARVRMultiplayer.Models;

namespace ARVRMultiplayer.Views
{
    /// <summary>
    /// Gère l'affichage visuel (Cubes, modèles 3D) des autres joueurs sur le réseau.
    /// </summary>
    [RequireComponent(typeof(NetworkAvatarState))]
    public class NetworkAvatarView : NetworkBehaviour
    {
        [Header("Éléments Visuels")]
        [SerializeField] private Transform _visualHead;
        [SerializeField] private Transform _visualLeftHand;
        [SerializeField] private Transform _visualRightHand;

        private NetworkAvatarState _state;

        private void Awake()
        {
            _state = GetComponent<NetworkAvatarState>();
            
            // On cache tout par défaut tant qu'on est pas connecté
            SetVisualsActive(false);
        }

        public override void OnNetworkSpawn()
        {
            // Si c'est NOTRE avatar, on ne veut pas voir nos propres cubes réseaux 
            // (on voit déjà notre vrai Rig local)
            if (IsOwner)
            {
                SetVisualsActive(false);
                enabled = false; // Désactive l'Update d'affichage pour le proprio
            }
            else
            {
                SetVisualsActive(true);
            }
        }

        private void Update()
        {
            // Met à jour les transform visuels avec les données du réseau
            if (_visualHead != null)
            {
                _visualHead.position = _state.HeadPos.Value;
                _visualHead.rotation = _state.HeadRot.Value;
            }

            // Affiche les mains uniquement si le joueur distant est en VR
            bool isDistantVR = _state.IsVR.Value;
            
            if (_visualLeftHand != null)
            {
                _visualLeftHand.gameObject.SetActive(isDistantVR);
                if (isDistantVR)
                {
                    _visualLeftHand.position = _state.LeftHandPos.Value;
                    _visualLeftHand.rotation = _state.LeftHandRot.Value;
                }
            }

            if (_visualRightHand != null)
            {
                _visualRightHand.gameObject.SetActive(isDistantVR);
                if (isDistantVR)
                {
                    _visualRightHand.position = _state.RightHandPos.Value;
                    _visualRightHand.rotation = _state.RightHandRot.Value;
                }
            }
        }

        private void SetVisualsActive(bool active)
        {
            if (_visualHead != null) _visualHead.gameObject.SetActive(active);
            if (_visualLeftHand != null) _visualLeftHand.gameObject.SetActive(active);
            if (_visualRightHand != null) _visualRightHand.gameObject.SetActive(active);
        }
    }
}