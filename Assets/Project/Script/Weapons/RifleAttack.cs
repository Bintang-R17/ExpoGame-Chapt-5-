using UnityEngine;

/// <summary>
/// Implementasi serangan untuk senjata tipe Rifle
/// </summary>
public class RifleAttack : MonoBehaviour, IAttack
{
    [Header("Weapon Data")]
    [SerializeField] private WeaponData weaponData;
    
    [Header("Rifle Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 20f;
    
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
    }
    
    /// <summary>
    /// Melakukan serangan tembakan
    /// </summary>
    public void Attack()
    {
        // Cek cooldown
        if (Time.time < nextFireTime)
        {
            return;
        }
        
        // Validasi
        if (firePoint == null)
        {
            Debug.LogWarning("Fire Point belum diset pada " + gameObject.name);
            return;
        }
        
        if (bulletPrefab == null)
        {
            Debug.LogWarning("Bullet Prefab belum diset pada " + gameObject.name);
            return;
        }
        
        // Spawn bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        // Set bullet properties
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null && weaponData != null)
        {
            bulletScript.SetDamage(weaponData.damage);
            bulletScript.SetSpeed(bulletSpeed);
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
    /// Visualisasi FirePoint di editor
    /// </summary>
    private void OnDrawGizmos()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(firePoint.position, 0.05f);
            Gizmos.DrawRay(firePoint.position, firePoint.forward * 2f);
        }
    }
}
