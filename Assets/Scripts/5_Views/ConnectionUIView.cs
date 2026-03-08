using System;
using UnityEngine;

namespace PortalEscape.Views
{
    /// <summary>
    /// Vue responsable d'afficher le bouton de connexion (remplace l'ancien OnGUI).
    /// Respecte la séparation : elle ne fait que déclencher un événement, sans aucune logique réseau.
    /// </summary>
    public class ConnectionUIView : MonoBehaviour
    {
        public event Action OnConnectRequested;
        
        private bool _isConnected = false;
        private bool _isServicesReady = false;

        public void SetServicesReady() => _isServicesReady = true;
        public void SetConnected() => _isConnected = true;

        // Peut être appelé depuis un script externe (ex: un bouton VR interactif)
        public void TriggerConnection()
        {
            if (!_isConnected && _isServicesReady)
            {
                OnConnectRequested?.Invoke();
            }
        }

        private void OnGUI()
        {
            // On masque le bouton si on est déjà connecté ou si les services ne sont pas prêts
            if (_isConnected || !_isServicesReady) return;

            GUI.skin.button.fontSize = 40;
            int width = 600;
            int height = 150;
            int x = (Screen.width - width) / 2;
            int y = Screen.height - height - 100;

            if (GUI.Button(new Rect(x, y, width, height), "REJOINDRE LA PARTIE"))
            {
                TriggerConnection();
            }
        }
    }
}