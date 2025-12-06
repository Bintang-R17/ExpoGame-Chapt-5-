using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Camera Manager with Free and Locked modes
/// Free: Manual camera control via right stick/mouse
/// Locked: Auto-focus on current target
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Player target to follow

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
    
    [Header("Lock-On Settings")]
    public CameraMode cameraMode = CameraMode.Free;
    public float lockTransitionSpeed = 0.15f; // Smooth transition time (0.12-0.2s)
    public LayerMask targetLayer; // Enemy layer for lock-on
    public float lockOnRange = 20f; // Max range to lock onto targets
    
    [Header("Locked Mode Manual Control")]
    public bool allowManualAdjustmentInLock = true;
    public float lockedHorizontalLimit = 30f; // Degrees left/right from target
    public float lockedVerticalLimit = 20f; // Degrees up/down from target
    public float lockedRotationSpeed = 80f; // Slower rotation in locked mode
    public float lockedVerticalSpeed = 60f;
    
    [Header("Debug")]
    public bool showDebugGizmos = true;
    public Color lockGizmoColor = Color.red;

    private float currentYaw = 0f;
    private float currentPitch = 20f;
    private Vector3 currentVelocity;
    
    // Lock-on state
    private Transform currentLockTarget;
    private Quaternion targetRotation;
    private float transitionTime = 0f;
    private float baseLockedYaw = 0f; // Base yaw when locked (toward target)
    private float baseLockedPitch = 0f; // Base pitch when locked
    private float lockedYawOffset = 0f; // Manual offset from base
    private float lockedPitchOffset = 0f; // Manual offset from base

    void Start()
    {
        if (target != null)
        {
            currentYaw = target.eulerAngles.y;
        }
        
        // Initialize target rotation
        targetRotation = transform.rotation;
    }

    void Update()
    {
        if (target == null) return;
        
        // Toggle lock mode with L key (for testing)
        // Note: In production, use TargetingManager.SetDefaultTarget() instead
        if (Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame)
        {
            ToggleLockMode();
        }

        // Handle input based on camera mode
        switch (cameraMode)
        {
            case CameraMode.Free:
                HandleFreeMode();
                break;
            case CameraMode.Locked:
                HandleLockedMode();
                break;
        }
    }
    
    /// <summary>
    /// Handle Free mode camera controls
    /// </summary>
    void HandleFreeMode()
    {
        // Input handling
        var gamepad = Gamepad.current;
        var mouse = Mouse.current;

        Vector2 lookInput = Vector2.zero;

        // Gamepad right stick
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
    
    /// <summary>
    /// Handle Locked mode - camera auto-focuses on target with limited manual control
    /// </summary>
    void HandleLockedMode()
    {
        if (currentLockTarget == null)
        {
            // No lock target, switch back to free mode
            cameraMode = CameraMode.Free;
            return;
        }
        
        // Calculate base direction to lock target (auto-focus)
        Vector3 directionToTarget = currentLockTarget.position - target.position;
        directionToTarget.y = 0f; // Keep horizontal
        
        if (directionToTarget.sqrMagnitude > 0.01f)
        {
            // Calculate desired base yaw to face target
            float targetYaw = Quaternion.LookRotation(directionToTarget).eulerAngles.y;
            
            // Smooth transition to target yaw (base position)
            baseLockedYaw = Mathf.LerpAngle(baseLockedYaw, targetYaw, Time.deltaTime / lockTransitionSpeed);
            
            // Calculate base pitch to look at target
            Vector3 focusPoint = target.position + Vector3.up * height;
            Vector3 toTarget = currentLockTarget.position - focusPoint;
            float targetPitch = Mathf.Atan2(toTarget.y, new Vector2(toTarget.x, toTarget.z).magnitude) * Mathf.Rad2Deg;
            
            // Smooth pitch transition (base position)
            baseLockedPitch = Mathf.Lerp(baseLockedPitch, targetPitch, Time.deltaTime / lockTransitionSpeed);
        }
        
        // Allow manual adjustment in locked mode
        if (allowManualAdjustmentInLock)
        {
            // Input handling
            var gamepad = Gamepad.current;
            var mouse = Mouse.current;

            Vector2 lookInput = Vector2.zero;

            // Gamepad right stick
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

            // Apply manual offset with limits
            lockedYawOffset += lookInput.x * lockedRotationSpeed * Time.deltaTime;
            lockedYawOffset = Mathf.Clamp(lockedYawOffset, -lockedHorizontalLimit, lockedHorizontalLimit);
            
            lockedPitchOffset -= lookInput.y * lockedVerticalSpeed * Time.deltaTime;
            lockedPitchOffset = Mathf.Clamp(lockedPitchOffset, -lockedVerticalLimit, lockedVerticalLimit);
        }
        
        // Combine base position + manual offset
        currentYaw = baseLockedYaw + lockedYawOffset;
        currentPitch = baseLockedPitch + lockedPitchOffset;
        
        // Clamp pitch to absolute limits
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
    
    /// <summary>
    /// Toggle between Free and Locked camera modes
    /// </summary>
    public void ToggleLockMode()
    {
        if (cameraMode == CameraMode.Free)
        {
            // Try to find nearest target to lock onto
            Transform nearestTarget = FindNearestTarget();
            if (nearestTarget != null)
            {
                SetLockTarget(nearestTarget);
            }
            else
            {
                Debug.Log("No valid target found for lock-on");
            }
        }
        else
        {
            ClearLock();
        }
    }
    
    /// <summary>
    /// Set lock target and switch to Locked mode
    /// </summary>
    public void SetLockTarget(Transform lockTarget)
    {
        if (lockTarget == null)
        {
            Debug.LogWarning("Cannot set null lock target");
            return;
        }
        
        currentLockTarget = lockTarget;
        cameraMode = CameraMode.Locked;
        transitionTime = 0f;
        
        // Reset manual offsets when locking onto new target
        lockedYawOffset = 0f;
        lockedPitchOffset = 0f;
        
        // Initialize base angles toward target
        Vector3 directionToTarget = lockTarget.position - target.position;
        directionToTarget.y = 0f;
        if (directionToTarget.sqrMagnitude > 0.01f)
        {
            baseLockedYaw = Quaternion.LookRotation(directionToTarget).eulerAngles.y;
        }
        
        Debug.Log($"Camera locked onto: {lockTarget.name}");
    }
    
    /// <summary>
    /// Clear lock and return to Free mode
    /// </summary>
    public void ClearLock()
    {
        currentLockTarget = null;
        cameraMode = CameraMode.Free;
        transitionTime = 0f;
        
        // Reset offsets
        lockedYawOffset = 0f;
        lockedPitchOffset = 0f;
        
        Debug.Log("Camera lock cleared - Free mode");
    }
    
    /// <summary>
    /// Find nearest target within range on target layer
    /// </summary>
    Transform FindNearestTarget()
    {
        if (target == null) return null;
        
        Collider[] targets = Physics.OverlapSphere(target.position, lockOnRange, targetLayer);
        
        Transform nearest = null;
        float nearestDistance = float.MaxValue;
        
        foreach (Collider col in targets)
        {
            // Skip self
            if (col.transform == target) continue;
            
            float distance = Vector3.Distance(target.position, col.transform.position);
            
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = col.transform;
            }
        }
        
        return nearest;
    }
    
    /// <summary>
    /// Get current camera mode
    /// </summary>
    public CameraMode GetCameraMode()
    {
        return cameraMode;
    }
    
    /// <summary>
    /// Get current lock target
    /// </summary>
    public Transform GetLockTarget()
    {
        return currentLockTarget;
    }
    
    /// <summary>
    /// Check if camera is locked onto a target
    /// </summary>
    public bool IsLocked()
    {
        return cameraMode == CameraMode.Locked && currentLockTarget != null;
    }
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        // Draw lock-on range
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(target.position, lockOnRange);
        }
        
        // Draw current lock target
        if (currentLockTarget != null && cameraMode == CameraMode.Locked)
        {
            Gizmos.color = lockGizmoColor;
            
            // Draw line from camera to target
            Gizmos.DrawLine(transform.position, currentLockTarget.position);
            
            // Draw sphere at target
            Gizmos.DrawWireSphere(currentLockTarget.position, 0.5f);
            
            // Draw connection from player to target
            if (target != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(target.position, currentLockTarget.position);
            }
        }
    }
#endif
}

/// <summary>
/// Camera mode enum
/// </summary>
public enum CameraMode
{
    Free,    // Manual control via input
    Locked   // Auto-focus on target
}