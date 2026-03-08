using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ARVRMultiplayer.Views
{
    /// <summary>
    /// Gère l'affichage visuel des logs (très utile en VR).
    /// </summary>
    public class DebugLogView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _logTextDisplay;
        [SerializeField] private int _maxLines = 15;

        private Queue<string> _logQueue = new Queue<string>();

        // Cette méthode sera appelée via Action par un DebugService ou Bootstrapper
        public void AddLog(string logMessage, LogType type)
        {
            string color = type == LogType.Error || type == LogType.Exception ? "red" : 
                           type == LogType.Warning ? "yellow" : "white";

            string formattedLog = $"<color={color}>{logMessage}</color>";
            _logQueue.Enqueue(formattedLog);

            if (_logQueue.Count > _maxLines)
            {
                _logQueue.Dequeue();
            }

            if (_logTextDisplay != null)
            {
                _logTextDisplay.text = string.Join("\n", _logQueue.ToArray());
            }
        }
    }
}