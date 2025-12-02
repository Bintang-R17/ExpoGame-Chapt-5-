using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public enum RoleType
{
    Striker,    // Close combat, burst damage
    Sniper      // Long range, precision shots (renamed from Analyst)
}

[System.Serializable]
public class RoleStats
{
    [Header("Weapon Loadout Identity")]
    public RoleType roleType;
    public string roleName;
    [TextArea] public string roleDescription;
    
    [Header("Movement Stats (Weapon Weight)")]
    public float moveSpeedModifier = 0f;  // Added to base speed
    public float jumpForceModifier = 0f;  // Added to base jump force
    public float dashSpeed = 15f;
    public int dashCharges = 1;
    public float dashCooldown = 2f;
    
    [Header("Weapon Stats")]
    public int weaponCount = 6;
    public float weaponDamageModifier = 0f;  // Added to base damage
    public float attackSpeed = 1f;           // Attack rate multiplier
    public float attackRange = 20f;
    public float projectileSpeed = 20f;
    
    [Header("Special Abilities")]
    public bool hasStun = false;
    public float stunDuration = 1f;
    public bool hasCritical = false;
    public float critMultiplier = 2f;
    public float critChance = 0.2f;
}

public class PlayerRole : MonoBehaviour
{
    [Header("Current Weapon Loadout")]
    [SerializeField] private RoleType currentRole = RoleType.Striker;
    
    [Header("Weapon Loadout Configurations")]
    [Tooltip("Loadouts only affect: Damage, Movement Speed, Attack Speed, Weapon Count")]
    [SerializeField] private RoleStats strikerStats;
    [SerializeField] private RoleStats sniperStats;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color strikerColor = new Color(1f, 0.3f, 0.3f); // Red
    [SerializeField] private Color sniperColor = new Color(0.3f, 0.7f, 1f); // Blue
    
    [Header("System Control")]
    [SerializeField] private bool enableRoleSystem = false; // DISABLED BY DEFAULT
    
    [Header("Character Models")]
    [Tooltip("GameObject dengan model & Animator untuk Striker")]
    [SerializeField] private GameObject strikerCharacterModel;
    [Tooltip("GameObject dengan model & Animator untuk Sniper")]
    [SerializeField] private GameObject sniperCharacterModel;
    
    private RoleStats activeStats;
    private PlayerController playerController;
    private OrbitalWeapons orbitalWeapons;
    private PlayerStatsManager statsManager;
    private SniperVisualEffect sniperVisualEffect;
    private Animator currentAnimator; // Animator dari model yang aktif
    
    // Store base stats before weapon loadout modifiers (NO HP!)
    private float baseMoveSpeed;
    private float baseJumpForce;
    private float baseAttackPower;
    
    private RoleType lastRole;
    
    void Start()
    {
        InitializeRoleStats();
        
        playerController = GetComponent<PlayerController>();
        orbitalWeapons = GetComponent<OrbitalWeapons>();
        statsManager = GetComponent<PlayerStatsManager>();
        sniperVisualEffect = GetComponent<SniperVisualEffect>();
        
        // Auto-create SniperVisualEffect if not present
        if (sniperVisualEffect == null)
        {
            sniperVisualEffect = gameObject.AddComponent<SniperVisualEffect>();
            Debug.Log("🎯 SniperVisualEffect component auto-created");
        }
        
        // Initialize character models
        InitializeCharacterModels();
        
        // Store base stats from PlayerStatsManager (NO HP - not affected by weapon loadout!)
        if (statsManager != null)
        {
            baseMoveSpeed = 2f + (statsManager.Stats.agility * 0.15f);
            baseAttackPower = statsManager.Stats.attackPower;
        }
        else
        {
            // Fallback if no PlayerStatsManager
            baseMoveSpeed = 5f;
            baseAttackPower = 10f;
        }
        
        // Store base jump force from PlayerController
        if (playerController != null)
        {
            baseJumpForce = playerController.GetJumpForce();
        }
        else
        {
            baseJumpForce = 8f; // Default
        }
        
        lastRole = currentRole;
        
        // Only apply weapon loadout if system is enabled
        if (enableRoleSystem)
        {
            SwitchRole(currentRole);
        }
        else
        {
            Debug.Log("⚠️ Weapon loadout system DISABLED - Using base stats only");
        }
    }
    
