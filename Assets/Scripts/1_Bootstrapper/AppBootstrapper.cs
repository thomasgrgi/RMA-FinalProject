using UnityEngine;
using Unity.Netcode;
using ARVRMultiplayer.Models;
using ARVRMultiplayer.Services;
using ARVRMultiplayer.Views;
using ARVRMultiplayer.Controllers;

namespace ARVRMultiplayer.Core
{
    public class AppBootstrapper : MonoBehaviour
    {
        [Header("Dépendances Unity externes")]
        [SerializeField] private NetworkManager _networkManager;

        [Header("UI Globale")]
        [Tooltip("Le Canvas qui contient la vue de connexion et les logs")]
        [SerializeField] private Canvas _mainUICanvas; 
        [SerializeField] private GameObject _uiMenuContainer; // Le conteneur à afficher/masquer
        [SerializeField] private ConnectionUIView _connectionView;
        [SerializeField] private DebugLogView _debugLogView;

        [Header("UI Spécifique AR")]
        [SerializeField] private ARHUDView _arHudView;

        [Header("Rigs Locaux")]
        [SerializeField] private bool _forceARMode = false;
        
        [SerializeField] private GameObject _vrRigRoot;
        [SerializeField] private Transform _vrHead;
        [SerializeField] private Transform _vrLeftHand;
        [SerializeField] private Transform _vrRightHand;
        
        [Space]
        [SerializeField] private GameObject _arRigRoot;
        [SerializeField] private Transform _arCamera;

        // Contrôleurs et Services
        private NetworkConnectionService _connectionService;
        private ConnectionController _connectionController;
        private LocalRigService _localRigService;
        private ARHUDController _arHudController;

        private async void Start()
        {
            Application.logMessageReceived += HandleSystemLog;
            
            // 1. Initialiser le service de Rig Local (et basculer l'UI)
            SetupLocalRigAndUI();

            // 2. Créer le Modèle et le Service réseau
            var networkModel = new NetworkStateModel();
            _connectionService = new NetworkConnectionService(_networkManager);
            await _connectionService.InitializeAsync();

            // 3. Créer le Contrôleur
            _connectionController = new ConnectionController(networkModel, _connectionService, _connectionView, maxPlayers: 10);
            
            // 4. S'abonner au spawn
            AvatarSyncController.OnLocalAvatarSpawned += HandleLocalAvatarSpawned;
        }

        private void SetupLocalRigAndUI()
        {
            _localRigService = new LocalRigService();

            bool isMobile = Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
            bool useAR = isMobile || _forceARMode;

            if (useAR)
            {
                Debug.Log("AppBootstrapper: Mode AR détecté.");
                _vrRigRoot.SetActive(false);
                _arRigRoot.SetActive(true);
                
                // Enregistrement avec le ROOT
                _localRigService.RegisterARRig(_arRigRoot.transform, _arCamera);

                // --- MAGIE ARCHITECTURALE ---
                // On passe le Canvas de VR (WorldSpace) à AR (ScreenSpaceOverlay) dynamiquement
                if (_mainUICanvas != null)
                {
                    _mainUICanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                }

                // On active le HUD AR et on crée son contrôleur
                if (_arHudView != null)
                {
                    _arHudView.gameObject.SetActive(true);
                    _arHudController = new ARHUDController(_localRigService, _arHudView, _uiMenuContainer, resetHeight: 1.5f);
                }
            }
            else
            {
                Debug.Log("AppBootstrapper: Mode VR détecté.");
                _arRigRoot.SetActive(false);
                _vrRigRoot.SetActive(true);
                
                // Enregistrement avec le ROOT
                _localRigService.RegisterVRRig(_vrRigRoot.transform, _vrHead, _vrLeftHand, _vrRightHand);

                // On s'assure que le Canvas est bien en mode VR
                if (_mainUICanvas != null)
                {
                    _mainUICanvas.renderMode = RenderMode.WorldSpace;
                }

                // On désactive le HUD AR car on est en VR
                if (_arHudView != null)
                {
                    _arHudView.gameObject.SetActive(false);
                }
            }
        }

        private void HandleLocalAvatarSpawned(AvatarSyncController localAvatarController)
        {
            localAvatarController.Init(_localRigService);
        }

        private void HandleSystemLog(string logString, string stackTrace, LogType type)
        {
            if (_debugLogView != null) _debugLogView.AddLog(logString, type);
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleSystemLog;
            AvatarSyncController.OnLocalAvatarSpawned -= HandleLocalAvatarSpawned;
            
            _arHudController?.Cleanup();
            _connectionController?.Cleanup();
            _connectionService?.Cleanup();
            _connectionService?.Disconnect();
        }
    }
}