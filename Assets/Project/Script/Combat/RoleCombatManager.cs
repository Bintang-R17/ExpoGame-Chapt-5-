using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Combat manager that routes input to the correct role-specific combat handler
/// Automatically switches combat behavior when player changes role
/// EXECUTION ORDER: -100 (runs before most scripts to ensure input setup)
/// </summary>
[DefaultExecutionOrder(-100)]
public class RoleCombatManager : MonoBehaviour
{
    [Header("References")]
    private PlayerRole playerRole;
    private PlayerInput playerInput;
    
    [Header("Combat Handlers")]
    private StrikerCombat strikerCombat;
    private SniperCombat sniperCombat;
    
    private IRoleCombat currentCombatHandler;
    
    void Awake()
    {
        playerRole = GetComponent<PlayerRole>();
        playerInput = GetComponent<PlayerInput>();
        
        // Get all combat handlers
        strikerCombat = GetComponent<StrikerCombat>();
        sniperCombat = GetComponent<SniperCombat>();
        
        // Initialize handlers
        if (strikerCombat != null) strikerCombat.Initialize(gameObject);
        if (sniperCombat != null) sniperCombat.Initialize(gameObject);
    }
    
    void Start()
    {
        // Subscribe to input events via C# delegates
        // SendMessages will also fire, but our methods have different names (Handle* vs On*)
        if (playerInput != null && playerInput.actions != null)
        {
            var punchAction = playerInput.actions.FindAction("Punch");
            var skill1Action = playerInput.actions.FindAction("Skill1");
            var skill2Action = playerInput.actions.FindAction("Skill2");
            
            if (punchAction != null) punchAction.performed += HandlePunchInput;
            if (skill1Action != null) skill1Action.performed += HandleSkill1Input;
            if (skill2Action != null) skill2Action.performed += HandleSkill2Input;
            
            Debug.Log("✅ RoleCombatManager: Combat input events subscribed (via delegates)");
        }
        else
        {
            Debug.LogError("❌ RoleCombatManager: PlayerInput or actions not found!");
        }
        
        // Set initial combat handler
        UpdateCombatHandler();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from input events
        if (playerInput != null && playerInput.actions != null)
        {
            var punchAction = playerInput.actions.FindAction("Punch");
            var skill1Action = playerInput.actions.FindAction("Skill1");
            var skill2Action = playerInput.actions.FindAction("Skill2");
            
            if (punchAction != null) punchAction.performed -= HandlePunchInput;
            if (skill1Action != null) skill1Action.performed -= HandleSkill1Input;
            if (skill2Action != null) skill2Action.performed -= HandleSkill2Input;
        }
        
        // Cleanup handlers
        if (strikerCombat != null) strikerCombat.Cleanup();
        if (sniperCombat != null) sniperCombat.Cleanup();
    }
    
    void Update()
    {
        // Check if role changed (allow first-time null handler)
        if (playerRole != null)
        {
            if (currentCombatHandler == null || playerRole.GetCurrentRole() != currentCombatHandler.GetRoleType())
            {
                UpdateCombatHandler();
            }
        }
    }
    
    void UpdateCombatHandler()
    {
        if (playerRole == null)
        {
            Debug.LogError("❌ UpdateCombatHandler: PlayerRole is NULL!");
            currentCombatHandler = null;
            return;
        }
        
        RoleType currentRole = playerRole.GetCurrentRole();
        Debug.Log($"🔄 UpdateCombatHandler: Switching to {currentRole}");
        
        // Enable appropriate combat handler
        switch (currentRole)
        {
            case RoleType.Striker:
                currentCombatHandler = strikerCombat;
                EnableCombatScript(strikerCombat, true);
                EnableCombatScript(sniperCombat, false);
                Debug.Log("⚔️ Combat switched to STRIKER");
                break;
                
            case RoleType.Sniper:
                currentCombatHandler = sniperCombat;
                EnableCombatScript(strikerCombat, false);
                EnableCombatScript(sniperCombat, true);
                Debug.Log("🎯 Combat switched to SNIPER");
                break;
                
            default:
                currentCombatHandler = null;
                EnableCombatScript(strikerCombat, false);
                EnableCombatScript(sniperCombat, false);
                Debug.LogWarning($"⚠️ Unknown role type: {currentRole}");
                break;
        }
        
        Debug.Log($"✅ Combat handler now: {currentCombatHandler?.GetType().Name ?? "NULL"}");
    }
    
    void EnableCombatScript(MonoBehaviour script, bool enabled)
    {
        if (script != null)
        {
            script.enabled = enabled;
        }
    }
    
    // Input routing methods - Completely renamed to prevent ANY SendMessage detection
    void HandlePunchInput(InputAction.CallbackContext context)
    {
        Debug.Log($"👊 HandlePunchInput called! Current handler: {currentCombatHandler?.GetType().Name ?? "NULL"}");
        
        if (currentCombatHandler != null)
        {
            Debug.Log($"➡️ Routing to {currentCombatHandler.GetRoleType()} combat handler");
            currentCombatHandler.HandleBasicAttack(context);
        }
        else
        {
            Debug.LogWarning("⚠️ No combat handler active for current role!");
        }
    }
    
    void HandleSkill1Input(InputAction.CallbackContext context)
    {
        if (currentCombatHandler != null)
        {
            currentCombatHandler.HandleSkill1(context);
        }
    }
    
    void HandleSkill2Input(InputAction.CallbackContext context)
    {
        if (currentCombatHandler != null)
        {
            currentCombatHandler.HandleSkill2(context);
        }
    }
    
    // Public API
    public IRoleCombat GetCurrentCombatHandler() => currentCombatHandler;
    public RoleType GetCurrentCombatRole() => currentCombatHandler?.GetRoleType() ?? RoleType.Striker;
}
