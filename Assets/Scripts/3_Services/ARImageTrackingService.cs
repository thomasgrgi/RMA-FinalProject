using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARVRMultiplayer.Services
{
    /// <summary>
    /// Service responsable de la détection d'images AR. 
    /// Il isole la logique d'AR Foundation du reste de l'application.
    /// </summary>
    [RequireComponent(typeof(ARTrackedImageManager))]
    public class ARImageTrackingService : MonoBehaviour
    {
        private ARTrackedImageManager _imageManager;

        // Événement déclenché quand une image est détectée et bien traquée
        public event Action<ARTrackedImage> OnImageTracked;

        private void Awake()
        {
            _imageManager = GetComponent<ARTrackedImageManager>();
            Debug.Log($"[AR Image Tracking] Service initialise sur '{name}'. Gestionnaire trouvé : {_imageManager != null}.");
        }

        private void OnEnable()
        {
            if (_imageManager == null)
            {
                Debug.LogWarning("[AR Image Tracking] Activation impossible : ARTrackedImageManager manquant.");
                return;
            }

            _imageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
            Debug.Log("[AR Image Tracking] Écoute des changements d'images activée.");
        }

        private void OnDisable()
        {
            if (_imageManager == null)
                return;

            _imageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
            Debug.Log("[AR Image Tracking] Écoute des changements d'images désactivée.");
        }

        private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
        {
            Debug.Log($"[AR Image Tracking] Changements reçus. Ajoutées: {args.added.Count}, mises à jour: {args.updated.Count}, supprimées: {args.removed.Count}.");

            // On vérifie les images nouvellement ajoutées
            foreach (var trackedImage in args.added)
            {
                ProcessTrackedImage(trackedImage);
            }

            // On vérifie les images mises à jour
            foreach (var trackedImage in args.updated)
            {
                ProcessTrackedImage(trackedImage);
            }
        }

        private void ProcessTrackedImage(ARTrackedImage trackedImage)
        {
            // On s'assure que l'image est bien traquée physiquement (pas juste "devinée")
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                Debug.Log($"[AR Image Tracking] Image suivie: '{trackedImage.referenceImage.name}' a la position {trackedImage.transform.position}.");
                OnImageTracked?.Invoke(trackedImage);
                return;
            }

            Debug.Log($"[AR Image Tracking] Image ignorée: '{trackedImage.referenceImage.name}' avec état {trackedImage.trackingState}.");
        }

        // Permet au contrôleur d'éteindre la détection une fois calibré (économise la batterie)
        public void SetTrackingEnabled(bool isEnabled)
        {
            _imageManager.enabled = isEnabled;
            Debug.Log($"[AR Image Tracking] Tracking {(isEnabled ? "activé" : "désactivé")}.");
        }
    }
}