using UnityEngine;

namespace ARVRMultiplayer.Services
{
    /// <summary>
    /// Service : configure les caméras VR et AR pour que color_cube
    /// ne soit visible que par le joueur AR.
    /// Utilise le système de Layers Unity (culling mask).
    /// </summary>
    public class LayerVisibilityService : MonoBehaviour
    {
        [Header("Caméras - Injectées via Inspecteur")] 
        [SerializeField] private Camera _vrCamera;
        [SerializeField] private Camera _arCamera;

        [Header("Layer dédié au color_cube (AR only)")] 
        [SerializeField] private string _arOnlyLayerName = "AR_Only";
        
        private int _arOnlyLayer = -1;

        private void Awake()
        {
            ValidateDependencies();
            
            _arOnlyLayer = LayerMask.NameToLayer(_arOnlyLayerName);

            if (_arOnlyLayer == -1)
                Debug.LogError($"[LayerVisibilityService] Le layer '{_arOnlyLayerName}' n'existe pas ! " +
                               $"Créez-le dans Edit > Project Settings > Tags and Layers.");
        }

        private void Start()
        {
            ConfigureCameraCulling();
        }

        /// <summary>
        /// Appelée par CubeReplicator après chaque Instantiate du préfab.
        /// Applique le layer AR_Only à l'instance et ses enfants.
        /// </summary>
        public void ApplyLayerToInstance(GameObject instance)
        {
            if (_arOnlyLayer == -1)
            {
                Debug.LogError("[LayerVisibilityService] Layer invalide, impossible d'appliquer la visibilité.");
                return;
            }

            if (instance == null)
            {
                Debug.LogError("[LayerVisibilityService] ApplyLayerToInstance appelé avec une instance null !");
                return;
            }

            SetLayerRecursively(instance, _arOnlyLayer);
            Debug.Log($"[LayerVisibilityService] Layer '{_arOnlyLayerName}' appliqué à l'instance {instance.name}.");
        }

        private void ConfigureCameraCulling()
        {
            if (_arOnlyLayer == -1) return;

            _vrCamera.cullingMask &= ~(1 << _arOnlyLayer);
            Debug.Log($"[LayerVisibilityService] Caméra VR : layer '{_arOnlyLayerName}' masqué.");

            _arCamera.cullingMask |= (1 << _arOnlyLayer);
            Debug.Log($"[LayerVisibilityService] Caméra AR : layer '{_arOnlyLayerName}' visible.");
        }

        private void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
                SetLayerRecursively(child.gameObject, layer);
        }

        private void ValidateDependencies()
        {
            if (_vrCamera == null)
                Debug.LogError("[LayerVisibilityService] _vrCamera non assignée !");
            if (_arCamera == null)
                Debug.LogError("[LayerVisibilityService] _arCamera non assignée !");
        }
    }
}