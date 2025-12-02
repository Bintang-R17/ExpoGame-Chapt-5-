using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isInvulnerable = false;

    [Header("Regeneration")]
    [SerializeField] private bool enableHealthRegen = true;
    [SerializeField] private float regenAmount = 5f; // Health per detik
    [SerializeField] private float regenDelay = 3f; // Delay setelah kena damage
    private float timeSinceLastDamage;

    [Header("Visual Effects")]
    [SerializeField] private Image damageOverlay; // Red screen effect
    [SerializeField] private float damageFlashDuration = 0.2f;
    private float damageFlashTimer;

    [Header("Events")]
    public UnityEvent OnDeath;
    public UnityEvent<float> OnHealthChanged; // Pass current health percentage

    // Properties
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float HealthPercentage => currentHealth / maxHealth;
    public bool IsDead => currentHealth <= 0;

    void Start()
    {
        // Initialize health
        currentHealth = maxHealth;
        timeSinceLastDamage = regenDelay;

        if (damageOverlay != null)
        {
            Color overlayColor = damageOverlay.color;
            overlayColor.a = 0f;
            damageOverlay.color = overlayColor;
        }
    }

    void Update()
    {
        // Health regeneration
        if (enableHealthRegen && !IsDead)
        {
            timeSinceLastDamage += Time.deltaTime;

            if (timeSinceLastDamage >= regenDelay && currentHealth < maxHealth)
            {
                Heal(regenAmount * Time.deltaTime);
            }
        }

        // Damage flash effect
        if (damageFlashTimer > 0)
        {
            damageFlashTimer -= Time.deltaTime;

            if (damageOverlay != null)
            {
                Color overlayColor = damageOverlay.color;
                overlayColor.a = Mathf.Lerp(0f, 0.5f, damageFlashTimer / damageFlashDuration);
                damageOverlay.color = overlayColor;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (IsDead || isInvulnerable) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        // Reset regen timer
        timeSinceLastDamage = 0f;

        // Visual feedback
        TriggerDamageFlash();

        // Trigger event (ImprovedPlayerHUD listens to this)
        OnHealthChanged?.Invoke(HealthPercentage);

        Debug.Log($"Player took {damage} damage! Health: {currentHealth}/{maxHealth}");

        // Check death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        OnHealthChanged?.Invoke(HealthPercentage);
    }

    public void SetInvulnerable(bool invulnerable)
    {
        isInvulnerable = invulnerable;
    }

    void Die()
    {
        Debug.Log("Player died!");

        // Trigger death event
        OnDeath?.Invoke();

        // Disable player control
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
            controller.enabled = false;

        // Bisa tambahkan:
        // - Death animation
        // - Respawn system
        // - Game over screen
    }

    void TriggerDamageFlash()
    {
        damageFlashTimer = damageFlashDuration;
    }

    // Respawn function
    public void Respawn(Vector3 spawnPosition)
    {
        currentHealth = maxHealth;
        transform.position = spawnPosition;

        // Re-enable player control
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
            controller.enabled = true;

        Debug.Log("Player respawned!");
    }

    // Public methods untuk sistem lain
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    public void FullHeal()
    {
        currentHealth = maxHealth;
    }
}