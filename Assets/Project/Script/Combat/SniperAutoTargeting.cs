using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Auto-targeting system for Sniper mode
/// Detects enemies in range and locks onto them with visual indicator
/// </summary>
public class SniperAutoTargeting : MonoBehaviour
{
    [Header("Targeting Settings")]
    [SerializeField] private float detectionRange = 50f;
    [SerializeField] private float targetingAngle = 45f; // Cone angle untuk deteksi
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float targetSwitchDelay = 0.3f;
    
    [Header("Lock-on Settings")]
    [SerializeField] private float lockOnTime = 2f; // Waktu sampai perfect shot
    [SerializeField] private float lockDecaySpeed = 1.5f; // Kecepatan decay saat tidak aim
    
    [Header("Perfect Shot")]
    [SerializeField] private float perfectShotWindow = 0.2f; // Window waktu untuk perfect shot
    [SerializeField] private float perfectShotDamageMultiplier = 3f;
    
    [Header("References")]
    [SerializeField] private SniperTargetIndicator indicatorPrefab;
    [SerializeField] private Transform indicatorParent; // Canvas untuk UI
    
    [Header("Visual Feedback")]
    [SerializeField] private Color normalColor = Color.red;
    [SerializeField] private Color lockingColor = Color.yellow;
    [SerializeField] private Color perfectColor = Color.green;
    
    // State
    private Transform currentTarget;
    private List<Transform> enemiesInRange = new List<Transform>();
    private SniperTargetIndicator currentIndicator;
    private float lockProgress = 0f; // 0 to 1
    private float lastTargetSwitchTime;
    private bool isActive = false;
    private Camera mainCamera;
    
    // Events
    public delegate void OnTargetLocked(Transform target, bool isPerfectShot);
    public event OnTargetLocked TargetLocked;
    
    public delegate void OnTargetLost();
    public event OnTargetLost TargetLost;
    
