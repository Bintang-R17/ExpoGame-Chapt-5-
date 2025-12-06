using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("Lock Requirement")]
    [SerializeField] private bool requireLockToTakeDamage = true; // Harus di-lock untuk bisa kena damage
    [SerializeField] private TargetingManager targetingManager;
    private bool isShieldBlocking = false; // Shield sedang block (saat timing miss)

    [Header("UI")]
    [SerializeField] private EnemyHealthBarUI healthBarUI;
    [SerializeField] private bool autoCreateHealthBar = true;
    [SerializeField] private bool showHealthBar = true;
    
    [Header("Hit Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private bool enableHitFlash = true;
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.1f;
    [SerializeField] private bool enableKnockback = false;
    [SerializeField] private float knockbackForce = 5f;

    [Header("Death Settings")]
    [SerializeField] private float destroyDelay = 2f;
    [SerializeField] private GameObject deathEffect;

    private bool isDead = false;
    private Animator animator;
    private Renderer[] renderers;
    private Color[] originalColors;
    private Rigidbody rb;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        renderers = GetComponentsInChildren<Renderer>();
        rb = GetComponent<Rigidbody>();
        
        // Auto-find TargetingManager
        if (targetingManager == null)
        {
            targetingManager = FindAnyObjectByType<TargetingManager>();
        }
        
        // Store original colors
        if (enableHitFlash && renderers != null)
        {
            originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material != null)
                {
                    originalColors[i] = renderers[i].material.color;
                }
            }
        }

        // Setup health bar UI
        if (showHealthBar)
        {
            Debug.Log($"[EnemyHealth] {gameObject.name} - Setting up health bar... showHealthBar=true");
            
            if (healthBarUI == null)
            {
                healthBarUI = GetComponentInChildren<EnemyHealthBarUI>();
                Debug.Log($"[EnemyHealth] {gameObject.name} - Search result: {(healthBarUI != null ? "FOUND" : "NOT FOUND")}");
            }
            
            if (healthBarUI == null && autoCreateHealthBar)
            {
                Debug.Log($"<color=cyan>🔨 AUTO-CREATING health bar for {gameObject.name}</color>");
                CreateHealthBar();
            }
            else if (!autoCreateHealthBar)
            {
                Debug.LogWarning($"⚠️ autoCreateHealthBar is FALSE for {gameObject.name}!");
            }
            
            if (healthBarUI != null)
            {
                healthBarUI.SetHealth(currentHealth, maxHealth);
                healthBarUI.Show(); // Force show
                Debug.Log($"<color=lime>✓ Health bar SHOWN for {gameObject.name}</color>");
            }
            else
            {
                Debug.LogError($"<color=red>✗ Health bar is NULL for {gameObject.name}!</color>");
            }
        }
        else
        {
            Debug.LogWarning($"[EnemyHealth] {gameObject.name} - showHealthBar is FALSE!");
        }
    }
    
    void CreateHealthBar()
    {
        // Create canvas for world space health bar
        GameObject healthBarObj = new GameObject("HealthBar");
        healthBarObj.transform.SetParent(transform);
        healthBarObj.transform.localPosition = new Vector3(0, 2.2f, 0); // Position above enemy
        
        Canvas canvas = healthBarObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        CanvasScaler scaler = healthBarObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;
        
        // Set canvas size (wider and thinner)
        RectTransform canvasRect = healthBarObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2f, 0.3f); // Bigger size
        canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        
        // Create background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(healthBarObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Create border
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(healthBarObj.transform, false);
        UnityEngine.UI.Outline outline = borderObj.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 1f);
        outline.effectDistance = new Vector2(2, 2);
        
        // Create slider
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(healthBarObj.transform, false);
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 1f; // Use normalized 0-1
        slider.value = 1f;
        slider.transition = UnityEngine.UI.Selectable.Transition.None;
        
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.offsetMin = new Vector2(3, 3);
        sliderRect.offsetMax = new Vector2(-3, -3);
        
        // Create fill area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;
        
        // Create fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = new Color(0f, 1f, 0f, 1f); // Bright green
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        slider.fillRect = fillRect;
        
        // Add canvas group for fading
        CanvasGroup canvasGroup = healthBarObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        
        // Add UI script
        healthBarUI = healthBarObj.AddComponent<EnemyHealthBarUI>();
        
        // Use reflection to set private fields
        var fillImageField = typeof(EnemyHealthBarUI).GetField("fillImage", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var sliderField = typeof(EnemyHealthBarUI).GetField("healthSlider", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var bgField = typeof(EnemyHealthBarUI).GetField("backgroundImage", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var cgField = typeof(EnemyHealthBarUI).GetField("canvasGroup", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (fillImageField != null) fillImageField.SetValue(healthBarUI, fillImage);
        if (sliderField != null) sliderField.SetValue(healthBarUI, slider);
        if (bgField != null) bgField.SetValue(healthBarUI, bgImage);
        if (cgField != null) cgField.SetValue(healthBarUI, canvasGroup);
        
        healthBarUI.SetHealth(currentHealth, maxHealth);
        healthBarUI.Show(); // Force show immediately
        
        // Ensure GameObject is active
        healthBarObj.SetActive(true);
        
        Debug.Log($"<color=lime>✅ Health bar CREATED for {gameObject.name}</color>");
        Debug.Log($"  → Canvas position: {healthBarObj.transform.position}");
        Debug.Log($"  → Canvas active: {healthBarObj.activeInHierarchy}");
        Debug.Log($"  → Slider value: {slider.value}");
        Debug.Log($"  → Canvas alpha: {canvasGroup.alpha}");
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        // Check if shield is blocking
        if (isShieldBlocking)
        {
            Debug.Log($"<color=cyan>🛡️ {gameObject.name} SHIELD BLOCKED {damage} damage!</color>");
            return; // HP locked - no damage
        }
        
        // Check if this enemy is the locked target
        if (requireLockToTakeDamage)
        {
            if (!IsLockedTarget())
            {
                Debug.Log($"<color=orange>🛡️ {gameObject.name} BLOCKED damage - Not locked target!</color>");
                return; // Block damage jika tidak di-lock
            }
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        Debug.Log($"💔 {gameObject.name} TOOK {damage} DAMAGE! HP: {currentHealth}/{maxHealth} ({GetHealthPercentage() * 100f:F0}%)");

        // Update health bar UI
        if (healthBarUI != null && showHealthBar)
        {
            Debug.Log($"📊 Updating health bar: {currentHealth}/{maxHealth}");
            healthBarUI.SetHealth(currentHealth, maxHealth);
            healthBarUI.Show();
        }
        else
        {
            Debug.LogWarning($"⚠️ Health bar UI is NULL or showHealthBar is false! UI: {healthBarUI}, Show: {showHealthBar}");
        }
        
        // Spawn hit effect
        if (hitEffectPrefab != null)
        {
            Vector3 hitPos = transform.position + Vector3.up * 1f;
            GameObject effect = Instantiate(hitEffectPrefab, hitPos, Quaternion.identity);
            Destroy(effect, 1f);
        }
        
        // Hit flash effect
        if (enableHitFlash)
        {
            StartCoroutine(HitFlashEffect());
        }

        // Check death
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Hit reaction animation via EnemyAI
            EnemyAI enemyAI = GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.PlayHitReaction();
            }
            
            // Apply knockback
            if (enableKnockback && rb != null)
            {
                Vector3 knockbackDir = transform.position - Camera.main.transform.position;
                knockbackDir.y = 0;
                knockbackDir.Normalize();
                rb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
            }
        }
    }

    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        // Immediately notify TargetingManager to switch target
        if (targetingManager != null)
        {
            Debug.Log($"<color=red>💀 {gameObject.name} DIED - Requesting target switch...</color>");
            // Force update target list immediately
            targetingManager.SendMessage("UpdateTargetList", SendMessageOptions.DontRequireReceiver);
        }

        // Death animation via EnemyAI
        EnemyAI enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.PlayDeathAnimation();
        }

        // Disable collider immediately (prevent further bullet hits)
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Spawn death effect
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Hide health bar
        if (healthBarUI != null)
        {
            healthBarUI.Hide();
        }

        // Destroy after delay (for death animation)
        Destroy(gameObject, destroyDelay);
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }
    
    /// <summary>
    /// Check if this enemy is currently the locked target
    /// </summary>
    public bool IsLockedTarget()
    {
        if (targetingManager == null) 
        {
            // No targeting manager - allow damage (backward compatibility)
            return true;
        }
        
        Transform currentTarget = targetingManager.GetCurrentTarget();
        if (currentTarget == null)
        {
            // No locked target - block damage
            return false;
        }
        
        // Check if this enemy or any parent is the locked target
        Transform checkTransform = transform;
        while (checkTransform != null)
        {
            if (checkTransform == currentTarget)
            {
                return true;
            }
            checkTransform = checkTransform.parent;
        }
        
        return false;
    }
    
    /// <summary>
    /// Set shield blocking state (called by ShieldCycle)
    /// </summary>
    public void SetShieldBlocking(bool blocking)
    {
        isShieldBlocking = blocking;
        if (blocking)
        {
            Debug.Log($"<color=cyan>[EnemyHealth] {gameObject.name} - Shield ACTIVE (HP locked)</color>");
        }
    }

    private bool HasParameter(Animator anim, string paramName)
    {
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
    
    System.Collections.IEnumerator HitFlashEffect()
    {
        // Flash to hit color
        if (renderers != null)
        {
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.color = hitFlashColor;
                }
            }
        }
        
        yield return new WaitForSeconds(hitFlashDuration);
        
        // Return to original colors
        if (renderers != null && originalColors != null)
        {
            for (int i = 0; i < renderers.Length && i < originalColors.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material != null)
                {
                    renderers[i].material.color = originalColors[i];
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Draw health bar gizmo
        Gizmos.color = Color.red;
        Vector3 healthBarPos = transform.position + Vector3.up * 2.5f;
        float healthPercentage = currentHealth / maxHealth;
        Gizmos.DrawLine(healthBarPos - Vector3.right * 0.5f, healthBarPos + Vector3.right * (healthPercentage - 0.5f));

        Gizmos.color = Color.gray;
        Gizmos.DrawLine(healthBarPos + Vector3.right * (healthPercentage - 0.5f), healthBarPos + Vector3.right * 0.5f);
    }
}
