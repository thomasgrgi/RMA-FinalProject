using UnityEngine;
using TMPro;

namespace PortalEscape.Views
{
    /// <summary>
    /// Vue responsable de l'affichage des logs dans le casque/téléphone.
    /// Ne contient aucune logique métier, se contente d'écouter le LoggerService.
    /// </summary>
    public class DebugOverlayView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI logText;
        private int logCount = 0;

        // Injection via Bootstrapper
        public void Init(Services.LoggerService logger)
        {
            // Abonnement à l'événement du service
            logger.OnNewLog += DisplayLog;
            logText.text = "=== Debug Overlay Initialisé ===";
        }

        private void DisplayLog(string message, Color color)
        {
            logCount++;
            if (logCount > 10) 
            {
                logText.text = ""; // On vide pour ne pas surcharger l'écran
                logCount = 1;
            }
            
            // Formatage de la couleur en balise HTML pour TextMeshPro
            string colorHex = ColorUtility.ToHtmlStringRGB(color);
            logText.text += $"\n<color=#{colorHex}>{message}</color>";
        }

        private void OnDestroy()
        {
            // IMPORTANT : Toujours se désabonner des événements pour éviter les fuites de mémoire
            // (La désinscription sera gérée proprement via le Bootstrapper dans un projet complet)
        }
    }
}