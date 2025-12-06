using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controller untuk player yang mengelola sistem senjata
/// Menggunakan Input System untuk deteksi input
/// </summary>
public class WeaponPlayerController : MonoBehaviour
{
    [Header("Weapon System")]
    [SerializeField] private IAttack currentWeapon;
    
    [Header("Weapon References")]
    [SerializeField] private RifleAttack rifleWeapon;
    [SerializeField] private SwordAttack swordWeapon;
    
    [Header("Animation (Optional)")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private bool useWeaponTypeParameter = false;
    
    private PlayerControllers.PlayerInput playerInputActions;
    private InputAction attackAction;
    private InputAction switchWeaponAction;
    
    private void Awake()
    {
        // Setup Input System
        playerInputActions = new PlayerControllers.PlayerInput();
        
        // Auto-detect animator jika belum di-assign
        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<Animator>();
        }
    }
    
    private void OnEnable()
    {
        if (playerInputActions == null) return;
        
        playerInputActions.Enable();
        
        // Attack dengan Punch button (buttonEast di gamepad / Mouse Left di keyboard)
        attackAction = playerInputActions.Player.Punch;
        if (attackAction != null)
            attackAction.performed += OnAttackPerformed;
        
        // Weapon switching dengan SwitchWeapons action (buttonNorth/Triangle)
        switchWeaponAction = playerInputActions.Player.SwitchWeapons;
        if (switchWeaponAction != null)
            switchWeaponAction.performed += OnSwitchWeapon;
    }
    
    private void OnDisable()
    {
        if (attackAction != null)
            attackAction.performed -= OnAttackPerformed;
        if (switchWeaponAction != null)
            switchWeaponAction.performed -= OnSwitchWeapon;
        if (playerInputActions != null)
            playerInputActions.Disable();
    }
    
    private void Start()
    {
        // Equip rifle secara default jika tersedia
        if (rifleWeapon != null)
        {
            SwitchToRifle();
        }
        else if (swordWeapon != null)
        {
            SwitchToSword();
        }
    }
    
    /// <summary>
    /// Handle weapon switching saat L1 ditekan - toggle between weapons
    /// </summary>
    private void OnSwitchWeapon(InputAction.CallbackContext context)
    {
        // Toggle antara rifle dan sword
        if (currentWeapon == rifleWeapon)
        {
            SwitchToSword();
        }
        else
        {
            SwitchToRifle();
        }
    }
    
