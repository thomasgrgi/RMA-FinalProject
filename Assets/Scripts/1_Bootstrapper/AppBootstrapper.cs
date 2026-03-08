using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.InputSystem;
using PortalEscape.Data;
using PortalEscape.Services;
using PortalEscape.Views;

namespace PortalEscape.Bootstrapper
{
    public class AppBootstrapper : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private PortalEscapeConfig config;

        [Header("Caméras / Rigs (Local)")]
        [Tooltip("Ce qui gère les capteurs physiques (Non-Réseau)")]
        [SerializeField] private GameObject vrRigPrefab;
        [SerializeField] private GameObject arRigPrefab;

        [Header("Avatars (Réseau)")]
        [Tooltip("Ce que les autres joueurs voient (NetworkObjects)")]
        [SerializeField] private GameObject vrNetworkAvatarPrefab;
        [SerializeField] private GameObject arNetworkAvatarPrefab;

        [Header("Vues à injecter")]
        [SerializeField] private DebugOverlayView debugOverlayView;
        [SerializeField] private SharedCubeView sharedCubeView;
        
        [Header("Composants Réseau")]
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private NetworkSyncService networkSyncService;

        // Services
        private LoggerService _loggerService;
        private ISession _session;

        private void Awake()
        {
            _loggerService = new LoggerService();

            if (config.showDebugOverlay && debugOverlayView != null)
            {
                debugOverlayView.Init(_loggerService);
            }
            else if (debugOverlayView != null)
            {
                debugOverlayView.gameObject.SetActive(false);
            }

            // 1. Fait apparaître NOTRE caméra physique (0 impact réseau)
            SpawnLocalRig();

            networkSyncService.Init(_loggerService);
            if (sharedCubeView != null) sharedCubeView.Init(networkSyncService);

            _loggerService.LogInfo("Bootstrapper initialisé.");
        }

        private void SpawnLocalRig()
        {
            if (config.currentRole == AppRole.VR_Host && vrRigPrefab != null)
            {
                Instantiate(vrRigPrefab);
                _loggerService.LogInfo("VR Rig Local instancié.");
            }
            else if (config.currentRole == AppRole.AR_Client && arRigPrefab != null)
            {
                Instantiate(arRigPrefab);
                _loggerService.LogInfo("AR Rig Local instancié.");
            }
        }

        private void Start()
        {
            // On lance la séquence asynchrone complète
            _ = InitAndConnectToSharedWorldAsync();
        }

        private void OnDestroy()
        {
            if (networkManager != null)
                networkManager.ConnectionApprovalCallback -= ApprovalCheck;
                
            _session?.LeaveAsync();
        }

        private async Task InitAndConnectToSharedWorldAsync()
        {
            try
            {
                _loggerService.LogInfo("Initialisation d'Unity Services...");
                // On s'assure que UGS est prêt AVANT de faire quoi que ce soit d'autre
                await UnityServices.InitializeAsync();
                
                string profileName = config.currentRole == AppRole.VR_Host ? "HostVR" : "ClientAR";
                AuthenticationService.Instance.SwitchProfile(profileName);
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                _loggerService.LogInfo($"Authentifié en tant que : {profileName}");

                // On attache notre "rôle" (VR ou AR) à nos données de connexion
                networkManager.NetworkConfig.ConnectionData = BitConverter.GetBytes((int)config.currentRole);

                // Si on est le Host VR, on active le callback d'approbation pour gérer les spawns
                if (config.currentRole == AppRole.VR_Host)
                {
                    networkManager.ConnectionApprovalCallback = ApprovalCheck;
                }

                // Configuration du Lobby de l'Escape Game
                var options = new SessionOptions() {
                    Name = "PortalEscapeRoom",
                    MaxPlayers = 4
                }.WithDistributedAuthorityNetwork();

                _loggerService.LogInfo("Connexion au monde partagé...");
                
                // Unity va automatiquement faire un StartHost ou StartClient selon s'il est le premier
                _session = await MultiplayerService.Instance.CreateOrJoinSessionAsync("PortalEscapeRoom", options);
                
                _loggerService.LogInfo("Session rejointe avec succès !");
            }
            catch (Exception e)
            {
                _loggerService.LogError($"Échec réseau: {e.Message}");
            }
        }

        /// <summary>
        /// Appelée UNIQUEMENT PAR LE SERVEUR quand un joueur (y compris lui-même) tente de rejoindre.
        /// </summary>
        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            // On accepte la connexion
            response.Approved = true;
            // On bloque le spawn automatique de base de Netcode (car le Host doit choisir le bon prefab)
            response.CreatePlayerObject = false;

            // Décodage du rôle envoyé par le joueur
            AppRole incomingRole = AppRole.VR_Host;
            if (request.Payload != null && request.Payload.Length == 4)
            {
                incomingRole = (AppRole)BitConverter.ToInt32(request.Payload, 0);
            }

            // Instanciation asymétrique du bon modèle !
            GameObject prefabToSpawn = incomingRole == AppRole.VR_Host ? vrNetworkAvatarPrefab : arNetworkAvatarPrefab;
            
            if (prefabToSpawn != null)
            {
                GameObject avatarInstance = Instantiate(prefabToSpawn);
                var netObj = avatarInstance.GetComponent<NetworkObject>();
                
                // Spawn l'objet sur le réseau et l'assigne spécifiquement à l'ID de connexion
                netObj.SpawnAsPlayerObject(request.ClientNetworkId);
                _loggerService.LogInfo($"Serveur : Avatar {incomingRole} instancié pour le client {request.ClientNetworkId}");
            }
            else
            {
                _loggerService.LogError($"Erreur : Prefab manquant pour le rôle {incomingRole}");
            }
        }
        
        private void Update()
        {
            // Test du mouvement partagé
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Vector3 randomPos = new Vector3(UnityEngine.Random.Range(-2f, 2f), 1f, UnityEngine.Random.Range(-2f, 2f));
                if (config.currentRole == AppRole.VR_Host && networkSyncService != null)
                {
                    networkSyncService.MoveCubeDirectly(randomPos);
                }
            }
        }
    }
}