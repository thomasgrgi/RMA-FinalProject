using Unity.Netcode;
using UnityEngine;

namespace ARVRMultiplayer.Models
{
    /// <summary>
    /// Contient EXCLUSIVEMENT les variables réseau synchronisées.
    /// Pas de logique de déplacement ou d'affichage ici.
    /// </summary>
    public class NetworkAvatarState : NetworkBehaviour
    {
        // On synchronise le type d'appareil pour savoir si on doit afficher les mains
        public NetworkVariable<bool> IsVR = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // Positions et Rotations
        public NetworkVariable<Vector3> HeadPos = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner);
        public NetworkVariable<Quaternion> HeadRot = new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Owner);
        
        public NetworkVariable<Vector3> LeftHandPos = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner);
        public NetworkVariable<Quaternion> LeftHandRot = new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Owner);

        public NetworkVariable<Vector3> RightHandPos = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner);
        public NetworkVariable<Quaternion> RightHandRot = new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Owner);
    }
}