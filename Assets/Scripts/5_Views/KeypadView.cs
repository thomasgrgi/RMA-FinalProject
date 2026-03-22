using UnityEngine;
using TMPro;
using System.Collections;
using ARVRMultiplayer.Models;
using Unity.Collections;

namespace ARVRMultiplayer.Views
{
    /// <summary>
    /// Gère l'affichage visuel du clavier (écran et clignotement des boutons).
    /// </summary>
    [RequireComponent(typeof(NetworkKeypadState))]
    public class KeypadView : MonoBehaviour
    {
        [Header("Écran")]
        [SerializeField] private TextMeshProUGUI _displayText;

        [Header("Feedback Visuel")]
        [Tooltip("Glissez ici tous les MeshRenderers de vos boutons pour les faire clignoter")]
        [SerializeField] private MeshRenderer[] _buttonRenderers; 
        
        [SerializeField] private Color _errorColor = Color.red;
        [SerializeField] private Color _successColor = Color.green;
        [SerializeField] private float _blinkDuration = 0.5f;

        private NetworkKeypadState _state;
        private Color[] _originalColors;

        private void Awake()
        {
            _state = GetComponent<NetworkKeypadState>();
            
            // Sauvegarde des couleurs initiales des boutons pour le clignotement
            _originalColors = new Color[_buttonRenderers.Length];
            for (int i = 0; i < _buttonRenderers.Length; i++)
            {
                if (_buttonRenderers[i] != null)
                {
                    _originalColors[i] = _buttonRenderers[i].material.color;
                }
            }
        }

        private void OnEnable()
        {
            if (_state != null)
            {
                _state.CurrentInput.OnValueChanged += HandleInputChanged;
                _state.OnValidationFailure += HandleFailure;
                _state.OnValidationSuccess += HandleSuccess;
            }
        }

        private void OnDisable()
        {
            if (_state != null)
            {
                _state.CurrentInput.OnValueChanged -= HandleInputChanged;
                _state.OnValidationFailure -= HandleFailure;
                _state.OnValidationSuccess -= HandleSuccess;
            }
        }

        private void HandleInputChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
        {
            if (_displayText != null)
            {
                _displayText.text = newValue.ToString();
            }
        }

        private void HandleFailure()
        {
            StopAllCoroutines();
            StartCoroutine(BlinkRoutine(_errorColor));
        }

        private void HandleSuccess()
        {
            StopAllCoroutines();
            StartCoroutine(BlinkRoutine(_successColor));
        }

        private IEnumerator BlinkRoutine(Color targetColor)
        {
            // Appliquer la couleur de feedback
            SetButtonsColor(targetColor);

            yield return new WaitForSeconds(_blinkDuration);

            // Remettre la couleur d'origine
            for (int i = 0; i < _buttonRenderers.Length; i++)
            {
                if (_buttonRenderers[i] != null)
                {
                    _buttonRenderers[i].material.color = _originalColors[i];
                }
            }
        }

        private void SetButtonsColor(Color color)
        {
            foreach (var renderer in _buttonRenderers)
            {
                if (renderer != null) renderer.material.color = color;
            }
        }
    }
}