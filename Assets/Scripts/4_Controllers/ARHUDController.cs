using UnityEngine;
using ARVRMultiplayer.Services;
using ARVRMultiplayer.Views;

namespace ARVRMultiplayer.Controllers
{
    /// <summary>
    /// Fait le lien entre la vue HUD de l'AR, le Rig, et l'interface globale.
    /// </summary>
    public class ARHUDController
    {
        private readonly LocalRigService _rigService;
        private readonly ARHUDView _hudView;
        private readonly GameObject _menuPanel;
        private readonly float _resetHeight;

        private bool _isMenuVisible = true;

        public ARHUDController(LocalRigService rigService, ARHUDView hudView, GameObject menuPanel, float resetHeight = 1.5f)
        {
            _rigService = rigService;
            _hudView = hudView;
            _menuPanel = menuPanel;
            _resetHeight = resetHeight;

            // Abonnement aux événements de la vue
            _hudView.OnResetPositionClicked += HandleResetPosition;
            _hudView.OnToggleMenuClicked += HandleToggleMenu;

            // Appliquer l'état initial
            if (_menuPanel != null) _menuPanel.SetActive(_isMenuVisible);
        }

        private void HandleResetPosition()
        {
            // On s'assure qu'on est bien en AR et que les références existent
            if (_rigService.IsVR || _rigService.RigRoot == null || _rigService.Head == null) return;

            // Cible absolue pour la caméra (monde)
            Vector3 targetCameraWorldPos = new Vector3(0f, _resetHeight, 0f);

            // On déplace la racine du même delta que la caméra doit parcourir
            Vector3 delta = targetCameraWorldPos - _rigService.Head.position;
            _rigService.RigRoot.position += delta;
            
            Debug.Log("Position AR réinitialisée à l'origine.");
        }

        private void HandleToggleMenu()
        {
            _isMenuVisible = !_isMenuVisible;
            if (_menuPanel != null)
            {
                _menuPanel.SetActive(_isMenuVisible);
            }
        }

        public void Cleanup()
        {
            _hudView.OnResetPositionClicked -= HandleResetPosition;
            _hudView.OnToggleMenuClicked -= HandleToggleMenu;
        }
    }
}