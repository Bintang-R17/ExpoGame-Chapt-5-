using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

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
            if (healthBarUI == null)
            {
                healthBarUI = GetComponentInChildren<EnemyHealthBarUI>();
            }
            
            if (healthBarUI == null && autoCreateHealthBar)
            {
                Debug.Log($"🔨 Creating health bar for {gameObject.name}");
                CreateHealthBar();
            }
            
            if (healthBarUI != null)
            {
                healthBarUI.SetHealth(currentHealth, maxHealth);
                Debug.Log($"✅ {gameObject.name} health bar initialized: {currentHealth}/{maxHealth}");
            }
            else
            {
                Debug.LogWarning($"⚠️ {gameObject.name} health bar UI not found!");
            }
        }
    }
    
    void CreateHealthBar()
    {
        // Create canvas for world space health bar
        GameObject healthBarObj = new GameObject("HealthBar");
        healthBarObj.transform.SetParent(transform);
        healthBarObj.transform.localPosition = Vector3.zero;
        
        Canvas canvas = healthBarObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        CanvasScaler scaler = healthBarObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;
        
        // Set canvas size
        RectTransform canvasRect = healthBarObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2f, 0.3f);
        
        // Create background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(healthBarObj.transform);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Create slider
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(healthBarObj.transform);
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = maxHealth;
        slider.value = currentHealth;
        
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.offsetMin = new Vector2(5, 5);
        sliderRect.offsetMax = new Vector2(-5, -5);
        
        // Create fill area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform);
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;
        
        // Create fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform);
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = Color.green;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        slider.fillRect = fillRect;
        
        // Add UI script
        healthBarUI = healthBarObj.AddComponent<EnemyHealthBarUI>();
        healthBarUI.SetHealth(currentHealth, maxHealth);
        
        Debug.Log($"✅ Created health bar for {gameObject.name}");
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // Show damage feedback
        Debug.Log($"⚔️ {gameObject.name} took {damage} damage! HP: {currentHealth}/{maxHealth}");

        // Update health bar UI
        if (healthBarUI != null && showHealthBar)
        {
            healthBarUI.SetHealth(currentHealth, maxHealth);
            healthBarUI.Show();
            Debug.Log($"📊 Updated health bar: {currentHealth}/{maxHealth}");
        }
        else if (showHealthBar)
        {
            Debug.LogWarning($"⚠️ Health bar UI is null for {gameObject.name}!");
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

        // Death animation via EnemyAI
        EnemyAI enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.PlayDeathAnimation();
        }

        // Disable collider
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

        // Destroy after delay
        Destroy(gameObject, destroyDelay);
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
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
