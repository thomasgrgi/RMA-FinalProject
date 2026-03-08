using System;
using UnityEngine;
using Unity.Netcode;
using ARVRMultiplayer.Models;
using ARVRMultiplayer.Services;

namespace ARVRMultiplayer.Controllers
{
    /// <summary>
    /// Lit les mouvements du Rig local et les pousse dans le NetworkAvatarState.
    /// Ce script ne tourne QUE sur le client propriétaire de l'avatar.
    /// </summary>
    [RequireComponent(typeof(NetworkAvatarState))]
    public class AvatarSyncController : NetworkBehaviour
    {
        // Événement statique pour informer le Bootstrapper qu'un avatar local a spawn et a besoin d'injection
        public static event Action<AvatarSyncController> OnLocalAvatarSpawned;

        private NetworkAvatarState _state;
        private LocalRigService _rigService;
        private bool _isInitialized = false;

        private void Awake()
        {
            _state = GetComponent<NetworkAvatarState>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                // On signale au Bootstrapper de nous injecter le service du Rig
                OnLocalAvatarSpawned?.Invoke(this);
            }
            else
            {
                // Si on n'est pas le propriétaire, ce script ne fait rien, on peut le désactiver pour économiser des ressources
                enabled = false;
            }
        }

        // Méthode appelée par le Bootstrapper pour injecter les dépendances
        public void Init(LocalRigService rigService)
        {
            _rigService = rigService;
            _state.IsVR.Value = _rigService.IsVR;
            _isInitialized = true;
        }

        private void Update()
        {
            if (!IsOwner || !_isInitialized || _rigService == null) return;

            // Mise à jour de la tête (VR et AR)
            if (_rigService.Head != null)
            {
                _state.HeadPos.Value = _rigService.Head.position;
                _state.HeadRot.Value = _rigService.Head.rotation;
            }

            // Mise à jour des mains (Uniquement VR)
            if (_rigService.IsVR)
            {
                if (_rigService.LeftHand != null)
                {
                    _state.LeftHandPos.Value = _rigService.LeftHand.position;
                    _state.LeftHandRot.Value = _rigService.LeftHand.rotation;
                }

                if (_rigService.RightHand != null)
                {
                    _state.RightHandPos.Value = _rigService.RightHand.position;
                    _state.RightHandRot.Value = _rigService.RightHand.rotation;
                }
            }
        }
    }
}