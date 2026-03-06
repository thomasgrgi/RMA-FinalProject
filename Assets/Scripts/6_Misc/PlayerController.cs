using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 0.06f;
    public float sprintSpeed = 0.10f;
    public float jumpHeight = 0.015f;
    public float gravity = -0.20f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 0.15f;
    public Transform cameraTransform;

    private CharacterController _controller;
    private Vector3 _velocity;
    private float _xRotation = 0f;

    void Start()
    {
        _controller = GetComponent<CharacterController>();

        if (cameraTransform == null)
            cameraTransform = Camera.main?.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        ApplyGravity();
    }

    void HandleMouseLook()
    {
        Vector2 delta = Mouse.current.delta.ReadValue();

        float mouseX = delta.x * mouseSensitivity;
        float mouseY = delta.y * mouseSensitivity;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        var kb = Keyboard.current;

        // Grounded check & reset downward velocity
        if (_controller.isGrounded && _velocity.y < 0f)
            _velocity.y = -0.002f;

        // WASD
        float h = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float v = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

        Vector3 move = transform.right * h + transform.forward * v;
        if (move.magnitude > 1f) move.Normalize();

        float speed = kb.leftShiftKey.isPressed ? sprintSpeed : moveSpeed;
        _controller.Move(move * speed * Time.deltaTime);

        // Jump
        if (kb.spaceKey.wasPressedThisFrame && _controller.isGrounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    void ApplyGravity()
    {
        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }
}