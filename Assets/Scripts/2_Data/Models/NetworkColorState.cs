using Unity.Netcode;
using UnityEngine;

namespace ARVRMultiplayer.Models
{
    /// <summary>
    /// Modèle contenant uniquement la donnée de couleur synchronisée sur le réseau.
    /// Écrite par le serveur, lue par tout le monde.
    /// </summary>
    public class NetworkColorState : NetworkBehaviour
    {
        public NetworkVariable<Color> CurrentColor = new NetworkVariable<Color>(
            Color.white,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        // N'importe quel client (VR ou AR) peut demander au serveur de changer la couleur
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void RequestColorChangeRpc(Color newColor)
        {
            CurrentColor.Value = newColor;
        }
    }
}