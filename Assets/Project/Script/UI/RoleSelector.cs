using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoleSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerRole playerRole;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject roleSelectorPanel;
    [SerializeField] private Button strikerButton;
    [SerializeField] private Button sniperButton;
    [SerializeField] private Button confirmButton;
    
    [Header("Role Info Display")]
    [SerializeField] private TextMeshProUGUI roleNameText;
    [SerializeField] private TextMeshProUGUI roleDescriptionText;
    [SerializeField] private TextMeshProUGUI roleStatsText;
    [SerializeField] private Image roleIconBackground;
    
    [Header("Role Colors")]
    [SerializeField] private Color strikerColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private Color sniperColor = new Color(0.3f, 0.7f, 1f);
    [SerializeField] private Color defaultButtonColor = Color.white;
    [SerializeField] private Color selectedButtonColor = new Color(1f, 1f, 0.5f);
    
    [Header("Settings")]
    [SerializeField] private bool showOnStart = true;
    [SerializeField] private bool pauseGameWhenOpen = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.R;
    
    private RoleType selectedRole;
    private bool isOpen = false;
    
    void Start()
    {
        if (playerRole == null)
        {
            playerRole = FindFirstObjectByType<PlayerRole>();
        }
        
        SetupButtons();
        
        selectedRole = playerRole != null ? playerRole.GetCurrentRole() : RoleType.Striker;
        
        if (showOnStart)
        {
            OpenRoleSelector();
        }
        else
        {
            roleSelectorPanel?.SetActive(false);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleRoleSelector();
        }
    }
    
    void SetupButtons()
    {
        if (strikerButton != null)
            strikerButton.onClick.AddListener(() => SelectRole(RoleType.Striker));
            
        if (sniperButton != null)
            sniperButton.onClick.AddListener(() => SelectRole(RoleType.Sniper));
            
        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmRoleSelection);
    }
    
    public void OpenRoleSelector()
    {
        if (roleSelectorPanel != null)
        {
            roleSelectorPanel.SetActive(true);
            isOpen = true;
            
            if (pauseGameWhenOpen)
            {
                Time.timeScale = 0f;
            }
            
            UpdateRoleDisplay(selectedRole);
            UpdateButtonHighlights();
        }
    }
    
    public void CloseRoleSelector()
    {
        if (roleSelectorPanel != null)
        {
            roleSelectorPanel.SetActive(false);
            isOpen = false;
            
            if (pauseGameWhenOpen)
            {
                Time.timeScale = 1f;
            }
        }
    }
    
    public void ToggleRoleSelector()
    {
        if (isOpen)
            CloseRoleSelector();
        else
            OpenRoleSelector();
    }
    
    void SelectRole(RoleType role)
    {
        selectedRole = role;
        UpdateRoleDisplay(role);
        UpdateButtonHighlights();
    }
    
    void UpdateRoleDisplay(RoleType role)
    {
        RoleStats stats = GetRoleStats(role);
        if (stats == null) return;
        
        if (roleNameText != null)
        {
            roleNameText.text = stats.roleName;
            roleNameText.color = GetRoleColor(role);
        }
        
        if (roleDescriptionText != null)
        {
            roleDescriptionText.text = stats.roleDescription;
        }
        
        if (roleStatsText != null)
        {
            roleStatsText.text = FormatRoleStats(stats);
        }
        
        if (roleIconBackground != null)
        {
            roleIconBackground.color = GetRoleColor(role);
        }
    }
    
    string FormatRoleStats(RoleStats stats)
    {
        string statsText = "";
        statsText += $"⚔️ WEAPON LOADOUT\n";
        statsText += $"• Weapon Count: {stats.weaponCount}\n";
        statsText += $"• Damage Bonus: +{stats.weaponDamageModifier}\n";
        statsText += $"• Attack Speed: {stats.attackSpeed}x\n";
        statsText += $"• Range: {stats.attackRange}m\n";
        statsText += $"\n🏋️ MOVEMENT\n";
        statsText += $"• Speed Modifier: {(stats.moveSpeedModifier >= 0 ? "+" : "")}{stats.moveSpeedModifier}\n";
        statsText += $"• Dash: {stats.dashSpeed} ({stats.dashCharges} charges)\n";
        
        if (stats.hasStun)
        {
            statsText += $"\n✨ SPECIAL: Stun ({stats.stunDuration}s)";
        }
        
        if (stats.hasCritical)
        {
            statsText += $"\n✨ SPECIAL: Critical Hit\n";
            statsText += $"   Chance: {stats.critChance * 100}%\n";
            statsText += $"   Multiplier: {stats.critMultiplier}x";
        }
        
        statsText += "\n\n❤️ HP: Not affected";
        
        return statsText;
    }
    
    void UpdateButtonHighlights()
    {
        UpdateButtonColor(strikerButton, RoleType.Striker);
        UpdateButtonColor(sniperButton, RoleType.Sniper);
    }
    
    void UpdateButtonColor(Button button, RoleType role)
    {
        if (button == null) return;
        
        ColorBlock colors = button.colors;
        
        if (role == selectedRole)
        {
            colors.normalColor = selectedButtonColor;
            colors.highlightedColor = selectedButtonColor * 1.2f;
        }
        else
        {
            colors.normalColor = defaultButtonColor;
            colors.highlightedColor = defaultButtonColor * 0.8f;
        }
        
        button.colors = colors;
    }
    
    void ConfirmRoleSelection()
    {
        if (playerRole != null)
        {
            playerRole.SwitchRole(selectedRole);
            Debug.Log($"Role switched to: {selectedRole}");
        }
        
        CloseRoleSelector();
    }
    
    RoleStats GetRoleStats(RoleType role)
    {
        if (playerRole == null) return null;
        
        // Just return the active stats for the current role
        // UI will only show current loadout (no preview of other loadouts)
        return playerRole.GetActiveStats();
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
}
