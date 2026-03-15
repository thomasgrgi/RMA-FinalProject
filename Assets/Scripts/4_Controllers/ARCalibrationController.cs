using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils; // Requis pour XROrigin
using ARVRMultiplayer.Services;

namespace ARVRMultiplayer.Controllers
{
    /// <summary>
    /// Écoute la détection d'images et aligne l'AR Rig pour que le marqueur
    /// corresponde au repère défini.
    /// </summary>
    public class ARCalibrationController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Tooltip("Le service qui détecte les images.")] 
        private ARImageTrackingService _trackingService;
        
        [SerializeField, Tooltip("Le composant racine de l'AR (XROrigin).")] 
        private XROrigin _xrOrigin;

        [Header("Settings")]
        [SerializeField, Tooltip("Le nom exact de l'image de référence dans la librairie Unity.")] 
        private string _targetImageName = "CenterMarker";

        [SerializeField, Tooltip("Le GameObject cible virtuelle auquel le marqueur détecté doit s'aligner.")]
        private GameObject _virtualTarget;

        private bool _isCalibrated = false;

        private void OnEnable()
        {
            if (_trackingService != null)
            {
                _trackingService.OnImageTracked += HandleImageTracked;
                Debug.Log("[AR Calibration] Abonnement au service de tracking effectué.");
            }
            else
            {
                Debug.LogWarning("[AR Calibration] Aucun ARImageTrackingService assigné.");
            }
        }

        private void OnDisable()
        {
            if (_trackingService != null)
            {
                _trackingService.OnImageTracked -= HandleImageTracked;
                Debug.Log("[AR Calibration] Désabonnement du service de tracking effectué.");
            }
        }

        private void HandleImageTracked(ARTrackedImage trackedImage)
        {
            // Si on est déjà calibré, on ignore
            if (_isCalibrated)
            {
                Debug.Log($"[AR Calibration] Image '{trackedImage.referenceImage.name}' ignorée : calibration déjà effectuée.");
                return;
            }

            Debug.Log($"[AR Calibration] Image détectée par le service : '{trackedImage.referenceImage.name}'.");

            // Vérifie si l'image détectée est bien notre marqueur central
            if (trackedImage.referenceImage.name == _targetImageName)
            {
                CalibrateARSpace(trackedImage.transform);
                return;
            }

            Debug.Log($"[AR Calibration] Image '{trackedImage.referenceImage.name}' ignorée : marqueur attendu '{_targetImageName}'.");
        }

        private void CalibrateARSpace(Transform markerTransform)
        {
            if (_xrOrigin == null || markerTransform == null)
            {
                Debug.LogWarning("[AR Calibration] Calibration impossible : XROrigin ou marqueur manquant.");
                return;
            }

            if (_virtualTarget == null)
            {
                Debug.LogWarning("[AR Calibration] Calibration impossible : aucune cible virtuelle n'est assignée.");
                return;
            }

            Debug.Log($"[AR Calibration] Marqueur '{_targetImageName}' détecté. Alignement en cours...");

            Transform virtualTargetTransform = _virtualTarget.transform;
            Vector3 targetPosition = virtualTargetTransform.position;
            Quaternion targetRotation = virtualTargetTransform.rotation;
            Debug.Log($"[AR Calibration] Pose cible. Position: {targetPosition}, rotation: {targetRotation.eulerAngles}.");

            // Recalcule explicitement la pose de l'Origin pour que le marqueur détecté
            // corresponde exactement à la pose virtuelle cible.
            Transform originTransform = _xrOrigin.Origin != null ? _xrOrigin.Origin.transform : _xrOrigin.transform;

            Quaternion markerLocalRotation = Quaternion.Inverse(originTransform.rotation) * markerTransform.rotation;
            Vector3 markerLocalPosition = Quaternion.Inverse(originTransform.rotation) * (markerTransform.position - originTransform.position);

            Quaternion newOriginRotation = targetRotation * Quaternion.Inverse(markerLocalRotation);
            Vector3 newOriginPosition = targetPosition - (newOriginRotation * markerLocalPosition);

            Debug.Log($"[AR Calibration] Nouvelle pose Origin. Position: {newOriginPosition}, rotation: {newOriginRotation.eulerAngles}.");

            originTransform.SetPositionAndRotation(newOriginPosition, newOriginRotation);

            _isCalibrated = true;
            
            // On coupe le tracking d'images pour économiser les ressources du téléphone
            _trackingService.SetTrackingEnabled(false);

            Debug.Log("[AR Calibration] Calibration réussie ! L'AR Rig a été recentré.");
        }

        /// <summary>
        /// Peut être appelé par un bouton UI pour forcer une nouvelle calibration.
        /// </summary>
        public void ResetCalibration()
        {
            Debug.Log("[AR Calibration] Réinitialisation de la calibration demandée.");
            _isCalibrated = false;
            if (_trackingService != null)
            {
                _trackingService.SetTrackingEnabled(true);
                Debug.Log("[AR Calibration] Tracking réactivé pour permettre une nouvelle calibration.");
            }
            else
            {
                Debug.LogWarning("[AR Calibration] Impossible de réactiver le tracking : service manquant.");
            }
        }
    }
}