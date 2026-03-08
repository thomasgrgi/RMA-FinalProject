using UnityEngine;

namespace ARVRMultiplayer.Services
{
    /// <summary>
    /// Service qui stocke les références du Rig du joueur local (VR ou AR).
    /// Permet de découpler le Rig physique de la représentation réseau.
    /// </summary>
    public class LocalRigService
    {
        public bool IsVR { get; private set; }
        
        // Ajout du RigRoot pour pouvoir téléporter / réinitialiser la position globale
        public Transform RigRoot { get; private set; } 
        
        public Transform Head { get; private set; }
        public Transform LeftHand { get; private set; }
        public Transform RightHand { get; private set; }

        public void RegisterVRRig(Transform rigRoot, Transform head, Transform leftHand, Transform rightHand)
        {
            IsVR = true;
            RigRoot = rigRoot;
            Head = head;
            LeftHand = leftHand;
            RightHand = rightHand;
        }

        public void RegisterARRig(Transform rigRoot, Transform phoneCamera)
        {
            IsVR = false;
            RigRoot = rigRoot;
            Head = phoneCamera;
            LeftHand = null; // Pas de mains en AR (téléphone)
            RightHand = null;
        }
    }
}