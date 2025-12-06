using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages lock-on targeting with D-pad cycling
/// Detects targets in sphere radius and cycles them in angular order
/// </summary>
public class TargetingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private TargetIndicatorPool indicatorPool;
    
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 12f;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private float updateInterval = 0.2f; // Update target list frequency
    
    [Header("Cycling Settings")]
    [SerializeField] private float cycleCooldown = 0.18f;
    [SerializeField] private bool enableCycling = true;
    
    [Header("Auto-Lock Settings")]
    [SerializeField] private bool autoLockOnDetection = true; // Auto-lock saat detect enemy
    [SerializeField] private bool autoSwitchOnTargetDeath = true; // Auto-switch saat target mati
    
    [Header("Visual Indicator")]
    [SerializeField] private bool showIndicator = true;
    [SerializeField] private float indicatorYOffset = 2f; // Di atas kepala target
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private bool logTargetChanges = true;
    
    // State
    private List<TargetInfo> sortedTargets = new List<TargetInfo>();
    private int currentTargetIndex = -1;
    private float nextCycleTime = 0f;
    private float nextUpdateTime = 0f;
    private bool wasLocked = false; // Track previous lock state
    
    // Visual indicator
    private TargetIndicator currentIndicator;
    
    // Input
    private PlayerControllers.PlayerInput playerInputActions;
    
    void Awake()
    {
        // Auto-find references
        if (player == null)
        {
            player = transform;
        }
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (cameraFollow == null)
        {
            cameraFollow = FindAnyObjectByType<CameraFollow>();
        }
        
        // Auto-find indicator pool
        if (indicatorPool == null)
        {
            indicatorPool = GetComponentInChildren<TargetIndicatorPool>();
            if (indicatorPool == null)
            {
                // Create indicator pool if not found
                GameObject poolObj = new GameObject("IndicatorPool");
                poolObj.transform.SetParent(transform);
                indicatorPool = poolObj.AddComponent<TargetIndicatorPool>();
            }
        }
        
        // Setup input
        playerInputActions = new PlayerControllers.PlayerInput();
    }
    
    void OnEnable()
    {
        if (playerInputActions != null)
            playerInputActions.Enable();
    }
    
    void OnDisable()
    {
        if (playerInputActions != null)
            playerInputActions.Disable();
    }
    
    void Update()
    {
        if (!enableCycling) return;
        
        // Update target list periodically
        if (Time.time >= nextUpdateTime)
        {
            UpdateTargetList();
            
            // Auto-lock behavior
            HandleAutoLock();
            
            nextUpdateTime = Time.time + updateInterval;
        }
        
        // Handle D-pad input for cycling using Gamepad API
        HandleCycleInput();
    }
    
    /// <summary>
    /// Handle D-pad input for target cycling
    /// Uses Gamepad.current D-pad for direct input
    /// </summary>
    void HandleCycleInput()
    {
        if (Time.time < nextCycleTime) return; // Cooldown active
        
        var gamepad = Gamepad.current;
        if (gamepad == null) return;
        
        // D-pad right
        if (gamepad.dpad.right.wasPressedThisFrame)
        {
            CycleRight();
            nextCycleTime = Time.time + cycleCooldown;
        }
        // D-pad left
        else if (gamepad.dpad.left.wasPressedThisFrame)
        {
            CycleLeft();
            nextCycleTime = Time.time + cycleCooldown;
        }
    }
    
    /// <summary>
    /// Update list of valid targets around player
    /// </summary>
    void UpdateTargetList()
    {
        sortedTargets.Clear();
        
        if (player == null || mainCamera == null) return;
        
        // Detect all targets in sphere
        Collider[] detectedColliders = Physics.OverlapSphere(player.position, detectionRadius, targetLayer);
        
        if (detectedColliders.Length == 0)
        {
            // No targets found
            if (currentTargetIndex >= 0)
            {
                ClearLock();
            }
            return;
        }
        
        // Get camera forward on XZ plane
        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();
        
        // Build target info list
        foreach (Collider col in detectedColliders)
        {
            // Skip self
            if (col.transform == player) continue;
            
            // Skip dead enemies
            EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();
            if (enemyHealth != null && enemyHealth.IsDead())
            {
                continue; // Skip dead enemies
            }
            
            // Calculate direction to target on XZ plane
            Vector3 dirToTarget = col.transform.position - player.position;
            dirToTarget.y = 0f;
            
            if (dirToTarget.sqrMagnitude < 0.01f) continue; // Skip if too close
            
            dirToTarget.Normalize();
            
            // Calculate signed angle from camera forward
            float angle = Vector3.SignedAngle(cameraForward, dirToTarget, Vector3.up);
            
            // Create target info
            TargetInfo info = new TargetInfo
            {
                transform = col.transform,
                angle = angle,
                distance = Vector3.Distance(player.position, col.transform.position)
            };
            
            sortedTargets.Add(info);
        }
        
        // Sort targets by angle (left to right)
        sortedTargets = sortedTargets.OrderBy(t => t.angle).ToList();
        
        // Validate current target index
        if (currentTargetIndex >= sortedTargets.Count)
        {
            currentTargetIndex = -1;
        }
        
        // Check if current target still exists
        if (currentTargetIndex >= 0)
        {
            Transform currentTarget = GetCurrentTarget();
            if (currentTarget == null || !IsTargetInList(currentTarget))
            {
                if (logTargetChanges)
                {
                    Debug.Log($"<color=orange>[Targeting] Current target lost! Auto-switching...</color>");
                }
                currentTargetIndex = -1;
                
                // Auto-switch ke target lain jika ada
                if (autoSwitchOnTargetDeath && sortedTargets.Count > 0)
                {
                    AutoSwitchToNearestTarget();
                }
            }
        }
    }
    
    /// <summary>
    /// Check if target exists in sorted list
    /// </summary>
    bool IsTargetInList(Transform target)
    {
        foreach (var info in sortedTargets)
        {
            if (info.transform == target)
                return true;
        }
        return false;
    }
    
    /// <summary>
    /// Cycle to next target on the right
    /// </summary>
    public void CycleRight()
    {
        if (sortedTargets.Count == 0)
        {
            ClearLock();
            return;
        }
        
        // Move to next index
        currentTargetIndex++;
        
        // Wrap around
        if (currentTargetIndex >= sortedTargets.Count)
        {
            currentTargetIndex = 0;
        }
        
        SelectCurrentTarget();
    }
    
    /// <summary>
    /// Cycle to previous target on the left
    /// </summary>
    public void CycleLeft()
    {
        if (sortedTargets.Count == 0)
        {
            ClearLock();
            return;
        }
        
        // Move to previous index
        currentTargetIndex--;
        
        // Wrap around
        if (currentTargetIndex < 0)
        {
            currentTargetIndex = sortedTargets.Count - 1;
        }
        
        SelectCurrentTarget();
    }
    
    /// <summary>
    /// Select current target and update camera lock
    /// </summary>
    void SelectCurrentTarget()
    {
        Transform target = GetCurrentTarget();
        
        if (target == null)
        {
            ClearLock();
            return;
        }
        
        // Log target selection
        if (logTargetChanges)
        {
            float cooldownRemaining = Mathf.Max(0f, nextCycleTime - Time.time);
            Debug.Log($"[Targeting] Selected: {target.name} | Index: {currentTargetIndex}/{sortedTargets.Count} | Cooldown: {cooldownRemaining:F2}s");
        }
        
        // Update camera lock
        if (cameraFollow != null)
        {
            cameraFollow.SetLockTarget(target);
        }
        
        // Update visual indicator
        UpdateIndicator(target);
    }
    
    /// <summary>
    /// Get current locked target
    /// </summary>
    public Transform GetCurrentTarget()
    {
        if (currentTargetIndex < 0 || currentTargetIndex >= sortedTargets.Count)
            return null;
        
        return sortedTargets[currentTargetIndex].transform;
    }
    
    /// <summary>
    /// Set default target (first in list or nearest)
    /// </summary>
    public void SetDefaultTarget()
    {
        if (sortedTargets.Count == 0)
        {
            UpdateTargetList();
        }
        
        if (sortedTargets.Count == 0)
        {
            ClearLock();
            return;
        }
        
        // Select first target (closest to camera forward)
        currentTargetIndex = 0;
        SelectCurrentTarget();
    }
    
    /// <summary>
    /// Clear lock and reset target
    /// </summary>
    public void ClearLock()
    {
        currentTargetIndex = -1;
        
        if (cameraFollow != null)
        {
            cameraFollow.ClearLock();
        }
        
        // Clear visual indicator
        ClearIndicator();
        
        if (logTargetChanges)
        {
            Debug.Log("[Targeting] Lock cleared - no targets available");
        }
    }
    
    /// <summary>
    /// Update visual indicator for target
    /// </summary>
    void UpdateIndicator(Transform target)
    {
        if (!showIndicator || indicatorPool == null || target == null) 
        {
            if (logTargetChanges)
            {
                Debug.Log($"[Targeting] UpdateIndicator skipped - showIndicator:{showIndicator}, pool:{indicatorPool != null}, target:{target != null}");
            }
            return;
        }
        
        // Check if pool is enabled (prefab assigned)
        if (!indicatorPool.enabled)
        {
            if (logTargetChanges)
            {
                Debug.LogWarning("[Targeting] IndicatorPool is disabled (prefab not assigned) - skipping indicator update");
            }
            return;
        }
        
        // Return previous indicator to pool if exists
        if (currentIndicator != null)
        {
            indicatorPool.ReturnIndicator(currentIndicator);
            currentIndicator = null;
            Debug.Log($"[Targeting] Returned previous indicator to pool");
        }
        
        // Get new indicator from pool
        currentIndicator = indicatorPool.GetIndicator();
        
        if (currentIndicator != null)
        {
            currentIndicator.SetTarget(target);
            currentIndicator.SetYOffset(indicatorYOffset);
            
            Debug.Log($"<color=cyan>[Targeting] INDICATOR SPAWNED on: {target.name}</color>");
            Debug.Log($"<color=cyan>  → Position: {currentIndicator.transform.position}</color>");
            Debug.Log($"<color=cyan>  → Target Position: {target.position}</color>");
            Debug.Log($"<color=cyan>  → Y Offset: {indicatorYOffset}</color>");
            Debug.Log($"<color=cyan>  → Active: {currentIndicator.gameObject.activeSelf}</color>");
        }
        else
        {
            Debug.LogError($"[Targeting] Failed to get indicator from pool!");
        }
    }
    
    /// <summary>
    /// Clear visual indicator
    /// </summary>
    void ClearIndicator()
    {
        if (currentIndicator != null && indicatorPool != null)
        {
            Debug.Log($"<color=yellow>[Targeting] Clearing indicator from: {(currentIndicator.GetTarget() != null ? currentIndicator.GetTarget().name : "null")}</color>");
            indicatorPool.ReturnIndicator(currentIndicator);
            currentIndicator = null;
        }
    }
    
    /// <summary>
    /// Handle auto-lock behavior
    /// </summary>
    void HandleAutoLock()
    {
        if (!autoLockOnDetection) return;
        
        bool isCurrentlyLocked = HasTarget();
        
        // Auto-lock saat pertama kali detect enemy (transisi dari unlocked ke ada targets)
        if (!wasLocked && !isCurrentlyLocked && sortedTargets.Count > 0)
        {
            if (logTargetChanges)
            {
                Debug.Log($"<color=lime>[Targeting] AUTO-LOCK: Detected {sortedTargets.Count} enemies!</color>");
            }
            SetDefaultTarget();
        }
        
        wasLocked = isCurrentlyLocked;
    }
    
    /// <summary>
    /// Auto-switch to nearest available target
    /// </summary>
    void AutoSwitchToNearestTarget()
    {
        if (sortedTargets.Count == 0)
        {
            ClearLock();
            return;
        }
        
        // Cari target terdekat
        int nearestIndex = 0;
        float nearestDistance = float.MaxValue;
        
        for (int i = 0; i < sortedTargets.Count; i++)
        {
            if (sortedTargets[i].transform != null && sortedTargets[i].distance < nearestDistance)
            {
                nearestDistance = sortedTargets[i].distance;
                nearestIndex = i;
            }
        }
        
        currentTargetIndex = nearestIndex;
        SelectCurrentTarget();
        
        if (logTargetChanges)
        {
            Debug.Log($"<color=lime>[Targeting] AUTO-SWITCHED to: {GetCurrentTarget()?.name} (nearest target)</color>");
        }
    }
    
    /// <summary>
    /// Get number of available targets
    /// </summary>
    public int GetTargetCount()
    {
        return sortedTargets.Count;
    }
    
    /// <summary>
    /// Check if currently locked onto a target
    /// </summary>
    public bool HasTarget()
    {
        return currentTargetIndex >= 0 && currentTargetIndex < sortedTargets.Count;
    }
    
    /// <summary>
    /// Check if camera is currently locked (has active target)
    /// </summary>
    public bool IsLocked()
    {
        return HasTarget();
    }
    
    /// <summary>
    /// Get all detected targets (for UI or other systems)
    /// </summary>
    public List<Transform> GetAllTargets()
    {
        List<Transform> targets = new List<Transform>();
        foreach (var info in sortedTargets)
        {
            if (info.transform != null)
                targets.Add(info.transform);
        }
        return targets;
    }
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!showDebugGizmos || player == null) return;
        
        // Draw detection sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, detectionRadius);
        
        // Draw camera forward direction
        if (mainCamera != null)
        {
            Vector3 forward = mainCamera.transform.forward;
            forward.y = 0f;
            forward.Normalize();
            
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(player.position, forward * detectionRadius);
        }
        
        // Draw all targets
        foreach (var info in sortedTargets)
        {
            if (info.transform == null) continue;
            
            // Different color for current target
            bool isCurrent = sortedTargets.IndexOf(info) == currentTargetIndex;
            Gizmos.color = isCurrent ? Color.red : Color.green;
            
            // Draw sphere at target
            Gizmos.DrawWireSphere(info.transform.position, 0.5f);
            
            // Draw line from player to target
            Gizmos.color = isCurrent ? Color.red : Color.gray;
            Gizmos.DrawLine(player.position + Vector3.up, info.transform.position + Vector3.up);
            
            // Draw angle indicator
            if (Application.isPlaying)
            {
                UnityEditor.Handles.Label(
                    info.transform.position + Vector3.up * 2f,
                    $"{info.transform.name}\nAngle: {info.angle:F1}°\nDist: {info.distance:F1}m"
                );
            }
        }
        
        // Draw current target highlight
        Transform current = GetCurrentTarget();
        if (current != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(current.position, 1f);
        }
    }
#endif
    
    void OnDestroy()
    {
        // Cleanup indicator on destroy
        ClearIndicator();
    }
}

/// <summary>
/// Target information for sorting
/// </summary>
[System.Serializable]
public class TargetInfo
{
    public Transform transform;
    public float angle;      // Signed angle from camera forward (-180 to 180)
    public float distance;   // Distance from player
}
