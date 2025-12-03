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
    private InputAction movementAction;
    
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
        playerInputActions.Enable();
        
        // Attack dengan Punch button (buttonEast di gamepad / Mouse Left di keyboard)
        attackAction = playerInputActions.Player.Punch;
        attackAction.performed += OnAttackPerformed;
        
        // Weapon switching dengan movement (D-pad atau Left Stick)
        movementAction = playerInputActions.Player.Movement;
    }
    
    private void OnDisable()
    {
        attackAction.performed -= OnAttackPerformed;
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
    
    private void Update()
    {
        // Handle weapon switching dengan D-pad/stick horizontal
        HandleWeaponSwitching();
    }
    
    /// <summary>
    /// Handle weapon switching dengan movement input horizontal
    /// </summary>
    private void HandleWeaponSwitching()
    {
        Vector2 movement = movementAction.ReadValue<Vector2>();
        
        // Switch ke Rifle dengan arah kiri (D-pad left atau stick left)
        if (movement.x < -0.8f && !isProcessingSwitch)
        {
            SwitchToRifle();
            StartCoroutine(SwitchCooldown());
        }
        // Switch ke Sword dengan arah kanan (D-pad right atau stick right)
        else if (movement.x > 0.8f && !isProcessingSwitch)
        {
            SwitchToSword();
            StartCoroutine(SwitchCooldown());
        }
    }
    
    private bool isProcessingSwitch = false;
    
    /// <summary>
    /// Cooldown untuk mencegah switch berulang kali
    /// </summary>
    private System.Collections.IEnumerator SwitchCooldown()
    {
        isProcessingSwitch = true;
        yield return new WaitForSeconds(0.3f);
        isProcessingSwitch = false;
    }
    
    /// <summary>
    /// Callback untuk attack input - Punch button (buttonEast/Mouse Left)
    /// </summary>
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (currentWeapon != null)
        {
            currentWeapon.Attack();
        }
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
