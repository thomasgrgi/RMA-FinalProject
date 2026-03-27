using UnityEngine;
using ARVRMultiplayer.Models;

namespace ARVRMultiplayer.Views
{
    /// <summary>
    /// S'attache aux Rigs locaux ET au NetworkPlayer pour gérer les lumières personnelles.
    /// Écoute l'état global de l'environnement de façon totalement découplée.
    /// </summary>
    public class PlayerFlashlightView : MonoBehaviour
    {
        [Tooltip("La lumière attachée à la main (VR)")]
        [SerializeField] private Light _vrHandLight;
        
        [Tooltip("La lumière attachée à la tête (AR)")]
        [SerializeField] private Light _arHeadLight;

        // Référence optionnelle (présente uniquement sur le NetworkPlayer)
        private NetworkAvatarState _networkAvatar;

        private void Awake()
        {
            _networkAvatar = GetComponent<NetworkAvatarState>();
            
            // On éteint les lumières par défaut au démarrage
            if (_vrHandLight != null) _vrHandLight.enabled = false;
            if (_arHeadLight != null) _arHeadLight.enabled = false;
        }

        private void OnEnable()
        {
            // S'abonne à l'événement de la scène
            NetworkEnvironmentState.OnAlternateModeChanged += HandleEnvironmentChanged;
        }

        private void OnDisable()
        {
            NetworkEnvironmentState.OnAlternateModeChanged -= HandleEnvironmentChanged;
        }

        private void HandleEnvironmentChanged(bool isDark)
        {
            bool isVR = DetermineIfVR();

            // Allume la lumière VR si on est en VR, sinon la lumière AR
            if (_vrHandLight != null) _vrHandLight.enabled = isDark && isVR;
            if (_arHeadLight != null) _arHeadLight.enabled = isDark && !isVR;
        }

        private bool DetermineIfVR()
        {
            // Si on est sur l'avatar réseau (celui qu'on voit des autres joueurs), on lit la variable synchronisée
            if (_networkAvatar != null)
            {
                return _networkAvatar.IsVR.Value;
            }
            
            // Si on est sur le Rig local physique, on déduit le type selon la lumière qu'on a renseignée
            return _vrHandLight != null;
        }
    }
}