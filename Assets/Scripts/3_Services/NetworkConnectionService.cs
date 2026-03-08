using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace ARVRMultiplayer.Services
{
    /// <summary>
    /// Gère la logique de connexion avec Unity Services et Netcode for GameObjects.
    /// Indépendant de l'UI et des contrôleurs de jeu.
    /// </summary>
    public class NetworkConnectionService
    {
        private readonly NetworkManager _networkManager;
        private ISession _currentSession;

        public event Action OnConnectionSuccess;
        public event Action<string> OnConnectionFailure;
        public event Action<ulong> OnClientConnected;

        public NetworkConnectionService(NetworkManager networkManager)
        {
            _networkManager = networkManager;
        }

        public async Task InitializeAsync()
        {
            try
            {
                await UnityServices.InitializeAsync();
                
                // Abonnement aux événements de Netcode
                _networkManager.OnClientConnectedCallback += HandleClientConnected;
            }
            catch (Exception e)
            {
                Debug.LogError($"Erreur lors de l'initialisation des services: {e.Message}");
            }
        }

        public async Task ConnectToSessionAsync(string profileName, string sessionName, int maxPlayers)
        {
            try
            {
                // Gestion de l'authentification anonyme
                AuthenticationService.Instance.SwitchProfile(profileName);
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                // Configuration de la session (Distributed Authority pour s'adapter au P2P/Relay si besoin)
                var options = new SessionOptions()
                {
                    Name = sessionName,
                    MaxPlayers = maxPlayers
                }.WithDistributedAuthorityNetwork();

                // Création ou jointure
                _currentSession = await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionName, options);

                OnConnectionSuccess?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                OnConnectionFailure?.Invoke(e.Message);
            }
        }

        public void Disconnect()
        {
            _currentSession?.LeaveAsync();
            if (_networkManager != null && _networkManager.IsConnectedClient)
            {
                _networkManager.Shutdown();
            }
        }

        private void HandleClientConnected(ulong clientId)
        {
            OnClientConnected?.Invoke(clientId);
        }

        public void Cleanup()
        {
            if (_networkManager != null)
            {
                _networkManager.OnClientConnectedCallback -= HandleClientConnected;
            }
        }
    }
}