using UnityEngine;

/// <summary>
/// Implementasi serangan untuk senjata tipe Sword (melee)
/// </summary>
public class SwordAttack : MonoBehaviour, IAttack
{
    [Header("Weapon Data")]
    [SerializeField] private WeaponData weaponData;
    
    [Header("Sword Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform hitboxCenter;
    [SerializeField] private float hitboxRadius = 1.5f;
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip swingSound;
    [SerializeField] private AudioSource audioSource;
    
    private float nextAttackTime = 0f;
    
    private void Awake()
    {
        // Auto-detect animator: cari di GameObject ini dulu, baru parent
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (animator == null)
        {
            animator = GetComponentInParent<Animator>();
        }
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        if (hitboxCenter == null)
        {
            hitboxCenter = transform;
        }
    }
    
    /// <summary>
    /// Melakukan serangan melee
    /// </summary>
    public void Attack()
    {
        // Cek cooldown
        if (Time.time < nextAttackTime)
        {
            return;
        }
        
        // Trigger animasi
        if (animator != null)
        {
            Debug.Log($"[SWORD] Current AnimatorState hash: {animator.GetCurrentAnimatorStateInfo(0).fullPathHash}");
            Debug.Log($"[SWORD] WeaponType parameter value: {animator.GetInteger("WeaponType")}");
            animator.SetTrigger("Attack");
            Debug.Log("[SWORD] Attack trigger set!");
        }
        else
        {
            Debug.LogWarning("[SWORD] Animator is NULL!");
        }
        
        // Play sound
        if (audioSource != null && swingSound != null)
        {
            audioSource.PlayOneShot(swingSound);
        }
        
        // Lakukan damage detection
        PerformMeleeAttack();
        
        // Set cooldown
        if (weaponData != null)
        {
            nextAttackTime = Time.time + weaponData.attackCooldown;
        }
    }
    
    /// <summary>
    /// Deteksi dan damage enemy dalam range
    /// </summary>
    private void PerformMeleeAttack()
    {
        if (weaponData == null) return;
        
        // Deteksi semua collider dalam radius
        Collider[] hitColliders = Physics.OverlapSphere(
            hitboxCenter.position, 
            weaponData.attackRange > 0 ? weaponData.attackRange : hitboxRadius, 
            enemyLayer
        );
        
        // Apply damage ke semua enemy yang terdeteksi
        foreach (Collider collider in hitColliders)
        {
            // Cek apakah ada component health/damageable
            IDamageable damageable = collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(weaponData.damage);
                Debug.Log($"Hit {collider.name} for {weaponData.damage} damage!");
            }
            else
            {
                // Fallback: cari component dengan method TakeDamage
                collider.SendMessage("TakeDamage", weaponData.damage, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
    
    /// <summary>
    /// Get weapon data untuk referensi
    /// </summary>
    public WeaponData GetWeaponData()
    {
        return weaponData;
    }
    
    /// <summary>
    /// Visualisasi hitbox di editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (hitboxCenter == null) return;
        
        Gizmos.color = Color.red;
        float radius = weaponData != null && weaponData.attackRange > 0 
            ? weaponData.attackRange 
            : hitboxRadius;
        Gizmos.DrawWireSphere(hitboxCenter.position, radius);
    }
}
