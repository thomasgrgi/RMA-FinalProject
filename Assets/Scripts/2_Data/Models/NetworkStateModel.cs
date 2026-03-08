using System;

namespace ARVRMultiplayer.Models
{
    /// <summary>
    /// Contient uniquement les données liées à l'état de la connexion réseau.
    /// Ne contient aucune logique métier Unity.
    /// </summary>
    public class NetworkStateModel
    {
        public enum ConnectionStatus
        {
            Disconnected,
            Connecting,
            Connected,
            Error
        }

        private ConnectionStatus _currentStatus = ConnectionStatus.Disconnected;
        public ConnectionStatus CurrentStatus => _currentStatus;

        private string _errorMessage = string.Empty;
        public string ErrorMessage => _errorMessage;

        // Événement déclenché uniquement quand l'état change
        public event Action<ConnectionStatus, string> OnStatusChanged;

        public void SetStatus(ConnectionStatus newStatus, string error = "")
        {
            if (_currentStatus == newStatus && _errorMessage == error) return;

            _currentStatus = newStatus;
            _errorMessage = error;
            
            OnStatusChanged?.Invoke(_currentStatus, _errorMessage);
        }
    }
}