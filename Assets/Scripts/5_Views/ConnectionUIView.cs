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
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TMP_InputField _profileNameInput;
        [SerializeField] private TMP_InputField _sessionNameInput;

        // Action déclenchée quand l'utilisateur clique (transmise au Controller)
        public event Action<string, string> OnConnectButtonClicked;

        private void Awake()
        {
            _connectButton.onClick.AddListener(HandleConnectClick);
        }

        private void OnDestroy()
        {
            _connectButton.onClick.RemoveListener(HandleConnectClick);
        }

        private void HandleConnectClick()
        {
            string profile = string.IsNullOrEmpty(_profileNameInput.text) ? "DefaultPlayer" : _profileNameInput.text;
            string session = string.IsNullOrEmpty(_sessionNameInput.text) ? "SharedSession" : _sessionNameInput.text;
            
            OnConnectButtonClicked?.Invoke(profile, session);
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
                    break;
                case NetworkStateModel.ConnectionStatus.Connecting:
                    _statusText.text = "Connexion en cours...";
                    _statusText.color = Color.yellow;
                    _connectButton.interactable = false;
                    break;
                case NetworkStateModel.ConnectionStatus.Connected:
                    _statusText.text = "Connecté !";
                    _statusText.color = Color.green;
                    _connectButton.interactable = false;
                    break;
                case NetworkStateModel.ConnectionStatus.Error:
                    _statusText.text = $"Erreur: {errorMsg}";
                    _statusText.color = Color.red;
                    _connectButton.interactable = true;
                    break;
            }
        }
    }
}