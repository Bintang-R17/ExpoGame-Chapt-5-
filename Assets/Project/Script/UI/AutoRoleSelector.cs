using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Automatic role selector - No setup needed!
/// Just attach this to any GameObject and press 1,2,3,4 to switch roles
/// </summary>
public class AutoRoleSelector : MonoBehaviour
{
    private PlayerRole playerRole;
    
    [Header("Colors")]
    [SerializeField] private Color strikerColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private Color sniperColor = new Color(0.3f, 0.7f, 1f);
    
    [Header("Display Settings")]
    [SerializeField] private bool showRoleInfo = true;
    [SerializeField] private int fontSize = 16;
    
    void Start()
    {
        // Automatically find PlayerRole in scene
        playerRole = FindFirstObjectByType<PlayerRole>();
        
        if (playerRole == null)
        {
            Debug.LogError("PlayerRole not found! Make sure Player has PlayerRole component.");
        }
        else
        {
            Debug.Log($"<color=green>✅ Role system ready! Press 1-2 to switch roles</color>");
        }
    }
    
    void Update()
    {
        if (playerRole == null) return;
        
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Press 1 or 2 to switch roles (only Striker and Sniper available)
        if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
        {
            SwitchRole(RoleType.Striker);
        }
        else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
        {
            SwitchRole(RoleType.Sniper);
        }
    }
    
    void SwitchRole(RoleType newRole)
    {
        if (playerRole != null)
        {
            playerRole.SwitchRole(newRole);
            
            // Visual feedback
            string roleName = GetRoleName(newRole);
            Color roleColor = GetRoleColor(newRole);
            Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGB(roleColor)}>🎯 Role switched to: {roleName}</color>");
        }
    }
    
    string GetRoleName(RoleType role)
    {
        switch (role)
        {
            case RoleType.Striker: return "STRIKER";
            case RoleType.Sniper: return "SNIPER";
            default: return "UNKNOWN";
        }
    }
    
    Color GetRoleColor(RoleType role)
    {
        switch (role)
        {
            case RoleType.Striker: return strikerColor;
            case RoleType.Sniper: return sniperColor;
            default: return Color.white;
        }
    }
    
    void OnGUI()
    {
        if (!showRoleInfo || playerRole == null) return;
        
        RoleType currentRole = playerRole.GetCurrentRole();
        RoleStats stats = playerRole.GetActiveStats();
        if (stats == null) return;
        
        // Style
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = fontSize;
        boxStyle.fontStyle = FontStyle.Bold;
        boxStyle.normal.textColor = GetRoleColor(currentRole);
        boxStyle.alignment = TextAnchor.UpperLeft;
        boxStyle.padding = new RectOffset(10, 10, 10, 10);
        
        // Role info text
        string icon = GetRoleIcon(currentRole);
        string displayText = $"{icon} [{(int)currentRole + 1}] {stats.roleName}\n" +
                           $"━━━━━━━━━━━━━━━━\n" +
                           $"⚔️  Weapons: {stats.weaponCount}\n" +
                           $"💥 Damage: +{stats.weaponDamageModifier}\n" +
                           $"⚡ Attack Speed: {stats.attackSpeed}x\n" +
                           $"🏃 Speed Mod: {(stats.moveSpeedModifier >= 0 ? "+" : "")}{stats.moveSpeedModifier}";
        
        if (stats.hasStun)
            displayText += $"\n⚡ STUN ({stats.stunDuration}s)";
        if (stats.hasCritical)
            displayText += $"\n💥 CRIT {stats.critChance*100:F0}% (x{stats.critMultiplier})";
        
        displayText += "\n━━━━━━━━━━━━━━━━\n";
        displayText += "❤️ HP: Not affected\n";
        displayText += "━━━━━━━━━━━━━━━━\n";
        displayText += "Press 1 or 2";
        
        // Draw box
        GUI.Box(new Rect(10, 10, 220, 220), displayText, boxStyle);
    }
    
    string GetRoleIcon(RoleType role)
    {
        switch (role)
        {
            case RoleType.Striker: return "🗡️";
            case RoleType.Sniper: return "🎯";
            default: return "❓";
        }
    }
}
