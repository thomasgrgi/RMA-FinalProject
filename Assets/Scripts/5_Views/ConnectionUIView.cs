using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ARVRMultiplayer.Models;

namespace ARVRMultiplayer.Views
{
    /// <summary>
    /// Gère uniquement l'affichage et la capture des entrées utilisateur.
    /// Ne contient aucune logique réseau.
    /// </summary>
    public class ConnectionUIView : MonoBehaviour
    {
        [Header("Éléments d'interface")]
        [SerializeField] private Button _connectButton;
        [SerializeField] private Button _disconnectButton;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TMP_InputField _profileNameInput;
        [SerializeField] private TMP_InputField _sessionNameInput;

        // Action déclenchée quand l'utilisateur clique (transmise au Controller)
        public event Action<string, string> OnConnectButtonClicked;
        public event Action OnDisconnectButtonClicked;

        private void Awake()
        {
            _connectButton.onClick.AddListener(HandleConnectClick);
            if (_disconnectButton != null)
            {
                _disconnectButton.onClick.AddListener(HandleDisconnectClick);
            }
        }

        private void OnDestroy()
        {
            _connectButton.onClick.RemoveListener(HandleConnectClick);
            if (_disconnectButton != null)
            {
                _disconnectButton.onClick.RemoveListener(HandleDisconnectClick);
            }
        }

        private void HandleConnectClick()
        {
            string profile = string.IsNullOrEmpty(_profileNameInput.text) ? "DefaultPlayer" : _profileNameInput.text;
            string session = string.IsNullOrEmpty(_sessionNameInput.text) ? "SharedSession" : _sessionNameInput.text;
            
            OnConnectButtonClicked?.Invoke(profile, session);
        }

        private void HandleDisconnectClick()
        {
            OnDisconnectButtonClicked?.Invoke();
        }

        // Appelée par le contrôleur pour mettre à jour l'affichage
        public void UpdateView(NetworkStateModel.ConnectionStatus status, string errorMsg)
        {
            switch (status)
            {
                case NetworkStateModel.ConnectionStatus.Disconnected:
                    _statusText.text = "Prêt à se connecter";
                    _statusText.color = Color.white;
                    _connectButton.interactable = true;
                    if (_disconnectButton != null) _disconnectButton.interactable = false;
                    break;
                case NetworkStateModel.ConnectionStatus.Connecting:
                    _statusText.text = "Connexion en cours...";
                    _statusText.color = Color.yellow;
                    _connectButton.interactable = false;
                    if (_disconnectButton != null) _disconnectButton.interactable = false;
                    break;
                case NetworkStateModel.ConnectionStatus.Connected:
                    _statusText.text = "Connecté !";
                    _statusText.color = Color.green;
                    _connectButton.interactable = false;
                    if (_disconnectButton != null) _disconnectButton.interactable = true;
                    break;
                case NetworkStateModel.ConnectionStatus.Error:
                    _statusText.text = $"Erreur: {errorMsg}";
                    _statusText.color = Color.red;
                    _connectButton.interactable = true;
                    if (_disconnectButton != null) _disconnectButton.interactable = false;
                    break;
            }
        }
    }
}