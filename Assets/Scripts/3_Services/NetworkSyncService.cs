using System;
using Unity.Netcode;
using UnityEngine;

namespace PortalEscape.Services
{
    /// <summary>
    /// Gère la synchronisation réseau. 
    /// Hérite de NetworkBehaviour pour Netcode, mais agit comme un Service pour notre architecture.
    /// </summary>
    public class NetworkSyncService : NetworkBehaviour
    {
        // --- Événements pour découpler la logique des Vues ---
        public event Action<Vector3> OnCubePositionReceived;
        
        // --- Données synchronisées (Modèle) ---
        // NetworkVariable gère automatiquement la synchro entre Host et Client
        private NetworkVariable<Vector3> sharedCubePosition = new NetworkVariable<Vector3>(
            Vector3.zero, 
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Server
        );

        private LoggerService _logger;

        // Méthode d'injection (appelée par le Bootstrapper)
        public void Init(LoggerService logger)
        {
            _logger = logger;
            _logger.LogInfo("NetworkSyncService initialisé.");
        }

        public override void OnNetworkSpawn()
        {
            _logger?.LogInfo(IsServer ? "Réseau démarré en tant que Serveur (VR)" : "Réseau démarré en tant que Client (AR)");
            
            // On s'abonne aux changements de la variable réseau (sans utiliser Update !)
            sharedCubePosition.OnValueChanged += HandleCubePositionChanged;
        }

        public override void OnNetworkDespawn()
        {
            sharedCubePosition.OnValueChanged -= HandleCubePositionChanged;
        }

        private void HandleCubePositionChanged(Vector3 previousValue, Vector3 newValue)
        {
            // On notifie les vues intéressées que la position a changé
            OnCubePositionReceived?.Invoke(newValue);
            _logger?.LogInfo($"Position du cube synchronisée : {newValue}");
        }

        // --- API appelée par les Contrôleurs ---
        
        /// <summary>
        /// Appelée par l'AR pour demander un déplacement.
        /// Le client (AR) demande au serveur (VR) de modifier la position.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void RequestMoveCubeServerRpc(Vector3 newPosition)
        {
            _logger?.LogInfo("Requête de déplacement reçue par le serveur.");
            sharedCubePosition.Value = newPosition;
        }
        
        /// <summary>
        /// Appelée directement par la VR (Host) pour modifier la position.
        /// </summary>
        public void MoveCubeDirectly(Vector3 newPosition)
        {
            if (IsServer)
            {
                sharedCubePosition.Value = newPosition;
            }
        }
    }
}