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
        // Faux = État normal (par défaut), Vrai = État alternatif (lumières éteintes, etc.)
        public NetworkVariable<bool> IsAlternateMode = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        // N'importe quel joueur peut demander au serveur de basculer l'état
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void RequestToggleEnvironmentRpc()
        {
            // On inverse la valeur actuelle (True devient False, False devient True)
            IsAlternateMode.Value = !IsAlternateMode.Value;
        }
    }
}