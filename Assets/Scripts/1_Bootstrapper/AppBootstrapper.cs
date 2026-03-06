using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using PortalEscape.Data;
using PortalEscape.Services;
using PortalEscape.Views;

namespace PortalEscape.Bootstrapper
{
    /// <summary>
    /// Point d'entrée unique de l'application.
    /// Remplace le GameObject "Manager". Il instancie, configure et injecte.
    /// </summary>
    public class AppBootstrapper : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private PortalEscapeConfig config;

        [Header("Vues à injecter")]
        [SerializeField] private DebugOverlayView debugOverlayView;
        [SerializeField] private SharedCubeView sharedCubeView;
        
        [Header("Composants Réseau")]
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private NetworkSyncService networkSyncService;

        // Services (Instanciés ici, pas de Singleton !)
        private LoggerService _loggerService;

        private void Awake()
        {
            // 1. Création des Services Pours (sans MonoBehavior)
            _loggerService = new LoggerService();

            // 2. Initialisation des Vues (Injection des dépendances)
            if (config.showDebugOverlay)
            {
                debugOverlayView.Init(_loggerService);
            }
            else
            {
                debugOverlayView.gameObject.SetActive(false);
            }

            // 3. Initialisation des Services Unity (Network)
            networkSyncService.Init(_loggerService);

            // 4. Connexion des Vues aux Services
            sharedCubeView.Init(networkSyncService);

            _loggerService.LogInfo("Bootstrapper a terminé l'injection des dépendances.");
        }

        private void Start()
        {
            // 5. Démarrage de l'application selon le rôle (Défini dans le ScriptableObject)
            StartNetworkRole();
        }

        private void StartNetworkRole()
        {
            var transport = networkManager.GetComponent<UnityTransport>();

            if (config.currentRole == AppRole.VR_Host)
            {
                // La VR héberge la partie (Host = Server + Client)
                _loggerService.LogInfo("Démarrage en tant que VR Host...");
                networkManager.StartHost();
            }
            else if (config.currentRole == AppRole.AR_Client)
            {
                // Le téléphone rejoint la partie
                _loggerService.LogInfo($"Tentative de connexion AR au serveur : {config.serverIP}");
                transport.SetConnectionData(config.serverIP, 7777);
                networkManager.StartClient();
            }
        }
        
        // --- Pour tester rapidement dans Unity sans UI complexe ---
        private void Update()
        {
            // Simule un déplacement demandé par le joueur en appuyant sur Espace
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Vector3 randomPos = new Vector3(Random.Range(-2f, 2f), 1f, Random.Range(-2f, 2f));
                
                if (config.currentRole == AppRole.VR_Host)
                {
                    networkSyncService.MoveCubeDirectly(randomPos);
                    _loggerService.LogInfo("VR déplace le cube.");
                }
                else
                {
                    networkSyncService.RequestMoveCubeServerRpc(randomPos);
                    _loggerService.LogInfo("AR demande le déplacement du cube.");
                }
            }
        }
    }
}