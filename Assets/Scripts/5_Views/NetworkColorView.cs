using UnityEngine;
using ARVRMultiplayer.Models;

namespace ARVRMultiplayer.Views
{
    /// <summary>
    /// S'occupe uniquement d'appliquer la couleur au matériau du cube
    /// lorsque le modèle réseau signale un changement.
    /// </summary>
    [RequireComponent(typeof(NetworkColorState))]
    public class NetworkColorView : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;
        private NetworkColorState _state;

        private void Awake()
        {
            _state = GetComponent<NetworkColorState>();
            if (_renderer == null) _renderer = GetComponent<Renderer>();
        }

        private void OnEnable()
        {
            if (_state != null)
                _state.CurrentColor.OnValueChanged += HandleColorChanged;
        }

        private void OnDisable()
        {
            if (_state != null)
                _state.CurrentColor.OnValueChanged -= HandleColorChanged;
        }

        private void HandleColorChanged(Color oldColor, Color newColor)
        {
            if (_renderer != null)
            {
                _renderer.material.color = newColor;
            }
        }
    }
}