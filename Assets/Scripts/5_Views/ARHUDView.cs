using System;
using UnityEngine;
using UnityEngine.UI;

namespace ARVRMultiplayer.Views
{
    /// <summary>
    /// Vue gérant les boutons superposés à l'écran en mode AR.
    /// Ne contient aucune logique de déplacement ou de menu.
    /// </summary>
    public class ARHUDView : MonoBehaviour
    {
        [Header("Boutons HUD")]
        [SerializeField] private Button _resetPositionButton;
        [SerializeField] private Button _toggleMenuButton;

        public event Action OnResetPositionClicked;
        public event Action OnToggleMenuClicked;

        private void Awake()
        {
            if (_resetPositionButton != null)
                _resetPositionButton.onClick.AddListener(() => OnResetPositionClicked?.Invoke());
                
            if (_toggleMenuButton != null)
                _toggleMenuButton.onClick.AddListener(() => OnToggleMenuClicked?.Invoke());
        }

        private void OnDestroy()
        {
            if (_resetPositionButton != null)
                _resetPositionButton.onClick.RemoveAllListeners();
                
            if (_toggleMenuButton != null)
                _toggleMenuButton.onClick.RemoveAllListeners();
        }
    }
}