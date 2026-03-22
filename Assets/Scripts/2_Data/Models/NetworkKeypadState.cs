using Unity.Netcode;
using Unity.Collections;
using System;
using UnityEngine;

namespace ARVRMultiplayer.Models
{
    /// <summary>
    /// Gère la donnée du clavier (le code tapé) et la validation réseau.
    /// Ne connaît ni les boutons, ni l'interface.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkKeypadState : NetworkBehaviour
    {
        // On utilise FixedString32Bytes car c'est le format texte optimisé pour Unity Netcode
        public NetworkVariable<FixedString32Bytes> CurrentInput = new NetworkVariable<FixedString32Bytes>(
            "",
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        // Événements déclenchés par le serveur pour avertir tout le monde
        public event Action OnValidationSuccess;
        public event Action OnValidationFailure;

        private readonly string _targetCode = "5203";
        private readonly int _maxDigits = 4;

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void SubmitDigitRpc(char digit)
        {
            string currentString = CurrentInput.Value.ToString();
            
            if (currentString.Length < _maxDigits)
            {
                CurrentInput.Value = new FixedString32Bytes(currentString + digit);
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void ClearRpc()
        {
            CurrentInput.Value = new FixedString32Bytes("");
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void ValidateRpc()
        {
            if (CurrentInput.Value.ToString() == _targetCode)
            {
                NotifySuccessRpc();
            }
            else
            {
                NotifyFailureRpc();
                ClearRpc(); // Vide l'écran automatiquement en cas d'erreur
            }
        }

        [Rpc(SendTo.Everyone)]
        private void NotifySuccessRpc()
        {
            OnValidationSuccess?.Invoke();
        }

        [Rpc(SendTo.Everyone)]
        private void NotifyFailureRpc()
        {
            OnValidationFailure?.Invoke();
        }
    }
}