using UnityEngine;
using ARVRMultiplayer.Models;

namespace ARVRMultiplayer.Views
{
    /// <summary>
    /// S'occupe d'appliquer les modifications visuelles (RenderSettings, Lumières, Objets)
    /// lorsque l'état du réseau change. N'a aucune idée de comment l'interaction est déclenchée.
    /// </summary>
    [RequireComponent(typeof(NetworkEnvironmentState))]
    public class EnvironmentView : MonoBehaviour
    {
        [Header("Éléments à modifier")]
        [Tooltip("La lumière à éteindre lors du clic")]
        [SerializeField] private Light _lightToDisable;
        
        [Tooltip("La lumière à allumer lors du clic")]
        [SerializeField] private Light _lightToEnable;
        
        [Tooltip("L'objet (et ses enfants) à faire apparaître lors du clic")]
        [SerializeField] private GameObject _objectToShow;

        [Header("Paramètres normaux (Défaut)")]
        [SerializeField] private float _defaultAmbientIntensity = 1f;
        [SerializeField] private float _defaultReflectionIntensity = 1f;

        [Header("Paramètres alternatifs (Après clic)")]
        [SerializeField] private float _alternateAmbientIntensity = 0f;
        [SerializeField] private float _alternateReflectionIntensity = 0f;

        private NetworkEnvironmentState _state;

        private void Awake()
        {
            _state = GetComponent<NetworkEnvironmentState>();
        }

        private void OnEnable()
        {
            if (_state != null)
                _state.IsAlternateMode.OnValueChanged += HandleEnvironmentChanged;
        }

        private void OnDisable()
        {
            if (_state != null)
                _state.IsAlternateMode.OnValueChanged -= HandleEnvironmentChanged;
        }

        // On s'assure d'appliquer l'état correct au démarrage pour les joueurs qui se connectent en retard
        private void Start()
        {
            if (_state != null && _state.IsSpawned)
            {
                ApplyEnvironmentState(_state.IsAlternateMode.Value);
            }
        }

        private void HandleEnvironmentChanged(bool oldMode, bool newMode)
        {
            ApplyEnvironmentState(newMode);
        }

        private void ApplyEnvironmentState(bool isAlternate)
        {
            Debug.Log($"[EnvironmentView] Application de l'état visuel. Mode Alternatif : {isAlternate}");

            // 1. Modifier l'Environment Lighting (Intensité ambiante) et les Réflexions
            RenderSettings.ambientIntensity = isAlternate ? _alternateAmbientIntensity : _defaultAmbientIntensity;
            RenderSettings.reflectionIntensity = isAlternate ? _alternateReflectionIntensity : _defaultReflectionIntensity;

            // 2. Activer / Désactiver les sources de lumière spécifiques
            if (_lightToDisable != null) _lightToDisable.enabled = !isAlternate;
            if (_lightToEnable != null) _lightToEnable.enabled = isAlternate;

            // 3. Afficher / Masquer le GameObject (et ses enfants)
            if (_objectToShow != null) _objectToShow.SetActive(isAlternate);
        }
    }
}