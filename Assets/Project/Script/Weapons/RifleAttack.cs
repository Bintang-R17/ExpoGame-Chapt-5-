using UnityEngine;

/// <summary>
/// Implementasi serangan untuk senjata tipe Rifle - Direct Hit (Simplified)
/// </summary>
public class RifleAttack : MonoBehaviour, IAttack
{
    [Header("Weapon Data")]
    [SerializeField] private WeaponData weaponData;
    
    [Header("Rifle Settings")]
    [SerializeField] private Transform firePoint;
    
    [Header("Targeting")]
    [SerializeField] private bool onlyDamageLockedTarget = true;
    [SerializeField] private TargetingManager targetingManager;
    
    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab; // Hit effect on target
    [SerializeField] private GameObject muzzleFlashPrefab; // Muzzle flash on gun
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioSource audioSource;
    
    private float nextFireTime = 0f;
    
    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        // Auto-find TargetingManager
        if (targetingManager == null)
        {
            targetingManager = FindAnyObjectByType<TargetingManager>();
        }
    }
    
    /// <summary>
    /// Melakukan serangan tembakan langsung ke target (no bullet spawn)
    /// </summary>
    public void Attack()
    {
        // Cek cooldown
        if (Time.time < nextFireTime)
        {
            return;
        }
        
        // Check for locked target
        Transform lockedTarget = null;
        if (onlyDamageLockedTarget && targetingManager != null)
        {
            lockedTarget = targetingManager.GetCurrentTarget();
            if (lockedTarget == null)
            {
                Debug.Log("<color=orange>[Rifle] No locked target - cannot shoot!</color>");
                return; // Tidak ada target yang di-lock
            }
        }
        
        // Validasi firePoint
        if (firePoint == null)
        {
            Debug.LogWarning("Fire Point belum diset pada " + gameObject.name);
            return;
        }
        
        // Direct hit on locked target
        if (lockedTarget != null)
        {
            // Apply damage directly
            EnemyHealth enemyHealth = lockedTarget.GetComponent<EnemyHealth>();
            if (enemyHealth != null && weaponData != null)
            {
                enemyHealth.TakeDamage(weaponData.damage);
                Debug.Log($"<color=lime>[Rifle] Direct hit on {lockedTarget.name} for {weaponData.damage} damage!</color>");
                
                // Spawn hit effect on target
                if (hitEffectPrefab != null)
                {
                    Vector3 hitPos = lockedTarget.position + Vector3.up * 1f; // Above target
                    GameObject hitEffect = Instantiate(hitEffectPrefab, hitPos, Quaternion.identity);
                    Destroy(hitEffect, 1f);
                }
            }
        }
        
        // Spawn muzzle flash effect
        if (muzzleFlashPrefab != null && firePoint != null)
        {
            GameObject muzzle = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            Destroy(muzzle, 0.5f);
        }
        
        // Play sound
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
        
        // Set cooldown
        if (weaponData != null)
        {
            nextFireTime = Time.time + weaponData.attackCooldown;
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
    /// Get weapon damage value
    /// </summary>
    public float GetDamage()
    {
        return weaponData != null ? weaponData.damage : 0f;
    }
    
    /// <summary>
    /// Visualisasi FirePoint di editor
    /// </summary>
    private void OnDrawGizmos()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red; // Red for direct hit rifle
            Gizmos.DrawSphere(firePoint.position, 0.05f);
            Gizmos.DrawRay(firePoint.position, firePoint.forward * 2f);
        }
    }
}
