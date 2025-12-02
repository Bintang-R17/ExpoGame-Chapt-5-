using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Debug script to diagnose Input System configuration issues
/// Attach this to Player GameObject to see detailed input setup info
/// </summary>
public class InputSystemDebugger : MonoBehaviour
{
    private PlayerInput playerInput;
    
    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        
        if (playerInput == null)
        {
            Debug.LogError("❌ InputSystemDebugger: No PlayerInput component found!");
            return;
        }
        
        Debug.Log("=== INPUT SYSTEM DEBUGGER ===");
        Debug.Log($"🎮 Current Device: {playerInput.currentControlScheme}");
        Debug.Log($"📢 Notification Behavior: {playerInput.notificationBehavior}");
        Debug.Log($"📋 Action Map: {playerInput.currentActionMap?.name ?? "NULL"}");
        Debug.Log($"🔗 Actions Asset: {playerInput.actions?.name ?? "NULL"}");
        
        if (playerInput.notificationBehavior == PlayerNotifications.SendMessages)
        {
            Debug.LogWarning("⚠️⚠️⚠️ WARNING: PlayerInput is using SendMessages mode!");
            Debug.LogWarning("⚠️ This will cause Unity to look for OnPunch(), OnSkill1() methods!");
            Debug.LogWarning("⚠️ RoleCombatManager should force InvokeCSharpEvents in Awake()");
        }
        else
        {
            Debug.Log("✅ PlayerInput is using InvokeCSharpEvents (correct!)");
        }
        
        // List all available actions
        Debug.Log("📋 Available Actions:");
        if (playerInput.actions != null)
        {
            foreach (var action in playerInput.actions)
            {
                Debug.Log($"  - {action.name} ({action.type})");
            }
        }
    }
    
    void Start()
    {
        // Check again after Start to see if RoleCombatManager changed it
        if (playerInput != null)
        {
            Debug.Log($"🔄 [After Start] Notification Behavior: {playerInput.notificationBehavior}");
        }
    }
    
    void OnEnable()
    {
        // Monitor input action triggers
        if (playerInput != null && playerInput.actions != null)
        {
            playerInput.actions["Punch"].performed += ctx => Debug.Log("🥊 PUNCH action triggered!");
            playerInput.actions["Skill1"].performed += ctx => Debug.Log("⚡ SKILL1 action triggered!");
            
            if (playerInput.actions.FindAction("Skill2") != null)
            {
                playerInput.actions["Skill2"].performed += ctx => Debug.Log("💫 SKILL2 action triggered!");
            }
        }
    }
}
