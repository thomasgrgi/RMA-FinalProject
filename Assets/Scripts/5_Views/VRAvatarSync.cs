using Unity.Netcode;
using UnityEngine;

public class VRAvatarSync : NetworkBehaviour
{
    [Header("Éléments traqués par la VR (Sources)")]
    public Transform vrHead;
    public Transform vrLeftHand;
    public Transform vrRightHand;

    [Header("Éléments visuels (Avatars)")]
    public Transform visualHead;
    public Transform visualLeftHand;
    public Transform visualRightHand;

    // Variables réseau (Seul le joueur VR - Owner - a le droit d'écrire dedans)
    private NetworkVariable<Vector3> headPos = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<Quaternion> headRot = new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Owner);
    
    private NetworkVariable<Vector3> leftHandPos = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<Quaternion> leftHandRot = new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Owner);

    private NetworkVariable<Vector3> rightHandPos = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<Quaternion> rightHandRot = new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        // Au démarrage, on cache les cubes pour que le joueur VR ne soit pas gêné visuellement
        // avant même d'avoir rejoint le serveur.
        if (visualHead != null) visualHead.gameObject.SetActive(false);
        if (visualLeftHand != null) visualLeftHand.gameObject.SetActive(false);
        if (visualRightHand != null) visualRightHand.gameObject.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Si on N'EST PAS le propriétaire (donc on est le PC qui regarde l'avatar VR)
        if (!IsOwner)
        {
            // On réactive les cubes visuels pour pouvoir voir le joueur VR !
            if (visualHead != null) visualHead.gameObject.SetActive(true);
            if (visualLeftHand != null) visualLeftHand.gameObject.SetActive(true);
            if (visualRightHand != null) visualRightHand.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (!IsSpawned) return;

        if (IsOwner)
        {
            // On calcule la position et rotation RELATIVES à la racine du VRPlayer
            // transform = le VRPlayer racine
            headPos.Value = transform.InverseTransformPoint(vrHead.position);
            headRot.Value = Quaternion.Inverse(transform.rotation) * vrHead.rotation;

            leftHandPos.Value = transform.InverseTransformPoint(vrLeftHand.position);
            leftHandRot.Value = Quaternion.Inverse(transform.rotation) * vrLeftHand.rotation;

            rightHandPos.Value = transform.InverseTransformPoint(vrRightHand.position);
            rightHandRot.Value = Quaternion.Inverse(transform.rotation) * vrRightHand.rotation;
        }
        else
        {
            // On applique directement sur les visuels, car ils sont maintenant
            // enfants directs du VRPlayer (donc localPosition = position par rapport à la racine)
            visualHead.localPosition = headPos.Value;
            visualHead.localRotation = headRot.Value;

            visualLeftHand.localPosition = leftHandPos.Value;
            visualLeftHand.localRotation = leftHandRot.Value;

            visualRightHand.localPosition = rightHandPos.Value;
            visualRightHand.localRotation = rightHandRot.Value;
        }
    }
}