    void Update()
    {
        // Handle gamepad role switching (works even if role system disabled)
        HandleGamepadInput();
        
        // Only track role changes if system is enabled
        if (!enableRoleSystem) return;
        
        // Detect role change from Inspector during runtime
        if (currentRole != lastRole)
        {
            Debug.Log($"🔄 Role changed from {lastRole} to {currentRole}");
            lastRole = currentRole;
            SwitchRole(currentRole);
        }
    }
    
    void HandleGamepadInput()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null) return;
        
        // D-pad Up = Toggle/Cycle between roles
        if (gamepad.dpad.up.wasPressedThisFrame)
        {
            RoleType nextRole = currentRole == RoleType.Striker ? RoleType.Sniper : RoleType.Striker;
            SwitchRole(nextRole);
            Debug.Log($"🎮 Role toggled to: {nextRole}");
        }
        
        // D-pad Left or LB = Direct select Striker
        if (gamepad.dpad.left.wasPressedThisFrame || gamepad.leftShoulder.wasPressedThisFrame)
        {
            SwitchRole(RoleType.Striker);
            Debug.Log($"🎮 Role switched to: Striker");
        }
        
        // D-pad Right or RB = Direct select Sniper
        else if (gamepad.dpad.right.wasPressedThisFrame || gamepad.rightShoulder.wasPressedThisFrame)
        {
            SwitchRole(RoleType.Sniper);
            Debug.Log($"🎮 Role switched to: Sniper");
        }
    }
    
    // Called when values change in Inspector (Editor only)
    void OnValidate()
    {
        if (!Application.isPlaying) return;
        if (!enableRoleSystem) return;
        
        // Force role switch when changed in Inspector during Play mode
        if (activeStats == null || activeStats.roleType != currentRole)
        {
            InitializeRoleStats();
            SwitchRole(currentRole);
        }
    }
    
    void InitializeRoleStats()
    {
        // STRIKER - Heavy Weapons Loadout (Normal Movement)
        strikerStats = new RoleStats
        {
            roleType = RoleType.Striker,
            roleName = "STRIKER",
            roleDescription = "Heavy weapons loadout. Normal movement, devastating close-range damage.",
            moveSpeedModifier = 0f,       // Normal speed
            jumpForceModifier = 0f,       // Normal jump
            dashSpeed = 12f,
            dashCharges = 2,
            dashCooldown = 2f,
            weaponCount = 4,
            weaponDamageModifier = 40f,   // +40 damage (heavy hitting)
            attackSpeed = 0.7f,           // Slower attack rate
            attackRange = 8f,
            projectileSpeed = 20f,
            hasStun = true,
            stunDuration = 2f,
            hasCritical = false
        };
        
        // SNIPER - Precision Weapons Loadout (Fast & High Jump)
        sniperStats = new RoleStats
        {
            roleType = RoleType.Sniper,
            roleName = "SNIPER",
            roleDescription = "Long range precision loadout. Fast movement with high jump for positioning.",
            moveSpeedModifier = 2f,       // Faster speed (+2)
            jumpForceModifier = 3f,       // Higher jump (+3)
            dashSpeed = 15f,
            dashCharges = 1,
            dashCooldown = 2.5f,
            weaponCount = 6,
            weaponDamageModifier = 15f,   // +15 damage (precision shots)
            attackSpeed = 0.9f,           // Slightly slower (precise aiming)
            attackRange = 40f,
            projectileSpeed = 60f,
            hasStun = false,
            hasCritical = true,
            critMultiplier = 3.5f,
            critChance = 0.35f
        };
    }
    
    public void SwitchRole(RoleType newRole)
    {
        if (!enableRoleSystem)
        {
            Debug.LogWarning("⚠️ Weapon loadout system is disabled - Cannot switch loadout");
            return;
        }
        
        currentRole = newRole;
        
        switch (newRole)
        {
            case RoleType.Striker:
                activeStats = strikerStats;
                break;
            case RoleType.Sniper:
                activeStats = sniperStats;
                break;
        }
        
        ApplyRoleStats();
        UpdateVisuals();
        UpdateRoleVisualEffects();
        SwitchCharacterModel();
    }
    
    void InitializeCharacterModels()
    {
        // Auto-detect character models if not assigned
        if (strikerCharacterModel == null)
        {
            Transform striker = transform.Find("Striker");
            if (striker != null)
            {
                strikerCharacterModel = striker.gameObject;
                Debug.Log("✅ Striker model auto-detected");
            }
        }
        
        if (sniperCharacterModel == null)
        {
            Transform sniper = transform.Find("Sniper");
            if (sniper != null)
            {
                sniperCharacterModel = sniper.gameObject;
                Debug.Log("✅ Sniper model auto-detected");
            }
        }
        
        // ALWAYS switch character model on startup to disable inactive role
        SwitchCharacterModel();
    }
    
    void SwitchCharacterModel()
    {
        // Deactivate all models first
        if (strikerCharacterModel != null)
            strikerCharacterModel.SetActive(false);
        if (sniperCharacterModel != null)
            sniperCharacterModel.SetActive(false);
        
        // Activate current role model (even if role system disabled)
        GameObject activeModel = null;
        
        switch (currentRole)
        {
            case RoleType.Striker:
                if (strikerCharacterModel != null)
                {
                    strikerCharacterModel.SetActive(true);
                    activeModel = strikerCharacterModel;
                    currentAnimator = strikerCharacterModel.GetComponent<Animator>();
                    Debug.Log("🎬 Striker character model activated");
                }
                break;
                
            case RoleType.Sniper:
                if (sniperCharacterModel != null)
                {
                    sniperCharacterModel.SetActive(true);
                    activeModel = sniperCharacterModel;
                    currentAnimator = sniperCharacterModel.GetComponent<Animator>();
                    Debug.Log("🎬 Sniper character model activated");
                }
                break;
        }
        
        // Update PlayerController reference to active animator
        if (playerController != null && currentAnimator != null)
        {
            playerController.SetAnimator(currentAnimator);
        }
    }
    
    void ApplyRoleStats()
    {
        if (activeStats == null) return;
        if (!enableRoleSystem) return; // Skip if system disabled
        
        // Calculate combined stats (base + weapon loadout modifiers)
        // NOTE: HP is NOT affected by weapon loadout!
        float totalMoveSpeed = GetTotalMoveSpeed();
        float totalJumpForce = GetTotalJumpForce();
        float totalDamage = GetTotalWeaponDamage();
        float attackSpeedMultiplier = GetAttackSpeed();
        
        // Apply to PlayerController if exists
        if (playerController != null)
        {
            playerController.SetMaxSpeed(totalMoveSpeed);
            playerController.SetJumpForce(totalJumpForce);
            Debug.Log($"⚔️ Role [{activeStats.roleName}] Speed: {totalMoveSpeed} (Base: {baseMoveSpeed} + Modifier: {activeStats.moveSpeedModifier})");
            Debug.Log($"⚔️ Role [{activeStats.roleName}] Jump: {totalJumpForce} (Base: {baseJumpForce} + Modifier: {activeStats.jumpForceModifier})");
        }
        
        // HP is NOT modified by weapon loadout - player keeps their base HP!
        
        // Trigger OrbitalWeapons to reinitialize with new weapon count
        if (orbitalWeapons != null)
        {
            Debug.Log($"⚔️ Weapon Loadout [{activeStats.roleName}]:");
            Debug.Log($"   • {activeStats.weaponCount} weapons");
            Debug.Log($"   • Damage: {totalDamage} (Base: {baseAttackPower} + Modifier: {activeStats.weaponDamageModifier})");
            Debug.Log($"   • Attack Speed: {attackSpeedMultiplier}x");
        }
    }
    
    void UpdateVisuals()
    {
        Color roleColor = GetRoleColor();
        
        // Update weapon trail colors
        if (orbitalWeapons != null)
        {
            // orbitalWeapons.SetTrailColor(roleColor);
        }
        
        // Update player material glow (optional)
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer.material.HasProperty("_EmissionColor"))
            {
                renderer.material.SetColor("_EmissionColor", roleColor * 0.5f);
            }
        }
        
        // Handle role-specific visual effects
        UpdateRoleVisualEffects();
    }
    
    void UpdateRoleVisualEffects()
    {
        // Activate/deactivate role-specific visual effects
        if (sniperVisualEffect != null)
        {
            if (currentRole == RoleType.Sniper && enableRoleSystem)
            {
                sniperVisualEffect.ActivateEffect();
            }
            else
            {
                sniperVisualEffect.DeactivateEffect();
            }
        }
    }
    
    // Public getter for current animator
    public Animator GetCurrentAnimator() => currentAnimator;
    
    Color GetRoleColor()
    {
        switch (currentRole)
        {
            case RoleType.Striker: return strikerColor;
            case RoleType.Sniper: return sniperColor;
            default: return Color.white;
        }
    }
    
    // Public getters - Combined Stats (Base + Role Bonus)
    public RoleType GetCurrentRole() => currentRole;
    public RoleStats GetActiveStats() => activeStats;
    public string GetRoleName() => activeStats?.roleName ?? "Unknown";
    public bool IsRoleSystemEnabled() => enableRoleSystem;
    
    // WEAPON LOADOUT MODIFIERS: Base + Modifier (only if system enabled)
    // HP is NOT affected by weapon loadout!
    public float GetTotalMoveSpeed() => enableRoleSystem ? (baseMoveSpeed + (activeStats?.moveSpeedModifier ?? 0f)) : baseMoveSpeed;
    public float GetTotalJumpForce() => enableRoleSystem ? (baseJumpForce + (activeStats?.jumpForceModifier ?? 0f)) : baseJumpForce;
    public float GetTotalWeaponDamage() => enableRoleSystem ? (baseAttackPower + (activeStats?.weaponDamageModifier ?? 0f)) : baseAttackPower;
    public float GetTotalDashSpeed() => enableRoleSystem ? (activeStats?.dashSpeed ?? 15f) : 15f;
    
    // Legacy method - now returns combined damage
    public float GetWeaponDamage() => GetTotalWeaponDamage();
    
    // Role-specific abilities (from role only, not additive) - Return defaults if system disabled
    public bool HasStun() => enableRoleSystem ? (activeStats?.hasStun ?? false) : false;
    public bool HasCritical() => enableRoleSystem ? (activeStats?.hasCritical ?? false) : false;
    public float GetCritChance() => enableRoleSystem ? (activeStats?.critChance ?? 0f) : 0f;
    public float GetCritMultiplier() => enableRoleSystem ? (activeStats?.critMultiplier ?? 1f) : 1f;
    public float GetAttackSpeed() => enableRoleSystem ? (activeStats?.attackSpeed ?? 1f) : 1f;
    public int GetWeaponCount() => enableRoleSystem ? (activeStats?.weaponCount ?? 6) : 6;
    public int GetDashCharges() => enableRoleSystem ? (activeStats?.dashCharges ?? 1) : 1;
    public float GetDashCooldown() => enableRoleSystem ? (activeStats?.dashCooldown ?? 2f) : 2f;
    
    // Base stats getters (HP not included - loadout doesn't affect HP)
    public float GetBaseMoveSpeed() => baseMoveSpeed;
    public float GetBaseAttackPower() => baseAttackPower;
    
    // Update base stats (dipanggil oleh PlayerStatsManager saat stats berubah)
    // NOTE: HP tidak diupdate karena weapon loadout tidak mempengaruhi HP!
    public void UpdateBaseStats(float newBaseSpeed, float newBaseDamage)
    {
        baseMoveSpeed = newBaseSpeed;
        baseAttackPower = newBaseDamage;
        
        // Reapply weapon loadout modifiers dengan base stats yang baru (only if system enabled)
        if (enableRoleSystem)
        {
            ApplyRoleStats();
            Debug.Log($"📊 Base stats updated - Speed: {baseMoveSpeed}, Damage: {baseAttackPower} (HP unchanged)");
        }
    }
    
    // Enable/disable weapon loadout system at runtime
    public void SetRoleSystemEnabled(bool enabled)
    {
        enableRoleSystem = enabled;
        
        if (enabled && activeStats != null)
        {
            ApplyRoleStats();
            Debug.Log("✅ Weapon loadout system ENABLED - Modifies: Damage, Speed, Attack Rate");
        }
        else
        {
            Debug.Log("⚠️ Weapon loadout system DISABLED - Using base stats only (HP always unchanged)");
        }
    }
}
