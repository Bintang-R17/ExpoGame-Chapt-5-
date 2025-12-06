using UnityEngine;
using System.Collections;

/// <summary>
/// Enemy shield/vulnerability cycle system
/// Controls when enemy is vulnerable and shows timing window
/// </summary>
public class ShieldCycle : MonoBehaviour
{
    [Header("Cycle Settings")]
    [SerializeField] private float cycleDuration = 5f; // Total cycle time
    [SerializeField] private float exposeDuration = 1.5f; // Vulnerability window duration
    [SerializeField] private bool autoStart = true;
    [SerializeField] private bool loopCycle = true;
    
    [Header("Shield Visual (Optional)")]
    [SerializeField] private GameObject shieldVisual;
    [SerializeField] private Material shieldMaterial;
    [SerializeField] private Color shieldActiveColor = new Color(0f, 0.5f, 1f, 0.5f);
    [SerializeField] private Color shieldVulnerableColor = new Color(1f, 0.5f, 0f, 0.3f);
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip shieldBreakSound;
    [SerializeField] private AudioClip shieldRestoreSound;
    [SerializeField] private AudioSource audioSource;
    
    [Header("Debug")]
    [SerializeField] private bool logStateChanges = true;
    
    // State
    private bool isVulnerable = false;
    private bool isExposed = false;
    private float cycleTimer = 0f;
    private Coroutine cycleCoroutine;
    
    // References
    private TimingBarUI timingBarUI;
    private EnemyHealth enemyHealth;
    
