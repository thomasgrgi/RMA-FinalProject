using System;
using Unity.Netcode;
using UnityEngine;

namespace ARVRMultiplayer.Models
{
    /// <summary>
    /// Contient uniquement l'état de l'environnement (Jour/Nuit ou Normal/Alternatif) 
    /// synchronisé sur le réseau pour tous les joueurs.
    /// </summary>
    public class NetworkEnvironmentState : NetworkBehaviour
    {
        public NetworkVariable<bool> IsAlternateMode = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        // Événement statique global pour que tous les avatars puissent s'y abonner sans FindObjectOfType
        public static event Action<bool> OnAlternateModeChanged;

        public override void OnNetworkSpawn()
        {
            IsAlternateMode.OnValueChanged += (oldVal, newVal) => OnAlternateModeChanged?.Invoke(newVal);
            
            // Appliquer l'état initial pour ceux qui rejoignent la partie en retard
            if (IsClient)
            {
                OnAlternateModeChanged?.Invoke(IsAlternateMode.Value);
            }
        }

        public override void OnNetworkDespawn()
        {
            IsAlternateMode.OnValueChanged -= (oldVal, newVal) => OnAlternateModeChanged?.Invoke(newVal);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void RequestToggleEnvironmentRpc()
        {
            // On inverse la valeur actuelle (True devient False, False devient True)
            IsAlternateMode.Value = !IsAlternateMode.Value;
        }
    }
}