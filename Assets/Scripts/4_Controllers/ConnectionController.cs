using UnityEngine;
using ARVRMultiplayer.Models;
using ARVRMultiplayer.Services;
using ARVRMultiplayer.Views;

namespace ARVRMultiplayer.Controllers
{
    /// <summary>
    /// Fait le lien entre la Vue, le Modèle et le Service.
    /// S'abonne aux événements pour réagir aux actions.
    /// </summary>
    public class ConnectionController
    {
        private readonly NetworkStateModel _model;
        private readonly NetworkConnectionService _service;
        private readonly ConnectionUIView _view;
        private readonly int _maxPlayers;

        public ConnectionController(NetworkStateModel model, NetworkConnectionService service, ConnectionUIView view, int maxPlayers = 10)
        {
            _model = model;
            _service = service;
            _view = view;
            _maxPlayers = maxPlayers;

            // Abonnement aux événements du Service
            _service.OnConnectionSuccess += HandleConnectionSuccess;
            _service.OnConnectionFailure += HandleConnectionFailure;
            _service.OnClientConnected += HandleClientConnected;

            // Abonnement aux événements du Modèle
            _model.OnStatusChanged += _view.UpdateView;

            // Abonnement aux événements de la Vue
            _view.OnConnectButtonClicked += HandleConnectRequested;

            // Initialisation de la vue avec l'état actuel
            _model.SetStatus(NetworkStateModel.ConnectionStatus.Disconnected);
        }

        private async void HandleConnectRequested(string profileName, string sessionName)
        {
            _model.SetStatus(NetworkStateModel.ConnectionStatus.Connecting);
            await _service.ConnectToSessionAsync(profileName, sessionName, _maxPlayers);
        }

        private void HandleConnectionSuccess()
        {
            _model.SetStatus(NetworkStateModel.ConnectionStatus.Connected);
            Debug.Log("Connexion réseau établie avec succès !");
        }

        private void HandleConnectionFailure(string error)
        {
            _model.SetStatus(NetworkStateModel.ConnectionStatus.Error, error);
            Debug.LogError($"Échec de connexion : {error}");
        }

        private void HandleClientConnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} a rejoint la partie.");
        }

        public void Cleanup()
        {
            _service.OnConnectionSuccess -= HandleConnectionSuccess;
            _service.OnConnectionFailure -= HandleConnectionFailure;
            _service.OnClientConnected -= HandleClientConnected;
            _model.OnStatusChanged -= _view.UpdateView;
            _view.OnConnectButtonClicked -= HandleConnectRequested;
        }
    }
}