    void Awake()
    {
        // Find references
        enemyHealth = GetComponent<EnemyHealth>();
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        // Setup shield visual
        if (shieldVisual != null && shieldMaterial != null)
        {
            Renderer renderer = shieldVisual.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = shieldMaterial;
            }
        }
    }
    
    void Start()
    {
        // Find timing bar UI using singleton
        timingBarUI = TimingBarUI.Instance;
        
        if (timingBarUI == null)
        {
            Debug.LogWarning($"[ShieldCycle] TimingBarUI not found in scene! {gameObject.name} will cycle but no UI will display.");
            Debug.LogWarning("[ShieldCycle] To fix: Create UI GameObject with TimingBarUI component, or timing window will be invisible.");
        }
        else
        {
            Debug.Log($"<color=cyan>[ShieldCycle] {gameObject.name} found TimingBarUI - Ready!</color>");
        }
        
        if (autoStart)
        {
            StartCycle();
        }
    }
    
    /// <summary>
    /// Start shield cycle
    /// </summary>
    public void StartCycle()
    {
        if (cycleCoroutine != null)
        {
            StopCoroutine(cycleCoroutine);
        }
        
        cycleCoroutine = StartCoroutine(CycleRoutine());
        
        if (logStateChanges)
        {
            Debug.Log($"<color=cyan>[ShieldCycle] {gameObject.name} - Cycle Started</color>");
        }
    }
    
    /// <summary>
    /// Stop shield cycle
    /// </summary>
    public void StopCycle()
    {
        if (cycleCoroutine != null)
        {
            StopCoroutine(cycleCoroutine);
            cycleCoroutine = null;
        }
        
        SetShieldActive(false);
        isVulnerable = false;
        isExposed = false;
    }
    
    /// <summary>
    /// Main cycle coroutine
    /// </summary>
    IEnumerator CycleRoutine()
    {
        while (true)
        {
            // Shield active phase
            SetShieldActive(true);
            isVulnerable = false;
            
            float shieldDuration = cycleDuration - exposeDuration;
            yield return new WaitForSeconds(shieldDuration);
            
            // Expose phase - show timing window
            Expose();
            
            yield return new WaitForSeconds(exposeDuration);
            
            // Note: Don't hide timing bar here - let EnemyTargetUI handle it
            // Timing bar stays visible while enemy is targeted
            
            if (!loopCycle)
            {
                break;
            }
        }
    }
    
    /// <summary>
    /// Trigger expose/vulnerable state
    /// </summary>
    public void Expose()
    {
        isVulnerable = true;
        isExposed = true;
        
        SetShieldActive(false);
        
        // Always show timing bar when exposed
        if (timingBarUI != null)
        {
            timingBarUI.ShowPersistent(transform, exposeDuration, 0f, 1f);
            
            if (logStateChanges)
            {
                Debug.Log($"<color=lime>[ShieldCycle] {gameObject.name} - Timing bar SHOWN</color>");
            }
        }
        else
        {
            Debug.LogWarning($"<color=red>[ShieldCycle] {gameObject.name} - TimingBarUI is NULL!</color>");
        }
        
        if (logStateChanges)
        {
            Debug.Log($"<color=yellow>[ShieldCycle] {gameObject.name} - EXPOSED!</color>");
            Debug.Log($"<color=yellow>  → Duration: {exposeDuration}s</color>");
            Debug.Log($"<color=yellow>  → Height System: ≥80% Perfect | 40-79% Good (2x) | <40% Miss</color>");
        }
        
        // Play sound
        if (audioSource != null && shieldBreakSound != null)
        {
            audioSource.PlayOneShot(shieldBreakSound);
        }
    }
    
    /// <summary>
    /// Set shield visual state
    /// </summary>
    void SetShieldActive(bool active)
    {
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(active);
        }
        
        // Lock/unlock HP via EnemyHealth
        if (enemyHealth != null)
        {
            enemyHealth.SetShieldBlocking(active);
        }
        
        // Update shield color
        if (shieldMaterial != null)
        {
            shieldMaterial.color = active ? shieldActiveColor : shieldVulnerableColor;
        }
        
        // Play restore sound
        if (active && audioSource != null && shieldRestoreSound != null)
        {
            audioSource.PlayOneShot(shieldRestoreSound);
        }
        
        if (logStateChanges)
        {
            Debug.Log($"<color=cyan>[ShieldCycle] {gameObject.name} - Shield: {(active ? "ACTIVE" : "DOWN")}</color>");
        }
    }
    
    /// <summary>
    /// Check if enemy is currently vulnerable
    /// </summary>
    public bool IsVulnerable()
    {
        return isVulnerable && isExposed;
    }
    
    /// <summary>
    /// Check if currently in expose window
    /// </summary>
    public bool IsExposed()
    {
        return isExposed;
    }
    
    /// <summary>
    /// Show timing bar immediately (called when enemy becomes target)
    /// </summary>
    public void ShowTimingBar()
    {
        if (timingBarUI != null && isExposed)
        {
            timingBarUI.ShowPersistent(transform, exposeDuration, 0f, 1f);
            Debug.Log($"<color=cyan>[ShieldCycle] {gameObject.name} - Timing bar shown on target switch</color>");
        }
        else if (timingBarUI == null)
        {
            Debug.LogWarning($"<color=red>[ShieldCycle] {gameObject.name} - Cannot show timing bar: TimingBarUI is NULL!</color>");
        }
        else if (!isExposed)
        {
            Debug.Log($"<color=orange>[ShieldCycle] {gameObject.name} - Cannot show timing bar: Not exposed yet</color>");
        }
    }
    
    /// <summary>
    /// Force shield break (for testing or special attacks)
    /// </summary>
    public void ForceExpose(float duration = 0f)
    {
        if (duration > 0f)
        {
            exposeDuration = duration;
        }
        
        Expose();
    }
    
    /// <summary>
    /// Handle perfect hit (Green zone: HP / 1 = instant kill)
    /// </summary>
    public void OnPerfectHit(float damage)
    {
        if (logStateChanges)
        {
            Debug.Log($"<color=lime>[ShieldCycle] {gameObject.name} - PERFECT HIT! (MaxHP / 1)</color>");
        }
        
        // Disable shield and apply full HP damage (MaxHP / 1)
        if (enemyHealth != null)
        {
            enemyHealth.SetShieldBlocking(false);
            float fullDamage = enemyHealth.GetMaxHealth() / 1f; // MaxHP / 1
            enemyHealth.TakeDamage(fullDamage);
        }
        
        // Stop cycle
        StopCycle();
        
        // Hide timing bar
        if (timingBarUI != null && timingBarUI.GetCurrentTarget() == transform)
        {
            timingBarUI.Hide();
        }
    }
    
    /// <summary>
    /// Handle blocked hit (Red zone: HP / 0 = no damage, trigger counter)
    /// </summary>
    public void OnBlockedHit()
    {
        if (logStateChanges)
        {
            Debug.Log($"<color=red>[ShieldCycle] {gameObject.name} - BLOCKED! (HP / 0 = No damage)</color>");
        }
        
        // Shield absorbs damage - no health loss (HP / 0)
        // Optionally trigger counter attack
        TriggerCounter();
    }
    
    /// <summary>
    /// Handle good hit (Yellow zone: HP / 4)
    /// </summary>
    public void OnGoodHit(float damage)
    {
        if (logStateChanges)
        {
            Debug.Log($"<color=yellow>[ShieldCycle] {gameObject.name} - GOOD HIT! (MaxHP / 4)</color>");
        }
        
        // Temporarily disable shield and apply quarter damage (MaxHP / 4)
        if (enemyHealth != null)
        {
            enemyHealth.SetShieldBlocking(false);
            float quarterDamage = enemyHealth.GetMaxHealth() / 4f; // MaxHP / 4
            enemyHealth.TakeDamage(quarterDamage);
            // Shield will reactivate on next cycle
        }
    }
    
    /// <summary>
    /// Trigger enemy counter attack
    /// </summary>
    void TriggerCounter()
    {
        // TODO: Implement counter attack logic
        // - Play counter animation
        // - Deal damage to player
        // - Special effects
        
        Debug.Log($"<color=orange>[ShieldCycle] {gameObject.name} triggered COUNTER ATTACK!</color>");
    }
    
    void OnDestroy()
    {
        // Hide timing bar if this enemy is current target
        if (timingBarUI != null && timingBarUI.GetCurrentTarget() == transform)
        {
            timingBarUI.Hide();
        }
    }
}