    // Properties
    public Transform CurrentTarget => currentTarget;
    public bool HasTarget => currentTarget != null;
    public bool IsPerfectShot => lockProgress >= 1f;
    public float LockProgress => lockProgress;
    public int EnemyCount => enemiesInRange.Count;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        // Find canvas if not assigned
        if (indicatorParent == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                indicatorParent = canvas.transform;
            }
        }
    }
    
    void Update()
    {
        if (!isActive) return;
        
        // Scan for enemies
        ScanForEnemies();
        
        // Update current target
        UpdateTargeting();
        
        // Update lock progress
        UpdateLockProgress();
        
        // Update indicator
        UpdateIndicator();
    }
    
    void ScanForEnemies()
    {
        enemiesInRange.Clear();
        
        // Get all colliders in range
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange, enemyLayer);
        
        foreach (Collider col in colliders)
        {
            // Check if in view cone
            Vector3 directionToEnemy = (col.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToEnemy);
            
            if (angle <= targetingAngle)
            {
                // Check line of sight
                if (HasLineOfSight(col.transform))
                {
                    enemiesInRange.Add(col.transform);
                }
            }
        }
        
        // Sort by distance (closest first)
        enemiesInRange = enemiesInRange.OrderBy(t => Vector3.Distance(transform.position, t.position)).ToList();
    }
    
    bool HasLineOfSight(Transform target)
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 direction = (target.position - origin).normalized;
        float distance = Vector3.Distance(transform.position, target.position);
        
        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            // Check if hit the target or hit something else
            return hit.transform == target || hit.transform.IsChildOf(target);
        }
        
        return false;
    }
    
    void UpdateTargeting()
    {
        // If no enemies in range, lose target
        if (enemiesInRange.Count == 0)
        {
            if (currentTarget != null)
            {
                LoseTarget();
            }
            return;
        }
        
        // Auto-switch to closest enemy if no target
        if (currentTarget == null)
        {
            AcquireTarget(enemiesInRange[0]);
        }
        // Check if current target is still valid
        else if (!enemiesInRange.Contains(currentTarget))
        {
            LoseTarget();
            
            // Switch to next available target
            if (enemiesInRange.Count > 0)
            {
                AcquireTarget(enemiesInRange[0]);
            }
        }
        // Check if should switch to closer target
        else if (Time.time - lastTargetSwitchTime > targetSwitchDelay)
        {
            Transform closestEnemy = enemiesInRange[0];
            if (closestEnemy != currentTarget)
            {
                float currentDistance = Vector3.Distance(transform.position, currentTarget.position);
                float closestDistance = Vector3.Distance(transform.position, closestEnemy.position);
                
                // Only switch if significantly closer (20% closer)
                if (closestDistance < currentDistance * 0.8f)
                {
                    LoseTarget();
                    AcquireTarget(closestEnemy);
                }
            }
        }
    }
    
    void UpdateLockProgress()
    {
        if (currentTarget == null)
        {
            // Decay lock progress when no target
            lockProgress = Mathf.Max(0f, lockProgress - Time.deltaTime * lockDecaySpeed);
            return;
        }
        
        // Check if aiming at target
        Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
        
        // Increase lock if aiming at target
        if (angleToTarget <= targetingAngle * 0.5f) // Tighter angle for locking
        {
            lockProgress = Mathf.Min(1f, lockProgress + Time.deltaTime / lockOnTime);
        }
        else
        {
            // Decay if not aiming well
            lockProgress = Mathf.Max(0f, lockProgress - Time.deltaTime * lockDecaySpeed);
        }
    }
    
    void UpdateIndicator()
    {
        if (currentIndicator == null) return;
        
        if (currentTarget == null)
        {
            currentIndicator.Hide();
            return;
        }
        
        // Update indicator position
        Vector3 screenPos = mainCamera.WorldToScreenPoint(currentTarget.position);
        
        // Check if on screen
        if (screenPos.z > 0 && screenPos.x >= 0 && screenPos.x <= Screen.width && 
            screenPos.y >= 0 && screenPos.y <= Screen.height)
        {
            currentIndicator.Show();
            currentIndicator.UpdatePosition(screenPos);
            currentIndicator.UpdateLockProgress(lockProgress);
            
            // Update color based on lock progress
            Color indicatorColor;
            if (IsPerfectShot)
            {
                indicatorColor = perfectColor;
            }
            else if (lockProgress > 0.5f)
            {
                indicatorColor = Color.Lerp(lockingColor, perfectColor, (lockProgress - 0.5f) * 2f);
            }
            else
            {
                indicatorColor = Color.Lerp(normalColor, lockingColor, lockProgress * 2f);
            }
            
            currentIndicator.UpdateColor(indicatorColor);
        }
        else
        {
            currentIndicator.Hide();
        }
    }
    
    void AcquireTarget(Transform target)
    {
        currentTarget = target;
        lastTargetSwitchTime = Time.time;
        lockProgress = 0f;
        
        // Create indicator if needed
        if (currentIndicator == null && indicatorPrefab != null && indicatorParent != null)
        {
            currentIndicator = Instantiate(indicatorPrefab, indicatorParent);
        }
        
        Debug.Log($"🎯 Target acquired: {target.name}");
        
        // Invoke event
        TargetLocked?.Invoke(target, false);
    }
    
    void LoseTarget()
    {
        Debug.Log($"🎯 Target lost: {currentTarget?.name}");
        
        currentTarget = null;
        lockProgress = 0f;
        
        if (currentIndicator != null)
        {
            currentIndicator.Hide();
        }
        
        // Invoke event
        TargetLost?.Invoke();
    }
    
    public void Activate()
    {
        isActive = true;
        lockProgress = 0f;
        Debug.Log("🎯 Sniper auto-targeting ACTIVATED");
    }
    
    public void Deactivate()
    {
        isActive = false;
        LoseTarget();
        
        if (currentIndicator != null)
        {
            currentIndicator.Hide();
        }
        
        Debug.Log("🎯 Sniper auto-targeting DEACTIVATED");
    }
    
    /// <summary>
    /// Shoot at current target with damage multiplier based on lock progress
    /// </summary>
    public bool TryShoot(out Transform target, out float damageMultiplier)
    {
        target = currentTarget;
        damageMultiplier = 1f;
        
        if (currentTarget == null)
        {
            Debug.Log("❌ No target to shoot!");
            return false;
        }
        
        // Calculate damage multiplier
        if (IsPerfectShot)
        {
            damageMultiplier = perfectShotDamageMultiplier;
            Debug.Log($"💥 PERFECT SHOT! {damageMultiplier}x damage!");
        }
        else
        {
            // Partial damage based on lock progress (50% to 100%)
            damageMultiplier = Mathf.Lerp(0.5f, 1f, lockProgress);
            Debug.Log($"🎯 Shot fired! Lock: {lockProgress * 100f:F0}%, Damage: {damageMultiplier * 100f:F0}%");
        }
        
        // Reset lock after shot
        lockProgress = 0f;
        
        return true;
    }
    
    /// <summary>
    /// Manually switch to next target
    /// </summary>
    public void SwitchToNextTarget()
    {
        if (enemiesInRange.Count <= 1) return;
        
        int currentIndex = enemiesInRange.IndexOf(currentTarget);
        int nextIndex = (currentIndex + 1) % enemiesInRange.Count;
        
        LoseTarget();
        AcquireTarget(enemiesInRange[nextIndex]);
        
        Debug.Log($"🔄 Switched to next target: {currentTarget.name}");
    }
    
    void OnDestroy()
    {
        if (currentIndicator != null)
        {
            Destroy(currentIndicator.gameObject);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw targeting cone
        Gizmos.color = Color.yellow;
        Vector3 forward = transform.forward * detectionRange;
        Vector3 right = Quaternion.Euler(0, targetingAngle, 0) * forward;
        Vector3 left = Quaternion.Euler(0, -targetingAngle, 0) * forward;
        
        Gizmos.DrawRay(transform.position, right);
        Gizmos.DrawRay(transform.position, left);
        
        // Draw line to current target
        if (currentTarget != null)
        {
            Gizmos.color = IsPerfectShot ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
        
        // Draw all enemies in range
        if (Application.isPlaying && enemiesInRange != null)
        {
            Gizmos.color = Color.white;
            foreach (Transform enemy in enemiesInRange)
            {
                if (enemy != null && enemy != currentTarget)
                {
                    Gizmos.DrawWireSphere(enemy.position, 1f);
                }
            }
        }
    }
}