    /// <summary>
    /// Callback untuk attack input - Punch button (buttonEast/Mouse Left)
    /// </summary>
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (currentWeapon != null)
        {
            // Check for timing window
            TimingBarUI timingUI = TimingBarUI.Instance;
            if (timingUI != null && timingUI.IsActive())
            {
                ShieldTimingResult result = timingUI.CheckTiming();
                ProcessTimingAttack(result);
            }
            else
            {
                // Normal attack without timing
                currentWeapon.Attack();
            }
        }
    }
    
    /// <summary>
    /// Process attack with timing window result
    /// </summary>
    private void ProcessTimingAttack(ShieldTimingResult result)
    {
        TimingBarUI timingUI = TimingBarUI.Instance;
        if (timingUI == null) return;
        
        Transform target = timingUI.GetCurrentTarget();
        if (target == null) return;
        
        ShieldCycle shieldCycle = target.GetComponent<ShieldCycle>();
        if (shieldCycle == null)
        {
            // No shield cycle, normal attack
            currentWeapon.Attack();
            return;
        }
        
        // Process based on timing result
        switch (result)
        {
            case ShieldTimingResult.Perfect:
                OnPerfectHit(target, shieldCycle);
                break;
                
            case ShieldTimingResult.Good:
                OnGoodHit(target, shieldCycle);
                break;
                
            case ShieldTimingResult.Miss:
                OnBlockedHit(target, shieldCycle);
                break;
        }
        
        // Don't hide timing bar - let EnemyTargetUI handle it
        // Bar stays visible while enemy is locked
    }
    
    /// <summary>
    /// Handle perfect timing hit (Green zone: HP / 1)
    /// </summary>
    private void OnPerfectHit(Transform target, ShieldCycle shieldCycle)
    {
        Debug.Log($"<color=lime>⚡ PERFECT HIT on {target.name} - (HP / 1)!</color>");
        
        // Trigger perfect hit on shield cycle (damage calculated there)
        shieldCycle.OnPerfectHit(0f);
        
        // Don't spawn bullet for Perfect/Good hits
        // Damage is applied directly by ShieldCycle
    }
    
    /// <summary>
    /// Handle good timing hit (Yellow zone: HP / 4)
    /// </summary>
    private void OnGoodHit(Transform target, ShieldCycle shieldCycle)
    {
        Debug.Log($"<color=yellow>✓ GOOD HIT on {target.name} - (HP / 4)</color>");
        
        // Trigger good hit on shield cycle (damage calculated there)
        shieldCycle.OnGoodHit(0f);
        
        // Don't spawn bullet for Perfect/Good hits
        // Damage is applied directly by ShieldCycle
    }
    
    /// <summary>
    /// Handle blocked hit - no damage, trigger counter
    /// </summary>
    private void OnBlockedHit(Transform target, ShieldCycle shieldCycle)
    {
        Debug.Log($"<color=red>✗ BLOCKED by {target.name} - No damage!</color>");
        
        // Shield blocks attack - no damage, no bullet spawn
        shieldCycle.OnBlockedHit();
        
        // Note: No weapon attack on blocked hits
        // Player should time correctly to get Perfect/Good hits
    }
    
    /// <summary>
    /// Get current weapon damage value
    /// </summary>
    private float GetCurrentWeaponDamage()
    {
        if (currentWeapon == null) return 0f;
        
        MonoBehaviour weaponMono = currentWeapon as MonoBehaviour;
        if (weaponMono == null) return 0f;
        
        // Try to get weapon data from different weapon types
        SwordAttack sword = weaponMono as SwordAttack;
        if (sword != null)
        {
            WeaponData data = sword.GetWeaponData();
            return data != null ? data.damage : 0f;
        }
        
        RifleAttack rifle = weaponMono as RifleAttack;
        if (rifle != null)
        {
            WeaponData data = rifle.GetWeaponData();
            return data != null ? data.damage : 0f;
        }
        
        return 10f; // Default damage
    }
    
    /// <summary>
    /// Equip senjata baru
    /// </summary>
    /// <param name="newWeapon">Senjata yang akan di-equip</param>
    public void EquipWeapon(IAttack newWeapon)
    {
        if (newWeapon == null)
        {
            Debug.LogWarning("Trying to equip null weapon!");
            return;
        }
        
        // Disable senjata lama
        if (currentWeapon != null)
        {
            MonoBehaviour weaponMono = currentWeapon as MonoBehaviour;
            if (weaponMono != null)
            {
                weaponMono.gameObject.SetActive(false);
            }
        }
        
        // Enable senjata baru
        currentWeapon = newWeapon;
        MonoBehaviour newWeaponMono = newWeapon as MonoBehaviour;
        if (newWeaponMono != null)
        {
            newWeaponMono.gameObject.SetActive(true);
            Debug.Log($"Equipped weapon: {newWeaponMono.gameObject.name}");
        }
    }
    
    /// <summary>
    /// Switch ke Rifle
    /// </summary>
    public void SwitchToRifle()
    {
        if (rifleWeapon != null)
        {
            EquipWeapon(rifleWeapon);
            
            // Set animator parameter
            if (playerAnimator != null)
            {
                playerAnimator.SetInteger("WeaponType", 1); // 1 = Rifle
                Debug.Log($"WeaponType parameter set to: {playerAnimator.GetInteger("WeaponType")}");
            }
        }
        else
        {
            Debug.LogWarning("Rifle weapon not assigned!");
        }
    }
    
    /// <summary>
    /// Switch ke Sword
    /// </summary>
    public void SwitchToSword()
    {
        if (swordWeapon != null)
        {
            EquipWeapon(swordWeapon);
            
            // Set animator parameter
            if (playerAnimator != null)
            {
                playerAnimator.SetInteger("WeaponType", 2); // 2 = Sword
                Debug.Log($"WeaponType parameter set to: {playerAnimator.GetInteger("WeaponType")}");
            }
        }
        else
        {
            Debug.LogWarning("Sword weapon not assigned!");
        }
    }
    
    /// <summary>
    /// Get current weapon
    /// </summary>
    public IAttack GetCurrentWeapon()
    {
        return currentWeapon;
    }
}
