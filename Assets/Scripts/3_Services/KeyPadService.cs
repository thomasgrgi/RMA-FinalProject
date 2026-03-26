using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.Events;
using ARVRMultiplayer.Models;
using System;
using System.Text;

namespace ARVRMultiplayer.Controllers
{
    [RequireComponent(typeof(NetworkKeypadState))]
    public class KeyPadService : MonoBehaviour
    {
        [Serializable]
        public struct KeypadDigitButton
        {
            public char Digit;
            public XRBaseInteractable Interactable;
        }

        [Header("Code Secret")]
        [Tooltip("Le code que le joueur doit saisir pour déclencher OnSuccess.")]
        public string SecretCode;

        [Header("Boutons Chiffres")]
        [SerializeField] private KeypadDigitButton[] _digitButtons;

        [Header("Boutons d'Action")]
        [SerializeField] private XRBaseInteractable _okButton;
        [SerializeField] private XRBaseInteractable _clearButton;

        [Header("Événements")]
        [SerializeField] private UnityEvent _onSuccess;
        [SerializeField] private UnityEvent _onFailure;

        private NetworkKeypadState _state;
        private readonly StringBuilder _localInput = new StringBuilder();

        private float _lastInputTime = 0f;
        private const float DebounceTime = 0.2f;

        private void Awake()
        {
            _state = GetComponent<NetworkKeypadState>();

            foreach (var btn in _digitButtons)
            {
                if (btn.Interactable != null)
                {
                    char digitToSubmit = btn.Digit;
                    btn.Interactable.selectEntered.AddListener((_) => HandleDigitPressed(digitToSubmit));
                }
            }

            if (_okButton != null) _okButton.selectEntered.AddListener((_) => HandleOkPressed());
            if (_clearButton != null) _clearButton.selectEntered.AddListener((_) => HandleClearPressed());
        }

        private bool CanPress()
        {
            if (Time.time - _lastInputTime > DebounceTime)
            {
                _lastInputTime = Time.time;
                return true;
            }
            return false;
        }

        private void HandleDigitPressed(char digit)
        {
            if (!CanPress()) return;

            _localInput.Append(digit);
            Debug.Log($"[Keypad] Touche {digit} pressée. Saisie : {_localInput}");
            _state.SubmitDigitRpc(digit);
        }

        private void HandleOkPressed()
        {
            if (!CanPress()) return;

            string entered = _localInput.ToString();
            Debug.Log($"[Keypad] OK. Saisie : '{entered}' — attendu : '{SecretCode}'");

            // Tell the network regardless, so the display stays in sync.
            _state.ValidateRpc();

            if (entered == SecretCode)
            {
                Debug.Log("🎉 [Keypad] Code correct !");
                _onSuccess?.Invoke();
            }
            else
            {
                Debug.Log("❌ [Keypad] Code incorrect.");
                _onFailure?.Invoke();
            }

            // Reset local buffer after every validation attempt.
            _localInput.Clear();
        }

        private void HandleClearPressed()
        {
            if (!CanPress()) return;

            _localInput.Clear();
            Debug.Log("[Keypad] CLEAR.");
            _state.ClearRpc();
        }
    }
}