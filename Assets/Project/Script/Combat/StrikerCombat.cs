using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class StrikerCombat : MonoBehaviour, IRoleCombat
{
    [Header("Basic Attack Settings")]
    [SerializeField] private float basicAttackDamage = 50f;
    [SerializeField] private float basicAttackRange = 3f;
    [SerializeField] private float basicAttackCooldown = 1.2f;
    [SerializeField] private LayerMask enemyLayer;
    private float lastBasicAttackTime;
    
    [Header("Skill 1 - Shockwave Settings")]
    [SerializeField] private float shockwaveDamage = 150f;
    [SerializeField] private float shockwaveRadius = 8f;
    [SerializeField] private float shockwaveStunDuration = 2f;
    [SerializeField] private float shockwaveCooldown = 8f;
    [SerializeField] private float shockwaveWindupTime = 0.5f; // Time before shockwave triggers
    private float lastShockwaveTime;
    private bool isPerformingShockwave;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject shockwaveVFXPrefab;
    [SerializeField] private Color shockwaveColor = new Color(1f, 0.3f, 0.3f, 0.5f);
    [SerializeField] private ParticleSystem punchEffect;
    
    [Header("Audio")]
    [SerializeField] private AudioClip punchSound;
    [SerializeField] private AudioClip shockwaveRoarSound;
    [SerializeField] private AudioClip shockwaveImpactSound;
    private AudioSource audioSource;
    
    [Header("References")]
    private PlayerRole playerRole;
    private Animator animator;
    private PlayerController playerController;
    private PlayerInput playerInput;
    
    // Input tracking
    private bool skill1Pressed;
    private GameObject owner;
    
    // IRoleCombat implementation
    public RoleType GetRoleType() => RoleType.Striker;
    
    public void Initialize(GameObject ownerObject)
    {
        owner = ownerObject;
        enabled = false; // Start disabled, RoleCombatManager will enable when needed
    }
    
    public void Cleanup()
    {
        // Cleanup code if needed
    }
    
    void Start()
    {
        playerRole = GetComponent<PlayerRole>();
        animator = GetComponentInChildren<Animator>();
        playerController = GetComponent<PlayerController>();
        playerInput = GetComponent<PlayerInput>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Set initial cooldowns
        lastBasicAttackTime = -basicAttackCooldown;
        lastShockwaveTime = -shockwaveCooldown;
        
        // NOTE: Input subscription is now handled by RoleCombatManager
    }
    
    void OnDestroy()
    {
        // NOTE: Input unsubscription is now handled by RoleCombatManager
    }

    // IRoleCombat interface methods - Renamed to prevent Unity SendMessage conflicts
    public void HandleBasicAttack(InputAction.CallbackContext context)
    {
        Debug.Log("⚔️ STRIKER HandleBasicAttack called!");
        TryBasicAttack();
    }

    public void HandleSkill1(InputAction.CallbackContext context)
    {
        TryShockwave();
    }
    
    public void HandleSkill2(InputAction.CallbackContext context)
    {
        // Striker doesn't have Skill2 yet
        Debug.Log("⚠️ Striker Skill2 not implemented");
    }

    void Update()
    {
        // No input polling needed, using Input System events
    }
    
    void TryBasicAttack()
    {
        if (isPerformingShockwave) return;
        
        if (Time.time - lastBasicAttackTime < basicAttackCooldown)
        {
            Debug.Log($"⏳ Basic attack on cooldown! {(basicAttackCooldown - (Time.time - lastBasicAttackTime)):F1}s remaining");
            return;
        }
        
        PerformBasicAttack();
    }
    
    void PerformBasicAttack()
    {
        lastBasicAttackTime = Time.time;
        
        Debug.Log("👊 STRIKER: Basic punch attack!");
        
        // Play animation if available
        if (animator != null)
        {
            animator.SetTrigger("Punch");
        }
        
        // Play sound
        if (punchSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(punchSound);
        }
        
        // Spawn punch effect
        if (punchEffect != null)
        {
            punchEffect.Play();
        }
        
        // Deal damage to enemies in front
        Vector3 attackPosition = transform.position + transform.forward * (basicAttackRange * 0.5f);
        Collider[] hitEnemies = Physics.OverlapSphere(attackPosition, basicAttackRange * 0.5f, enemyLayer);
        
        Debug.Log($"🔍 Checking for enemies at position {attackPosition} with radius {basicAttackRange * 0.5f}");
        Debug.Log($"🎯 Found {hitEnemies.Length} colliders in attack range");
        
        float totalDamage = GetTotalDamage(basicAttackDamage);
        Debug.Log($"⚔️ Total damage calculated: {totalDamage} (Base: {basicAttackDamage} + Weapon: {(playerRole != null ? playerRole.GetTotalWeaponDamage() : 0)})");
        
        foreach (Collider enemy in hitEnemies)
        {
            Debug.Log($"🔎 Hit collider: {enemy.name} on layer {LayerMask.LayerToName(enemy.gameObject.layer)}");
            
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                Debug.Log($"✅ EnemyHealth found on {enemy.name}, dealing {totalDamage} damage!");
                enemyHealth.TakeDamage(totalDamage);
                Debug.Log($"💥 Successfully damaged {enemy.name} for {totalDamage} damage!");
                
                // Knockback effect
                Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    Vector3 knockbackDir = (enemy.transform.position - transform.position).normalized;
                    enemyRb.AddForce(knockbackDir * 5f, ForceMode.Impulse);
                    Debug.Log($"💨 Applied knockback to {enemy.name}");
                }
            }
            else
            {
                Debug.LogWarning($"❌ No EnemyHealth component found on {enemy.name}!");
            }
        }
        
        if (hitEnemies.Length == 0)
        {
            Debug.LogWarning("⚠️ No enemies hit by basic attack!");
        }
        
        // Visual feedback sphere (debug)
        StartCoroutine(ShowAttackRange(attackPosition, basicAttackRange * 0.5f, Color.yellow, 0.2f));
    }
    
    void TryShockwave()
    {
        if (isPerformingShockwave)
        {
            Debug.Log("⏳ Already performing shockwave!");
            return;
        }
        
        if (Time.time - lastShockwaveTime < shockwaveCooldown)
        {
            Debug.Log($"⏳ Shockwave on cooldown! {(shockwaveCooldown - (Time.time - lastShockwaveTime)):F1}s remaining");
            return;
        }
        
        StartCoroutine(PerformShockwave());
    }
    
    IEnumerator PerformShockwave()
    {
        isPerformingShockwave = true;
        lastShockwaveTime = Time.time;
        
        Debug.Log("📢 STRIKER: SHOCKWAVE ROAR!");
        
        // Lock player movement during windup
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Play roar animation
        if (animator != null)
        {
            animator.SetTrigger("Roar");
        }
        
        // Play roar sound
        if (shockwaveRoarSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shockwaveRoarSound);
        }
        
        // Windup visual indicator
        StartCoroutine(ShowChargingIndicator());
        
        // Wait for windup
        yield return new WaitForSeconds(shockwaveWindupTime);
        
        // Execute shockwave
        ExecuteShockwave();
        
        // Re-enable movement
        yield return new WaitForSeconds(0.3f);
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        isPerformingShockwave = false;
    }
    
    void ExecuteShockwave()
    {
        Debug.Log($"💥 SHOCKWAVE IMPACT! Radius: {shockwaveRadius}m");
        
        // Play impact sound
        if (shockwaveImpactSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shockwaveImpactSound, 0.8f);
        }
        
        // Spawn VFX
        if (shockwaveVFXPrefab != null)
        {
            GameObject vfx = Instantiate(shockwaveVFXPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 3f);
        }
        
        // Visual shockwave ring
        StartCoroutine(ShowShockwaveRing());
        
        // Find all enemies in radius
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, shockwaveRadius, enemyLayer);
        
        float totalDamage = GetTotalDamage(shockwaveDamage);
        
        Debug.Log($"🎯 Shockwave hit {hitEnemies.Length} enemies!");
        
        foreach (Collider enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // Deal damage
                enemyHealth.TakeDamage(totalDamage);
                Debug.Log($"💥 Shockwave hit {enemy.name} for {totalDamage} damage!");
                
                // Apply stun
                EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    StartCoroutine(StunEnemy(enemyAI, shockwaveStunDuration));
                }
                
                // Knockback
                Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    Vector3 knockbackDir = (enemy.transform.position - transform.position).normalized;
                    knockbackDir.y = 0.5f; // Add upward force
                    enemyRb.AddForce(knockbackDir * 15f, ForceMode.Impulse);
                }
            }
        }
    }
    
    IEnumerator StunEnemy(EnemyAI enemy, float duration)
    {
        if (enemy == null) yield break;
        
        // Disable enemy AI
        enemy.enabled = false;
        
        // Visual stun effect
        GameObject stunVFX = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        stunVFX.transform.position = enemy.transform.position + Vector3.up * 2f;
        stunVFX.transform.localScale = Vector3.one * 0.5f;
        stunVFX.GetComponent<Renderer>().material.color = Color.yellow;
        Destroy(stunVFX.GetComponent<Collider>());
        
        Debug.Log($"😵 {enemy.name} stunned for {duration}s!");
        
        yield return new WaitForSeconds(duration);
        
        // Re-enable enemy
        if (enemy != null)
        {
            enemy.enabled = true;
            Debug.Log($"✅ {enemy.name} recovered from stun!");
        }
        
        if (stunVFX != null)
        {
            Destroy(stunVFX);
        }
    }
    
    IEnumerator ShowChargingIndicator()
    {
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicator.transform.position = transform.position;
        indicator.transform.localScale = Vector3.one * 0.5f;
        Destroy(indicator.GetComponent<Collider>());
        
        Renderer rend = indicator.GetComponent<Renderer>();
        rend.material.color = new Color(1f, 0.5f, 0f, 0.6f);
        
        float elapsed = 0f;
        while (elapsed < shockwaveWindupTime)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(0.5f, shockwaveRadius * 0.3f, elapsed / shockwaveWindupTime);
            indicator.transform.localScale = Vector3.one * scale;
            indicator.transform.position = transform.position + Vector3.up * 0.1f;
            yield return null;
        }
        
        Destroy(indicator);
    }
    
    IEnumerator ShowShockwaveRing()
    {
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.transform.position = transform.position + Vector3.up * 0.1f;
        ring.transform.localScale = new Vector3(0.1f, 0.05f, 0.1f);
        Destroy(ring.GetComponent<Collider>());
        
        Renderer rend = ring.GetComponent<Renderer>();
        rend.material.color = shockwaveColor;
        
        float elapsed = 0f;
        float duration = 0.5f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(0.1f, shockwaveRadius * 2f, elapsed / duration);
            float alpha = Mathf.Lerp(0.8f, 0f, elapsed / duration);
            
            ring.transform.localScale = new Vector3(scale, 0.05f, scale);
            Color color = shockwaveColor;
            color.a = alpha;
            rend.material.color = color;
            
            yield return null;
        }
        
        Destroy(ring);
    }
    
    IEnumerator ShowAttackRange(Vector3 position, float radius, Color color, float duration)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        sphere.transform.localScale = Vector3.one * radius * 2f;
        Destroy(sphere.GetComponent<Collider>());
        
        Renderer rend = sphere.GetComponent<Renderer>();
        color.a = 0.3f;
        rend.material.color = color;
        
        yield return new WaitForSeconds(duration);
        Destroy(sphere);
    }
    
    float GetTotalDamage(float baseDamage)
    {
        float totalDamage = baseDamage;
        
        if (playerRole != null)
        {
            totalDamage += playerRole.GetTotalWeaponDamage();
        }
        
        return totalDamage;
    }
    
    // Public methods for UI/debugging
    public float GetBasicAttackCooldownRemaining()
    {
        return Mathf.Max(0, basicAttackCooldown - (Time.time - lastBasicAttackTime));
    }
    
    public float GetShockwaveCooldownRemaining()
    {
        return Mathf.Max(0, shockwaveCooldown - (Time.time - lastShockwaveTime));
    }
    
    void OnDrawGizmosSelected()
    {
        // Show basic attack range
        Gizmos.color = Color.yellow;
        Vector3 attackPos = transform.position + transform.forward * (basicAttackRange * 0.5f);
        Gizmos.DrawWireSphere(attackPos, basicAttackRange * 0.5f);
        
        // Show shockwave radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shockwaveRadius);
    }
}
