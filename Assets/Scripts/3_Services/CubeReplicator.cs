using UnityEngine;
using Unity.Netcode;
using ARVRMultiplayer.Controllers;

namespace ARVRMultiplayer.Services
{
    /// <summary>
    /// Service : active/désactive color_cube via NetworkVariable,
    /// et synchronise sa rotation avec SharedCube via NetworkTransform.
    /// </summary>
    public class CubeReplicator : NetworkBehaviour
    {
        [Header("Références - Injectées via Inspecteur")]
        [SerializeField] private CubeGrabHandler _cubeGrabHandler;
        [SerializeField] private GameObject _colorCube;          // Objet dans la scène, pas un préfab
        [SerializeField] private Transform _tableAnchor;
        [SerializeField] private Transform _uncoloredCubeTransform;

        // NetworkVariable répliqué sur tous les clients
        private NetworkVariable<bool> _isCubeVisible = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private void Awake()
        {
            ValidateDependencies();
        }

        public override void OnNetworkSpawn()
        {
            // Écouter les changements de visibilité sur tous les clients
            _isCubeVisible.OnValueChanged += OnCubeVisibilityChanged;

            // Appliquer l'état initial
            ApplyCubeVisibility(_isCubeVisible.Value);
        }

        public override void OnNetworkDespawn()
        {
            _isCubeVisible.OnValueChanged -= OnCubeVisibilityChanged;
        }

        private void OnEnable()
        {
            _cubeGrabHandler.OnCubeGrabbed += OnGrabbed;
            _cubeGrabHandler.OnCubeReleased += OnReleased;
        }

        private void OnDisable()
        {
            _cubeGrabHandler.OnCubeGrabbed -= OnGrabbed;
            _cubeGrabHandler.OnCubeReleased -= OnReleased;
        }

        private void Update()
        {
            // Update justifié : sync continue de rotation pendant le grab
            if (!_isCubeVisible.Value || _colorCube == null) return;

            SyncTransform();
        }

        private void OnGrabbed()
        {
            if (IsServer) SetCubeVisible(true);
            else SetCubeVisibleServerRpc(true);
        }

        private void OnReleased()
        {
            if (IsServer) SetCubeVisible(false);
            else SetCubeVisibleServerRpc(false);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void SetCubeVisibleServerRpc(bool visible) => SetCubeVisible(visible);

        private void SetCubeVisible(bool visible)
        {
            // Seul le serveur écrit dans NetworkVariable
            _isCubeVisible.Value = visible;
            Debug.Log($"[CubeReplicator] _isCubeVisible = {visible}");
        }

        private void OnCubeVisibilityChanged(bool previous, bool current)
        {
            Debug.Log($"[CubeReplicator] Visibilité changée : {previous} → {current}");
            ApplyCubeVisibility(current);
        }

        private void ApplyCubeVisibility(bool visible)
        {
            _colorCube.SetActive(visible);
            Debug.Log($"[CubeReplicator] color_cube SetActive({visible})");
        }

        private void SyncTransform()
        {
            _colorCube.transform.position = _tableAnchor.position;
            _colorCube.transform.rotation = _uncoloredCubeTransform.rotation;
        }

        private void ValidateDependencies()
        {
            if (_cubeGrabHandler == null)
                Debug.LogError("[CubeReplicator] _cubeGrabHandler non assigné !");
            if (_colorCube == null)
                Debug.LogError("[CubeReplicator] _colorCube non assigné !");
            if (_tableAnchor == null)
                Debug.LogError("[CubeReplicator] _tableAnchor non assigné !");
            if (_uncoloredCubeTransform == null)
                Debug.LogError("[CubeReplicator] _uncoloredCubeTransform non assigné !");
        }
    }
}