using UnityEngine;

/// <summary>
/// Manager untuk menghubungkan AttackTimingBar dengan weapon system
/// </summary>
public class AttackTimingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AttackTimingBar timingBar;
    [SerializeField] private WeaponPlayerController weaponController;
    
    [Header("Timing Settings")]
    [SerializeField] private bool enableTimingSystem = true;
    [SerializeField] private bool showBarOnlyNearEnemy = true;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("Difficulty Settings")]
    [SerializeField] private float easySpeed = 0.5f;
    [SerializeField] private float normalSpeed = 1f;
    [SerializeField] private float hardSpeed = 1.5f;
    
    private bool isNearEnemy = false;
    
    void Start()
    {
        // Auto-find references if not assigned
        if (timingBar == null)
        {
            timingBar = FindAnyObjectByType<AttackTimingBar>();
        }
        
        if (weaponController == null)
        {
            weaponController = GetComponent<WeaponPlayerController>();
        }
        
        // Initialize
        if (timingBar != null && !enableTimingSystem)
        {
            timingBar.SetVisible(false);
        }
    }
    
    void Update()
    {
        if (!enableTimingSystem || timingBar == null) return;
        
        // Check if near enemy
        CheckNearbyEnemies();
        
        // Update bar visibility
        UpdateBarVisibility();
    }
    
    /// <summary>
    /// Check for nearby enemies
    /// </summary>
    void CheckNearbyEnemies()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRange, enemyLayer);
        isNearEnemy = enemies.Length > 0;
    }
    
    /// <summary>
    /// Update timing bar visibility
    /// </summary>
    void UpdateBarVisibility()
    {
        if (showBarOnlyNearEnemy)
        {
            timingBar.SetVisible(isNearEnemy);
            
            if (isNearEnemy)
            {
                timingBar.Activate();
            }
            else
            {
                timingBar.Deactivate();
            }
        }
        else
        {
            timingBar.SetVisible(true);
            timingBar.Activate();
        }
    }
    
    /// <summary>
    /// Call this when player attacks - returns damage multiplier
    /// </summary>
    public float OnPlayerAttack()
    {
        if (!enableTimingSystem || timingBar == null)
        {
            return 1f; // Normal damage
        }
        
        TimingResult result = timingBar.CheckTiming();
        
        // Apply damage multiplier to weapon
        ApplyTimingBonus(result);
        
        return result.damageMultiplier;
    }
    
    /// <summary>
    /// Apply timing bonus to current weapon
    /// </summary>
    void ApplyTimingBonus(TimingResult result)
    {
        // Log result for debugging
        Debug.Log($"Attack Timing: {result.quality} (x{result.damageMultiplier} damage, {result.accuracy * 100f:F1}% accuracy)");
        
        // You can add visual/audio feedback here
        switch (result.quality)
        {
            case TimingQuality.Perfect:
                // Play perfect hit effect
                break;
            case TimingQuality.Great:
                // Play great hit effect
                break;
            case TimingQuality.Good:
                // Play good hit effect
                break;
            case TimingQuality.Miss:
                // Play miss effect
                break;
        }
    }
    
    /// <summary>
    /// Set difficulty level
    /// </summary>
    public void SetDifficulty(string difficulty)
    {
        if (timingBar == null) return;
        
        switch (difficulty.ToLower())
        {
            case "easy":
                timingBar.SetSpeed(easySpeed);
                break;
            case "normal":
                timingBar.SetSpeed(normalSpeed);
                break;
            case "hard":
                timingBar.SetSpeed(hardSpeed);
                break;
        }
    }
    
    /// <summary>
    /// Enable or disable timing system
    /// </summary>
    public void SetTimingSystemEnabled(bool enabled)
    {
        enableTimingSystem = enabled;
        
        if (timingBar != null)
        {
            timingBar.SetVisible(enabled);
        }
    }
    
    /// <summary>
    /// Get if player is near enemy
    /// </summary>
    public bool IsNearEnemy()
    {
        return isNearEnemy;
    }
    
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = isNearEnemy ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
#endif
}
