using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Simple role selector - Press 1,2,3,4 keys to switch roles
/// Drag the PLAYER GameObject (yang punya PlayerRole component) ke field di bawah
/// </summary>
public class SimpleRoleSelector : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag Player GameObject here (the one with PlayerRole component)")]
    [SerializeField] private PlayerRole playerRole;
    
    [Header("UI (Optional)")]
    [SerializeField] private Text roleDisplayText;
    
    [Header("Colors")]
    [SerializeField] private Color strikerColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private Color sniperColor = new Color(0.3f, 0.7f, 1f);
    
    void Start()
    {
        if (playerRole == null)
        {
            playerRole = FindFirstObjectByType<PlayerRole>();
        }
        
        UpdateDisplay();
    }
    
    void Update()
    {
        var keyboard = Keyboard.current;
        
        // Keyboard only: Press 1 or 2 to switch roles
        // (Gamepad input handled by PlayerRole.cs)
        if (keyboard != null)
        {
            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
            {
                SwitchRole(RoleType.Striker);
            }
            else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
            {
                SwitchRole(RoleType.Sniper);
            }
        }
    }
    
    void SwitchRole(RoleType newRole)
    {
        if (playerRole != null)
        {
            playerRole.SwitchRole(newRole);
            UpdateDisplay();
            
            // Visual feedback
            Debug.Log($"<color=yellow>Role switched to: {newRole}</color>");
        }
    }
    
    void UpdateDisplay()
    {
        if (playerRole == null || roleDisplayText == null) return;
        
        RoleType currentRole = playerRole.GetCurrentRole();
        RoleStats stats = playerRole.GetActiveStats();
        
        if (stats != null)
        {
            roleDisplayText.text = $"LOADOUT: {stats.roleName}\n" +
                                  $"Speed Mod: {(stats.moveSpeedModifier >= 0 ? "+" : "")}{stats.moveSpeedModifier}\n" +
                                  $"Weapons: {stats.weaponCount} | Dmg: +{stats.weaponDamageModifier}";
            roleDisplayText.color = GetRoleColor(currentRole);
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
        // Simple on-screen display
        if (playerRole == null) return;
        
        RoleType currentRole = playerRole.GetCurrentRole();
        RoleStats stats = playerRole.GetActiveStats();
        if (stats == null) return;
        
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 16;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = GetRoleColor(currentRole);
        style.alignment = TextAnchor.UpperLeft;
        
        string displayText = $"[{(int)currentRole + 1}] {stats.roleName}\n" +
                           $"⚔️ {stats.weaponCount} Weapons (+{stats.weaponDamageModifier} dmg)\n" +
                           $"⚡ Attack Speed: {stats.attackSpeed}x\n" +
                           $"🏃 Speed Mod: {(stats.moveSpeedModifier >= 0 ? "+" : "")}{stats.moveSpeedModifier}";
        
        if (stats.hasStun)
            displayText += $"\n⚡ STUN {stats.stunDuration}s";
        if (stats.hasCritical)
            displayText += $"\n💥 CRIT {stats.critChance*100:F0}% (x{stats.critMultiplier})";
        
        displayText += "\n\n❤️ HP: Not affected";
        displayText += "\n\nKeyboard: 1/2";
        displayText += "\nGamepad: D-pad Up (cycle)";
        displayText += "\n         D-pad L/R or LB/RB";
        displayText += "\n(Input via PlayerRole.cs)";
        
        GUI.Box(new Rect(10, 10, 280, 230), displayText, style);
    }
}
