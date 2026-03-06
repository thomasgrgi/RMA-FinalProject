using UnityEngine;

namespace PortalEscape.Views
{
    /// <summary>
    /// Vue de l'objet partagé. 
    /// Ne contient pas de logique métier, réagit seulement aux ordres du réseau.
    /// </summary>
    public class SharedCubeView : MonoBehaviour
    {
        private Vector3 _targetPosition;
        private bool _isInitialized = false;

        // Injection via Bootstrapper
        public void Init(Services.NetworkSyncService networkService)
        {
            // On s'abonne à l'événement de changement de position
            networkService.OnCubePositionReceived += UpdateTargetPosition;
            _targetPosition = transform.position;
            _isInitialized = true;
        }

        private void UpdateTargetPosition(Vector3 newPosition)
        {
            _targetPosition = newPosition;
        }

        // L'Update est ici légitime : c'est un besoin continu d'interpolation purement visuelle !
        // Ce n'est pas de la logique métier.
        private void Update()
        {
            if (!_isInitialized) return;

            // Interpolation douce pour un rendu fluide sur les deux écrans
            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * 5f);
        }
    }
}