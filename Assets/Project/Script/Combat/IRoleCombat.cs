using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Interface for role-specific combat behavior
/// Each role implements this to define unique attack patterns
/// </summary>
public interface IRoleCombat
{
    void HandleBasicAttack(InputAction.CallbackContext context);
    void HandleSkill1(InputAction.CallbackContext context);
    void HandleSkill2(InputAction.CallbackContext context);
    
    void Initialize(GameObject owner);
    void Cleanup();
    
    RoleType GetRoleType();
}
