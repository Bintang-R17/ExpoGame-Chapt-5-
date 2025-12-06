using UnityEngine;

/// <summary>
/// Controls player head and upper body rotation to aim at locked target
/// Rotates head, spine, and weapon holder to face target during lock-on
/// </summary>
public class PlayerAimController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TargetingManager targetingManager;
    [SerializeField] private Transform playerRoot; // Player GameObject transform
    
    [Header("Bone References")]
    [SerializeField] private Transform headBone; // Head bone transform
    [SerializeField] private Transform spineBone; // Spine/Chest bone transform
    [SerializeField] private Transform weaponHolder; // Weapon holder transform (optional)
    
    [Header("Rotation Settings")]
    [SerializeField] private bool enableHeadTracking = true;
    [SerializeField] private bool enableSpineTracking = true;
    [SerializeField] private bool enableWeaponTracking = true;
    
    [Header("Rotation Limits")]
    [SerializeField] [Range(0f, 180f)] private float maxHeadAngle = 70f; // Max head rotation
    [SerializeField] [Range(0f, 180f)] private float maxSpineAngle = 45f; // Max spine rotation
    [SerializeField] [Range(0f, 1f)] private float headRotationWeight = 1f; // Head influence
    [SerializeField] [Range(0f, 1f)] private float spineRotationWeight = 0.5f; // Spine influence
    
    [Header("Smoothing")]
    [SerializeField] private float rotationSpeed = 8f; // Smooth rotation speed
    [SerializeField] private bool smoothTransition = true;
    
    [Header("Target Offset")]
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1f, 0f); // Aim at chest/head height
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private bool logRotations = false;
    
    // State
    private Quaternion originalHeadRotation;
    private Quaternion originalSpineRotation;
    private Quaternion originalWeaponRotation;
    private bool isAiming = false;
    private Transform currentTarget;
    
    void Awake()
    {
        // Auto-find TargetingManager
        if (targetingManager == null)
        {
            targetingManager = FindAnyObjectByType<TargetingManager>();
        }
        
        // Auto-find player root
        if (playerRoot == null)
        {
            playerRoot = transform;
        }
        
        // Auto-find bones if not assigned (search in children)
        if (headBone == null)
        {
            // Try common naming conventions
            headBone = FindBoneRecursive(playerRoot, "Head");
            if (headBone == null)
            {
                headBone = FindBoneRecursive(playerRoot, "head");
            }
        }
        
        if (spineBone == null)
        {
            // Try common naming conventions
            spineBone = FindBoneRecursive(playerRoot, "Spine");
            if (spineBone == null)
            {
                spineBone = FindBoneRecursive(playerRoot, "spine");
            }
            if (spineBone == null)
            {
                spineBone = FindBoneRecursive(playerRoot, "Chest");
            }
        }
        
        // Log warnings if bones not found
        if (headBone == null)
        {
            Debug.LogWarning("[PlayerAimController] Head bone not found! Head tracking disabled.");
            enableHeadTracking = false;
        }
        
        if (spineBone == null)
        {
            Debug.LogWarning("[PlayerAimController] Spine bone not found! Spine tracking disabled.");
            enableSpineTracking = false;
        }
    }
    
    void Start()
    {
        // Store original rotations
        if (headBone != null)
        {
            originalHeadRotation = headBone.localRotation;
        }
        if (spineBone != null)
        {
            originalSpineRotation = spineBone.localRotation;
        }
        if (weaponHolder != null)
        {
            originalWeaponRotation = weaponHolder.localRotation;
        }
    }
    
    void LateUpdate()
    {
        // Check if we have a locked target
        if (targetingManager != null)
        {
            currentTarget = targetingManager.GetCurrentTarget();
            isAiming = currentTarget != null && targetingManager.IsLocked();
        }
        else
        {
            isAiming = false;
            currentTarget = null;
        }
        
        if (isAiming && currentTarget != null)
        {
            AimAtTarget(currentTarget);
        }
        else
        {
            ResetToOriginalPose();
        }
    }
    
    /// <summary>
    /// Rotate bones to aim at target
    /// </summary>
    void AimAtTarget(Transform target)
    {
        Vector3 targetPosition = target.position + targetOffset;
        
        // Rotate head
        if (enableHeadTracking && headBone != null)
        {
            RotateBoneToTarget(headBone, targetPosition, maxHeadAngle, headRotationWeight, ref originalHeadRotation);
        }
        
        // Rotate spine
        if (enableSpineTracking && spineBone != null)
        {
            RotateBoneToTarget(spineBone, targetPosition, maxSpineAngle, spineRotationWeight, ref originalSpineRotation);
        }
        
        // Rotate weapon holder
        if (enableWeaponTracking && weaponHolder != null)
        {
            RotateBoneToTarget(weaponHolder, targetPosition, 180f, 1f, ref originalWeaponRotation);
        }
        
        if (logRotations)
        {
            Debug.Log($"<color=cyan>[PlayerAimController] Aiming at {target.name}</color>");
        }
    }
    
    /// <summary>
    /// Rotate a specific bone toward target with limits
    /// </summary>
    void RotateBoneToTarget(Transform bone, Vector3 targetPosition, float maxAngle, float weight, ref Quaternion originalRotation)
    {
        // Calculate direction to target in local space
        Vector3 directionToTarget = targetPosition - bone.position;
        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
        
        // Convert to local rotation
        Quaternion targetLocalRotation = Quaternion.Inverse(bone.parent.rotation) * lookRotation;
        
        // Clamp rotation angle
        float angle = Quaternion.Angle(originalRotation, targetLocalRotation);
        if (angle > maxAngle)
        {
            targetLocalRotation = Quaternion.Slerp(originalRotation, targetLocalRotation, maxAngle / angle);
        }
        
        // Apply weight
        targetLocalRotation = Quaternion.Slerp(originalRotation, targetLocalRotation, weight);
        
        // Smooth rotation
        if (smoothTransition)
        {
            bone.localRotation = Quaternion.Slerp(bone.localRotation, targetLocalRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            bone.localRotation = targetLocalRotation;
        }
    }
    
    /// <summary>
    /// Reset bones to original pose when not aiming
    /// </summary>
    void ResetToOriginalPose()
    {
        if (headBone != null)
        {
            if (smoothTransition)
            {
                headBone.localRotation = Quaternion.Slerp(headBone.localRotation, originalHeadRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                headBone.localRotation = originalHeadRotation;
            }
        }
        
        if (spineBone != null)
        {
            if (smoothTransition)
            {
                spineBone.localRotation = Quaternion.Slerp(spineBone.localRotation, originalSpineRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                spineBone.localRotation = originalSpineRotation;
            }
        }
        
        if (weaponHolder != null)
        {
            if (smoothTransition)
            {
                weaponHolder.localRotation = Quaternion.Slerp(weaponHolder.localRotation, originalWeaponRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                weaponHolder.localRotation = originalWeaponRotation;
            }
        }
    }
    
    /// <summary>
    /// Recursively search for bone by name
    /// </summary>
    Transform FindBoneRecursive(Transform parent, string boneName)
    {
        if (parent.name.Contains(boneName))
        {
            return parent;
        }
        
        foreach (Transform child in parent)
        {
            Transform result = FindBoneRecursive(child, boneName);
            if (result != null)
            {
                return result;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Get current aiming state
    /// </summary>
    public bool IsAiming()
    {
        return isAiming;
    }
    
    /// <summary>
    /// Get current target
    /// </summary>
    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        // Draw head bone
        if (headBone != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(headBone.position, 0.1f);
            Gizmos.DrawRay(headBone.position, headBone.forward * 0.5f);
        }
        
        // Draw spine bone
        if (spineBone != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spineBone.position, 0.1f);
            Gizmos.DrawRay(spineBone.position, spineBone.forward * 0.5f);
        }
        
        // Draw aim line to target
        if (isAiming && currentTarget != null)
        {
            Vector3 aimPosition = headBone != null ? headBone.position : transform.position;
            Vector3 targetPosition = currentTarget.position + targetOffset;
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(aimPosition, targetPosition);
            Gizmos.DrawWireSphere(targetPosition, 0.2f);
        }
    }
#endif
}
