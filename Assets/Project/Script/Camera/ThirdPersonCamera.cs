using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target; // Player transform
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -5f); // Offset dari player
    
    [Header("Rotation Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float gamepadSensitivity = 150f;
    [SerializeField] private float rotationSmoothTime = 0.12f;
    
    [Header("Pitch Constraints")]
    [SerializeField] private float minPitch = -20f; // Look down limit
    [SerializeField] private float maxPitch = 60f;  // Look up limit
    
    [Header("Collision Settings")]
    [SerializeField] private bool enableCollision = true;
    [SerializeField] private float collisionRadius = 0.3f;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float collisionSmoothTime = 0.1f;
    
    [Header("Zoom Settings")]
    [SerializeField] private bool enableZoom = true;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 8f;
    [SerializeField] private float zoomSpeed = 2f;
    
    // Camera angles
    private float yaw;   // Horizontal rotation
    private float pitch; // Vertical rotation
    
    // Smooth damping
    private Vector3 currentRotation;
    private Vector3 rotationVelocity;
    private float currentDistance;
    private float distanceVelocity;
    
    // Input
    private Vector2 lookInput;
    private float zoomInput;
    
    // Collision
    private float targetDistance;
    
    void Start()
    {
        // Auto-find player if not assigned
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogError("❌ ThirdPersonCamera: Player tidak ditemukan! Tag player dengan 'Player' atau assign manual di Inspector.");
                enabled = false;
                return;
            }
        }
        
        // Initialize camera angles from current rotation
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        
        // Normalize pitch to -180 to 180 range
        if (pitch > 180f)
            pitch -= 360f;
        
        currentRotation = new Vector3(pitch, yaw);
        
        // Initialize distance
        targetDistance = offset.magnitude;
        currentDistance = targetDistance;
        
        // Lock cursor (optional - uncomment if you want)
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        HandleRotation();
        HandleZoom();
        HandlePosition();
    }
    
    void HandleRotation()
    {
        // Get input (handled by Input System callbacks)
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;
        
        // Check for gamepad input (right stick)
        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            Vector2 gamepadLook = gamepad.rightStick.ReadValue();
            if (gamepadLook.sqrMagnitude > 0.01f)
            {
                mouseX = gamepadLook.x * gamepadSensitivity * Time.deltaTime;
                mouseY = gamepadLook.y * gamepadSensitivity * Time.deltaTime;
            }
        }
        
        // Apply rotation
        yaw += mouseX;
        pitch -= mouseY; // Invert Y axis for natural look
        
        // Clamp pitch
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        
        // Smooth rotation
        Vector3 targetRotation = new Vector3(pitch, yaw);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref rotationVelocity, rotationSmoothTime);
    }
    
    void HandleZoom()
    {
        if (!enableZoom) return;
        
        // Mouse scroll wheel
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            targetDistance -= scroll * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minZoom, maxZoom);
        }
        
        // Gamepad triggers (optional - LT/RT for zoom)
        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            float lt = gamepad.leftTrigger.ReadValue();
            float rt = gamepad.rightTrigger.ReadValue();
            
            if (lt > 0.1f || rt > 0.1f)
            {
                targetDistance += (lt - rt) * zoomSpeed * Time.deltaTime;
                targetDistance = Mathf.Clamp(targetDistance, minZoom, maxZoom);
            }
        }
    }
    
    void HandlePosition()
    {
        // Calculate desired position
        Quaternion rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);
        Vector3 direction = rotation * -Vector3.forward;
        
        // Desired camera distance
        float desiredDistance = targetDistance;
        
        // Camera collision detection
        if (enableCollision)
        {
            Vector3 targetPosition = target.position + offset;
            RaycastHit hit;
            
            if (Physics.SphereCast(targetPosition, collisionRadius, direction, out hit, targetDistance, collisionMask))
            {
                // Reduce distance if there's an obstacle
                desiredDistance = Mathf.Clamp(hit.distance - collisionRadius, minZoom, targetDistance);
            }
        }
        
        // Smooth distance transition
        currentDistance = Mathf.SmoothDamp(currentDistance, desiredDistance, ref distanceVelocity, collisionSmoothTime);
        
        // Final position
        Vector3 targetPos = target.position + offset;
        Vector3 finalPosition = targetPos + direction * currentDistance;
        
        transform.position = finalPosition;
        transform.rotation = rotation;
    }
    
    // Input System Callbacks
    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }
    
    public void OnZoom(InputValue value)
    {
        zoomInput = value.Get<float>();
    }
    
    // Public methods for external control
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    public void SetSensitivity(float mouse, float gamepad)
    {
        mouseSensitivity = mouse;
        gamepadSensitivity = gamepad;
    }
    
    public void SetZoomDistance(float distance)
    {
        targetDistance = Mathf.Clamp(distance, minZoom, maxZoom);
    }
    
    public float GetYaw()
    {
        return yaw;
    }
    
    public float GetPitch()
    {
        return pitch;
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (target == null || !enableCollision) return;
        
        // Draw collision sphere
        Gizmos.color = Color.yellow;
        Vector3 targetPos = target.position + offset;
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 direction = rotation * -Vector3.forward;
        Gizmos.DrawWireSphere(targetPos + direction * currentDistance, collisionRadius);
        
        // Draw line from target to camera
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(targetPos, transform.position);
    }
}
