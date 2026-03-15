using UnityEngine;

public class AvatarControllerAR : MonoBehaviour
{
    [Header("AR Phone Tracking Source")]
    [SerializeField] private Transform phoneTransform;

    [Header("IK Targets")]
    [SerializeField] private Transform IKLeftHand;
    [SerializeField] private Transform IKRightHand;
    [SerializeField] private Transform IKTablet;

    [Header("Body Offsets")]
    [SerializeField] private Vector3 headBodyOffset = new Vector3(0f, -7f, 0f);
    [SerializeField] private Vector3 headPositionOffset = new Vector3(0f, 0.05f, -1.25f);

    [Header("Hand Grip Offsets (local to phone)")]
    [SerializeField] private Vector3 leftHandOffset = new Vector3(-0.6f, -0.25f, 0f);
    [SerializeField] private Vector3 rightHandOffset = new Vector3(0.6f, -0.25f, 0f);
    [SerializeField] private Vector3 leftHandRotation;
    [SerializeField] private Vector3 rightHandRotation;

    [Header("Smoothing")]
    [SerializeField] private float turnSmoothness = 5f;
    [SerializeField, Range(1f, 20f)] private float trackingSmoothness = 12f;

    private Vector3 _smoothPosition;
    private Quaternion _smoothRotation;
    private Vector3 _headWorldPosition;

    private void Start()
    {
        if (phoneTransform == null)
        {
            Debug.LogError("[AvatarControllerAR] phoneTransform is not assigned.", this);
            enabled = false;
            return;
        }

        _smoothPosition = phoneTransform.position;
        _smoothRotation = phoneTransform.rotation;
    }

    private void LateUpdate()
    {
        SmoothPhonePose();
        ComputeHeadPosition();
        DriveTablet();
        DriveBody();
        DriveHands();
    }

    private void SmoothPhonePose()
    {
        float t = Time.deltaTime * trackingSmoothness;
        _smoothPosition = Vector3.Lerp(_smoothPosition, phoneTransform.position, t);
        _smoothRotation = Quaternion.Slerp(_smoothRotation, phoneTransform.rotation, t);
    }

    private void ComputeHeadPosition()
    {
        _headWorldPosition = _smoothPosition + _smoothRotation * headPositionOffset;
    }

    private void DriveTablet()
    {
        if (IKTablet == null) return;
        IKTablet.position = _smoothPosition;
        IKTablet.rotation = _smoothRotation;
    }

    private void DriveBody()
    {
        transform.position = _headWorldPosition + headBodyOffset;

        Vector3 flatForward = Vector3.ProjectOnPlane(
            _smoothRotation * Vector3.forward, Vector3.up).normalized;

        if (flatForward != Vector3.zero)
        {
            transform.forward = Vector3.Lerp(
                transform.forward, flatForward, Time.deltaTime * turnSmoothness);
        }
    }

    private void DriveHands()
    {
        if (IKLeftHand != null)
        {
            IKLeftHand.position = _smoothPosition + _smoothRotation * leftHandOffset;
            // Phone rotation * local euler offset, same pattern as MapTransform in the VR script
            IKLeftHand.rotation = _smoothRotation * Quaternion.Euler(leftHandRotation);
        }

        if (IKRightHand != null)
        {
            IKRightHand.position = _smoothPosition + _smoothRotation * rightHandOffset;
            IKRightHand.rotation = _smoothRotation * Quaternion.Euler(rightHandRotation);
        }
    }
}