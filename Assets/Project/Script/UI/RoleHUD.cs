using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoleHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerRole playerRole;
    
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI roleNameText;
    [SerializeField] private Image roleIconImage;
    [SerializeField] private GameObject specialAbilityIndicator;
    [SerializeField] private TextMeshProUGUI specialAbilityText;
    
    [Header("Colors")]
    [SerializeField] private Color strikerColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private Color sniperColor = new Color(0.3f, 0.7f, 1f);
    
    void Start()
    {
        if (playerRole == null)
        {
            playerRole = FindFirstObjectByType<PlayerRole>();
        }
        
        UpdateRoleDisplay();
    }
    
    void Update()
    {
        UpdateRoleDisplay();
    }
    
    void UpdateRoleDisplay()
    {
        if (playerRole == null) return;
        
        RoleType currentRole = playerRole.GetCurrentRole();
        Color roleColor = GetRoleColor(currentRole);
        
        if (roleNameText != null)
        {
            roleNameText.text = playerRole.GetRoleName();
            roleNameText.color = roleColor;
        }
        
        if (roleIconImage != null)
        {
            roleIconImage.color = roleColor;
        }
        
        UpdateSpecialAbilityDisplay();
    }
    
    void UpdateSpecialAbilityDisplay()
    {
        if (specialAbilityIndicator == null || specialAbilityText == null) return;
        
        RoleStats stats = playerRole.GetActiveStats();
        if (stats == null) return;
        
        if (stats.hasStun)
        {
            specialAbilityIndicator.SetActive(true);
            specialAbilityText.text = "⚡ STUN";
        }
        else if (stats.hasCritical)
        {
            specialAbilityIndicator.SetActive(true);
            specialAbilityText.text = $"💥 CRIT {stats.critChance * 100:F0}%";
        }
        else
        {
            specialAbilityIndicator.SetActive(false);
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
}
