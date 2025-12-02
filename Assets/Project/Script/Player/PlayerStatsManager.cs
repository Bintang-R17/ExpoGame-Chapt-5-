using UnityEngine;

[System.Serializable]
public class PlayerStats
{
    [Header("Base Stats")]
    public int level = 1;
    public int experience = 0;
    public int experienceToNextLevel = 100;
    
    [Header("Core Attributes")]
    public float strength = 10f;      // Melee damage
    public float agility = 10f;       // Attack speed, dodge
    public float vitality = 10f;      // Max health
    public float endurance = 10f;     // Max stamina
    public float intelligence = 10f;  // Magic damage
    
    [Header("Derived Stats")]
    public float attackPower = 20f;
    public float defense = 5f;
    public float critChance = 5f;     // Percentage
    public float critDamage = 150f;   // Percentage
    public float moveSpeedBonus = 0f; // Percentage
}

public class PlayerStatsManager : MonoBehaviour
{
    [SerializeField] private PlayerStats stats = new PlayerStats();
    
    // References
    private PlayerHealth healthSystem;
    private PlayerStamina staminaSystem;
    private PlayerController controller;
    private PlayerRole roleSystem;
    
    // Events
    public System.Action<int> OnLevelUp;
    public System.Action<int> OnExperienceGained;
    
    // Properties
    public PlayerStats Stats => stats;
    public int Level => stats.level;
    public int Experience => stats.experience;
    
    void Awake()
    {
        healthSystem = GetComponent<PlayerHealth>();
        staminaSystem = GetComponent<PlayerStamina>();
        controller = GetComponent<PlayerController>();
        roleSystem = GetComponent<PlayerRole>();
        
        // Calculate initial derived stats
        RecalculateStats();
    }
    
    void Start()
    {
        // Apply stats to other systems
        ApplyStatsToSystems();
    }
    
    public void RecalculateStats()
    {
        // Derived stats based on attributes
        stats.attackPower = 10f + (stats.strength * 2f);
        stats.defense = 5f + (stats.vitality * 0.5f);
        stats.critChance = 5f + (stats.agility * 0.3f);
        stats.critDamage = 150f + (stats.agility * 1f);
        stats.moveSpeedBonus = stats.agility * 0.5f;
        
        // Update systems
        ApplyStatsToSystems();
    }
    
    void ApplyStatsToSystems()
    {
        // Calculate base stats (foundation yang ditambah role bonus)
        float baseMaxHealth = 80f + (stats.vitality * 8f);     // Base HP lebih kecil, role dominan
        float baseMaxStamina = 100f + (stats.endurance * 5f);
        float baseMoveSpeed = 2f + (stats.agility * 0.15f);    // Base speed kecil, role yang tentukan playstyle
        
        // Update role system with new base stats (weapon loadout doesn't affect HP)
        if (roleSystem != null)
        {
            roleSystem.UpdateBaseStats(baseMoveSpeed, stats.attackPower);
            Debug.Log($"📊 Updated base stats in role system - Speed: {baseMoveSpeed}, Power: {stats.attackPower} (HP unchanged)");
        }
        else
        {
            // If no role system, apply directly to health and stamina
            if (healthSystem != null)
            {
                healthSystem.SetMaxHealth(baseMaxHealth);
            }
            
            if (staminaSystem != null)
            {
                staminaSystem.SetMaxStamina(baseMaxStamina);
            }
        }
        
        // Apply to stamina (not affected by role)
        if (staminaSystem != null)
        {
            staminaSystem.SetMaxStamina(baseMaxStamina);
        }
    }
    
    public void GainExperience(int amount)
    {
        stats.experience += amount;
        OnExperienceGained?.Invoke(amount);
        
        Debug.Log($"Gained {amount} XP! Total: {stats.experience}/{stats.experienceToNextLevel}");
        
        // Check level up
        while (stats.experience >= stats.experienceToNextLevel)
        {
            LevelUp();
        }
    }
    
    void LevelUp()
    {
        stats.level++;
        stats.experience -= stats.experienceToNextLevel;
        stats.experienceToNextLevel = Mathf.RoundToInt(stats.experienceToNextLevel * 1.5f);
        
        // Increase base stats
        stats.strength += 2;
        stats.agility += 2;
        stats.vitality += 3;
        stats.endurance += 2;
        stats.intelligence += 2;
        
        // Recalculate
        RecalculateStats();
        
        // Full heal on level up
        if (healthSystem != null)
            healthSystem.FullHeal();
        
        if (staminaSystem != null)
            staminaSystem.FullRestore();
        
        OnLevelUp?.Invoke(stats.level);
        Debug.Log($"LEVEL UP! Now level {stats.level}");
    }
    
    // Manual stat increase
    public void IncreaseStrength(float amount)
    {
        stats.strength += amount;
        RecalculateStats();
    }
    
    public void IncreaseAgility(float amount)
    {
        stats.agility += amount;
        RecalculateStats();
    }
    
    public void IncreaseVitality(float amount)
    {
        stats.vitality += amount;
        RecalculateStats();
    }
    
    public void IncreaseEndurance(float amount)
    {
        stats.endurance += amount;
        RecalculateStats();
    }
    
    public void IncreaseIntelligence(float amount)
    {
        stats.intelligence += amount;
        RecalculateStats();
    }
    
    // Calculate damage with crit
    public float CalculateDamage(float baseDamage)
    {
        float finalDamage = baseDamage + stats.attackPower;
        
        // Roll crit chance
        if (Random.Range(0f, 100f) < stats.critChance)
        {
            finalDamage *= (stats.critDamage / 100f);
            Debug.Log("CRITICAL HIT!");
        }
        
        return finalDamage;
    }
    
    // Take damage with defense
    public float CalculateDamageReduction(float incomingDamage)
    {
        // Simple damage reduction formula
        float reduction = stats.defense / (stats.defense + 100f);
        float finalDamage = incomingDamage * (1f - reduction);
        
        return Mathf.Max(finalDamage, 1f); // Minimum 1 damage
    }
}
