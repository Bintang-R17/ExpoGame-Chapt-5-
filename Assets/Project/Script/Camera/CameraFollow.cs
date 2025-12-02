using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Settings")]
    public float distance = 5f;
    public float height = 2f;
    public float rotationSpeed = 150f;
    public float verticalSpeed = 100f;

    [Header("Angle Limits")]
    public float minVerticalAngle = -20f;
    public float maxVerticalAngle = 80f;

    [Header("Smoothness")]
    public float positionSmooth = 10f;
    public float rotationSmooth = 8f;

    private float currentYaw = 0f;
    private float currentPitch = 20f;
    private Vector3 currentVelocity;

    void Start()
    {
        if (target != null)
        {
            currentYaw = target.eulerAngles.y;
        }
    }

    void Update()
    {
        if (target == null) return;

        // Input handling
        var gamepad = Gamepad.current;
        var mouse = Mouse.current;

        Vector2 lookInput = Vector2.zero;

        // Gamepad
        if (gamepad != null)
        {
            lookInput = gamepad.rightStick.ReadValue();
        }

        // Mouse (right-click to rotate)
        if (mouse != null && mouse.rightButton.isPressed)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();
            lookInput = mouseDelta * 0.1f;
        }

        // Apply rotation
        currentYaw += lookInput.x * rotationSpeed * Time.deltaTime;
        currentPitch -= lookInput.y * verticalSpeed * Time.deltaTime;
        currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate rotation
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);

        // Calculate target position
        Vector3 focusPoint = target.position + Vector3.up * height;
        Vector3 desiredPosition = focusPoint - (rotation * Vector3.forward * distance);

        // Smooth position
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            1f / positionSmooth
        );

        // Smooth rotation to look at target
        Quaternion desiredRotation = Quaternion.LookRotation(focusPoint - transform.position);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            rotationSmooth * Time.deltaTime
        );
    }
}   