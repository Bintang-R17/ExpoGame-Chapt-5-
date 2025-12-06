using UnityEngine;

/// <summary>
/// Enemy UI controller that shows health bar when targeted by player
/// Integrates with TargetingManager and EnemyHealth
/// </summary>
[RequireComponent(typeof(EnemyHealth))]
public class EnemyTargetUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private EnemyHealthBarUI healthBarUI;
    
    [Header("UI Settings")]
    [SerializeField] private bool showHealthBarWhenTargeted = true;
    [SerializeField] private bool hideHealthBarWhenUntargeted = true;
    [SerializeField] private bool alwaysShowHealthBar = false; // Override untuk selalu tampil
    
    [Header("Debug")]
    [SerializeField] private bool logStateChanges = true;
    
    // State
    private bool isTargeted = false;
    private TargetingManager targetingManager;
    
    void Awake()
    {
        if (enemyHealth == null)
        {
            enemyHealth = GetComponent<EnemyHealth>();
        }
    }
    
    void Start()
    {
        // Find references
        targetingManager = FindObjectOfType<TargetingManager>();
        
        if (targetingManager == null)
        {
            Debug.LogWarning($"[EnemyTargetUI] {gameObject.name} - TargetingManager not found!");
        }
        
        // Get health bar UI from EnemyHealth
        if (healthBarUI == null && enemyHealth != null)
        {
            healthBarUI = enemyHealth.GetComponentInChildren<EnemyHealthBarUI>();
        }
        
        if (healthBarUI == null)
        {
            Debug.LogWarning($"[EnemyTargetUI] {gameObject.name} - EnemyHealthBarUI not found! Health bar won't show.");
        }
        
        // Initial state: hide health bar unless alwaysShow
        if (!alwaysShowHealthBar && healthBarUI != null)
        {
            healthBarUI.Hide();
        }
    }
    
    void Update()
    {
        CheckTargetStatus();
    }
    
    /// <summary>
    /// Check if this enemy is currently targeted
    /// </summary>
    void CheckTargetStatus()
    {
        if (targetingManager == null) return;
        
        bool wasTargeted = isTargeted;
        isTargeted = targetingManager.GetCurrentTarget() == transform;
        
        // State changed
        if (wasTargeted != isTargeted)
        {
            if (isTargeted)
            {
                OnTargeted();
            }
            else
            {
                OnUntargeted();
            }
        }
    }
    
    /// <summary>
    /// Called when enemy becomes targeted
    /// </summary>
    void OnTargeted()
    {
        if (logStateChanges)
        {
            Debug.Log($"<color=cyan>[EnemyTargetUI] {gameObject.name} - TARGETED!</color>");
        }
        
        // Show health bar
        if (showHealthBarWhenTargeted && healthBarUI != null)
        {
            healthBarUI.Show();
            
            if (logStateChanges)
            {
                Debug.Log($"<color=lime>[EnemyTargetUI] {gameObject.name} - Health bar shown</color>");
            }
        }
        
        // Show timing bar if enemy has shield cycle
        ShieldCycle shieldCycle = GetComponent<ShieldCycle>();
        if (shieldCycle != null)
        {
            // If not exposed yet, force expose on target
            if (!shieldCycle.IsExposed())
            {
                if (logStateChanges)
                {
                    Debug.Log($"<color=yellow>[EnemyTargetUI] {gameObject.name} - Not exposed, forcing expose...</color>");
                }
                shieldCycle.Expose(); // Force expose when targeted
            }
            else
            {
                // Already exposed, just show timing bar
                shieldCycle.ShowTimingBar();
            }
            
            if (logStateChanges)
            {
                Debug.Log($"<color=cyan>[EnemyTargetUI] {gameObject.name} - Timing bar triggered</color>");
            }
        }
    }
    
    /// <summary>
    /// Called when enemy is no longer targeted
    /// </summary>
    void OnUntargeted()
    {
        if (logStateChanges)
        {
            Debug.Log($"<color=yellow>[EnemyTargetUI] {gameObject.name} - Untargeted</color>");
        }
        
        // Hide health bar (unless always show is enabled)
        if (hideHealthBarWhenUntargeted && !alwaysShowHealthBar && healthBarUI != null)
        {
            healthBarUI.Hide();
            
            if (logStateChanges)
            {
                Debug.Log($"<color=yellow>[EnemyTargetUI] {gameObject.name} - Health bar hidden</color>");
            }
        }
    }
    
    /// <summary>
    /// Check if currently targeted
    /// </summary>
    public bool IsTargeted()
    {
        return isTargeted;
    }
    
    /// <summary>
    /// Manually show health bar
    /// </summary>
    public void ForceShowHealthBar()
    {
        if (healthBarUI != null)
        {
            healthBarUI.Show();
        }
    }
    
    /// <summary>
    /// Manually hide health bar
    /// </summary>
    public void ForceHideHealthBar()
    {
        if (healthBarUI != null && !alwaysShowHealthBar)
        {
            healthBarUI.Hide();
        }
    }
    
    /// <summary>
    /// Set whether to always show health bar
    /// </summary>
    public void SetAlwaysShow(bool value)
    {
        alwaysShowHealthBar = value;
        
        if (value && healthBarUI != null)
        {
            healthBarUI.Show();
        }
    }
}
