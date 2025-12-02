using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Health UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Text healthText;
    [SerializeField] private Image healthFill;
    [SerializeField] private Gradient healthGradient; // Red to green

    [Header("Stamina UI")]
    [SerializeField] private Slider staminaBar;
    [SerializeField] private Text staminaText;
    [SerializeField] private Image staminaFill;

    [Header("Level UI")]
    [SerializeField] private Text levelText;
    [SerializeField] private Slider experienceBar;
    [SerializeField] private Text experienceText;

    [Header("Stats Panel")]
    [SerializeField] private Text strengthText;
    [SerializeField] private Text agilityText;
    [SerializeField] private Text vitalityText;
    [SerializeField] private Text enduranceText;
    [SerializeField] private Text intelligenceText;

    [Header("Derived Stats")]
    [SerializeField] private Text attackPowerText;
    [SerializeField] private Text defenseText;
    [SerializeField] private Text critChanceText;

    [Header("References")]
    [SerializeField] private PlayerHealth healthSystem;
    [SerializeField] private PlayerStamina staminaSystem;
    [SerializeField] private PlayerStatsManager statsManager;

    void Start()
    {
        // Auto-find references if not assigned
        if (healthSystem == null)
            healthSystem = FindFirstObjectByType<PlayerHealth>();

        if (staminaSystem == null)
            staminaSystem = FindFirstObjectByType<PlayerStamina>();

        if (statsManager == null)
            statsManager = FindFirstObjectByType<PlayerStatsManager>();

        // Auto-find health fill if not assigned
        if (healthFill == null && healthBar != null && healthBar.fillRect != null)
            healthFill = healthBar.fillRect.GetComponent<Image>();

        // Subscribe to events
        if (statsManager != null)
        {
            statsManager.OnLevelUp += OnPlayerLevelUp;
            statsManager.OnExperienceGained += OnPlayerExperienceGained;
        }
        
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged.AddListener(OnHealthChanged);
        }

        UpdateAllUI();
        
        Debug.Log($"✅ PlayerUI initialized - Health: {healthSystem?.CurrentHealth}/{healthSystem?.MaxHealth}");
    }
    
    void OnHealthChanged(float healthPercentage)
    {
        // Immediately update when health changes
        UpdateHealthUI();
    }

    void Update()
    {
        UpdateHealthUI();
        UpdateStaminaUI();
        UpdateExperienceUI();
    }

    void UpdateHealthUI()
    {
        if (healthSystem == null || healthBar == null) return;

        // Set slider max value to match player max health
        if (healthBar.maxValue != healthSystem.MaxHealth)
        {
            healthBar.maxValue = healthSystem.MaxHealth;
            healthBar.minValue = 0;
        }

        // Update slider value to current health
        healthBar.value = healthSystem.CurrentHealth;
        
        float healthPercent = healthSystem.HealthPercentage;

        if (healthText != null)
            healthText.text = $"{Mathf.Ceil(healthSystem.CurrentHealth)}/{healthSystem.MaxHealth}";

        // Color gradient based on percentage
        if (healthFill != null && healthGradient != null)
            healthFill.color = healthGradient.Evaluate(healthPercent);
            
        // Also update color if no gradient
        if (healthFill != null && healthGradient == null)
        {
            if (healthPercent > 0.6f)
                healthFill.color = Color.green;
            else if (healthPercent > 0.3f)
                healthFill.color = new Color(1f, 0.92f, 0.016f, 1f); // Yellow
            else
                healthFill.color = Color.red;
        }
    }

    void UpdateStaminaUI()
    {
        if (staminaSystem == null || staminaBar == null) return;

        staminaBar.value = staminaSystem.StaminaPercentage;

        if (staminaText != null)
            staminaText.text = $"{Mathf.Ceil(staminaSystem.CurrentStamina)}/{staminaSystem.MaxStamina}";
    }

    void UpdateExperienceUI()
    {
        if (statsManager == null) return;

        if (levelText != null)
            levelText.text = $"Level {statsManager.Level}";

        if (experienceBar != null)
        {
            float expPercent = (float)statsManager.Experience / statsManager.Stats.experienceToNextLevel;
            experienceBar.value = expPercent;
        }

        if (experienceText != null)
            experienceText.text = $"{statsManager.Experience}/{statsManager.Stats.experienceToNextLevel}";
    }

    void UpdateStatsUI()
    {
        if (statsManager == null) return;

        PlayerStats stats = statsManager.Stats;

        if (strengthText != null)
            strengthText.text = $"STR: {stats.strength}";

        if (agilityText != null)
            agilityText.text = $"AGI: {stats.agility}";

        if (vitalityText != null)
            vitalityText.text = $"VIT: {stats.vitality}";

        if (enduranceText != null)
            enduranceText.text = $"END: {stats.endurance}";

        if (intelligenceText != null)
            intelligenceText.text = $"INT: {stats.intelligence}";

        if (attackPowerText != null)
            attackPowerText.text = $"ATK: {stats.attackPower}";

        if (defenseText != null)
            defenseText.text = $"DEF: {stats.defense}";

        if (critChanceText != null)
            critChanceText.text = $"CRIT: {stats.critChance:F1}%";
    }

    void UpdateAllUI()
    {
        UpdateHealthUI();
        UpdateStaminaUI();
        UpdateExperienceUI();
        UpdateStatsUI();
    }

    void OnPlayerLevelUp(int newLevel)
    {
        Debug.Log($"UI: Player reached level {newLevel}!");
        UpdateAllUI();

        // Bisa tambahkan level up animation/effect di sini
    }

    void OnPlayerExperienceGained(int amount)
    {
        UpdateExperienceUI();
    }

    void OnDestroy()
    {
        // Unsubscribe events
        if (statsManager != null)
        {
            statsManager.OnLevelUp -= OnPlayerLevelUp;
            statsManager.OnExperienceGained -= OnPlayerExperienceGained;
        }
        
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged.RemoveListener(OnHealthChanged);
        }
    }
}