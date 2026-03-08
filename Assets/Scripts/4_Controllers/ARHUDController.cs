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

            // L'astuce ici : pour que la caméra (téléphone) soit à (0, height, 0),
            // on doit décaler la racine (XR Origin) de l'inverse de la position locale de la caméra.
            Vector3 headLocalPos = _rigService.Head.localPosition;
            _rigService.RigRoot.position = new Vector3(-headLocalPos.x, _resetHeight, -headLocalPos.z);
            
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