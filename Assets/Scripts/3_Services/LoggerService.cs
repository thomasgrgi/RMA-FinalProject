using System;
using UnityEngine;

namespace PortalEscape.Services
{
    /// <summary>
    /// Service responsable de l'observabilité. 
    /// Totalement indépendant de l'affichage (Vue).
    /// </summary>
    public class LoggerService
    {
        // Événement déclenché à chaque nouveau log
        public event Action<string, Color> OnNewLog;

        public void LogInfo(string message)
        {
            Debug.Log($"[INFO] {message}");
            OnNewLog?.Invoke($"[INFO] {message}", Color.white);
        }

        public void LogWarning(string message)
        {
            Debug.LogWarning($"[WARN] {message}");
            OnNewLog?.Invoke($"[WARN] {message}", Color.yellow);
        }

        public void LogError(string message)
        {
            Debug.LogError($"[ERROR] {message}");
            OnNewLog?.Invoke($"[ERROR] {message}", Color.red);
        }
    }
}