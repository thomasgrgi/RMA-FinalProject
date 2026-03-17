using UnityEngine;
using ARVRMultiplayer.Controllers;

namespace ARVRMultiplayer.Services
{
    /// <summary>
/// Service : instancie color_cube (préfab) au grab, synchronise sa rotation,
/// et le détruit au release.
/// </summary>
public class CubeReplicator : MonoBehaviour
{
    [Header("Références - Injectées via Inspecteur")]
    [SerializeField] private CubeGrabHandler _cubeGrabHandler;
    [SerializeField] private GameObject _colorCubePrefab;       // Le préfab, pas une instance
    [SerializeField] private Transform _tableAnchor;            // Emplacement précis sur la table
    [SerializeField] private Transform _uncoloredCubeTransform;

    private GameObject _colorCubeInstance = null;
    private bool _isReplicating = false;

    private void Awake()
    {
        ValidateDependencies();
    }

    private void OnEnable()
    {
        _cubeGrabHandler.OnCubeGrabbed += StartReplication;
        _cubeGrabHandler.OnCubeReleased += StopReplication;
    }

    private void OnDisable()
    {
        _cubeGrabHandler.OnCubeGrabbed -= StartReplication;
        _cubeGrabHandler.OnCubeReleased -= StopReplication;
    }

    private void Update()
    {
        // Update justifié : synchronisation continue de rotation tant que le cube est tenu
        if (!_isReplicating || _colorCubeInstance == null) return;

        SyncTransform();
    }

    private void StartReplication()
    {
        if (_colorCubeInstance != null)
        {
            Debug.LogWarning("[CubeReplicator] StartReplication appelé mais une instance existe déjà — ignoré.");
            return;
        }

        // Instanciation du préfab à la position de la table
        _colorCubeInstance = Instantiate(_colorCubePrefab, _tableAnchor.position, _uncoloredCubeTransform.rotation);
        _isReplicating = true;

        Debug.Log($"[CubeReplicator] color_cube instancié à {_tableAnchor.position}.");

        // Notifier LayerVisibilityService que l'instance est prête
        NotifyVisibilityService(_colorCubeInstance);
    }

    private void StopReplication()
    {
        _isReplicating = false;

        if (_colorCubeInstance != null)
        {
            Destroy(_colorCubeInstance);
            _colorCubeInstance = null;
            Debug.Log("[CubeReplicator] color_cube détruit.");
        }
        else
        {
            Debug.LogWarning("[CubeReplicator] StopReplication appelé mais aucune instance à détruire.");
        }
    }

    private void SyncTransform()
    {
        _colorCubeInstance.transform.position = _tableAnchor.position;
        _colorCubeInstance.transform.rotation = _uncoloredCubeTransform.rotation;
    }

    /// <summary>
    /// Notifie le LayerVisibilityService avec la nouvelle instance fraîchement créée.
    /// </summary>
    private void NotifyVisibilityService(GameObject instance)
    {
        LayerVisibilityService visibilityService = GetComponent<LayerVisibilityService>();

        if (visibilityService == null)
        {
            Debug.LogError("[CubeReplicator] LayerVisibilityService introuvable sur le même GameObject !");
            return;
        }

        visibilityService.ApplyLayerToInstance(instance);
    }

    private void ValidateDependencies()
    {
        if (_cubeGrabHandler == null)
            Debug.LogError("[CubeReplicator] _cubeGrabHandler non assigné dans l'inspecteur !");
        if (_colorCubePrefab == null)
            Debug.LogError("[CubeReplicator] _colorCubePrefab non assigné dans l'inspecteur !");
        if (_tableAnchor == null)
            Debug.LogError("[CubeReplicator] _tableAnchor non assigné dans l'inspecteur !");
        if (_uncoloredCubeTransform == null)
            Debug.LogError("[CubeReplicator] _uncoloredCubeTransform non assigné dans l'inspecteur !");
    }
}
}