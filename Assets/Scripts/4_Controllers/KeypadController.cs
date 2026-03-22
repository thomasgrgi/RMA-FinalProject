using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using ARVRMultiplayer.Models;
using System;

namespace ARVRMultiplayer.Controllers
{
    /// <summary>
    /// Lie les clics des XRSimpleInteractables aux actions du réseau.
    /// </summary>
    [RequireComponent(typeof(NetworkKeypadState))]
    public class KeypadController : MonoBehaviour
    {
        // Structure pour associer un caractère à un bouton interactable depuis l'inspecteur
        [Serializable]
        public struct KeypadDigitButton
        {
            public char Digit;
            public XRBaseInteractable Interactable;
        }

        [Header("Boutons Chiffres")]
        [Tooltip("Ajoutez les 10 boutons (0 à 9) et assignez leur caractère")]
        [SerializeField] private KeypadDigitButton[] _digitButtons;

        [Header("Boutons d'Action")]
        [SerializeField] private XRBaseInteractable _okButton;
        [SerializeField] private XRBaseInteractable _clearButton;

        private NetworkKeypadState _state;
        
        // Sécurité pour éviter le double clic rapide (particulièrement utile en AR)
        private float _lastInputTime = 0f;
        private const float DebounceTime = 0.2f;

        private void Awake()
        {
            _state = GetComponent<NetworkKeypadState>();

            // Abonnement des chiffres
            foreach (var btn in _digitButtons)
            {
                if (btn.Interactable != null)
                {
                    char digitToSubmit = btn.Digit; 
                    btn.Interactable.selectEntered.AddListener((args) => HandleDigitPressed(digitToSubmit));
                }
            }

            // Abonnement des actions
            if (_okButton != null) _okButton.selectEntered.AddListener((args) => HandleOkPressed());
            if (_clearButton != null) _clearButton.selectEntered.AddListener((args) => HandleClearPressed());

            // --- ESPACE DE LOGIQUE POUR LE SUCCÈS ---
            _state.OnValidationSuccess += HandleValidationSuccess;
        }

        private void OnDestroy()
        {
            // Désabonnement événement réseau
            if (_state != null)
            {
                _state.OnValidationSuccess -= HandleValidationSuccess;
            }
            
            // Les listeners des boutons sont détruits en même temps que ce composant,
            // donc pas besoin de les désabonner un par un ici.
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
            
            Debug.Log($"[Keypad] Touche {digit} pressée.");
            _state.SubmitDigitRpc(digit);
        }

        private void HandleOkPressed()
        {
            if (!CanPress()) return;
            
            Debug.Log("[Keypad] Touche OK pressée. Validation en cours...");
            _state.ValidateRpc();
        }

        private void HandleClearPressed()
        {
            if (!CanPress()) return;
            
            Debug.Log("[Keypad] Touche CLEAR pressée.");
            _state.ClearRpc();
        }

        private void HandleValidationSuccess()
        {
            // =========================================================
            // C'est ici que vous placerez la logique d'ouverture de porte,
            // de coffre-fort, ou de fin de jeu plus tard !
            // =========================================================
            Debug.Log("🎉 [Keypad] LE CODE EST CORRECT ! Action réussie !");
        }
    }
}