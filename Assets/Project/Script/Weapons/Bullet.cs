using UnityEngine;

/// <summary>
/// Script untuk projectile bullet yang ditembakkan rifle
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 5f;
    
    [Header("Effects (Optional)")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private TrailRenderer trailRenderer;
    
    private Rigidbody rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    private void Start()
    {
        // Set velocity
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
        }
        
        // Auto destroy setelah lifetime
        Destroy(gameObject, lifetime);
    }
    
    /// <summary>
    /// Set damage dari luar (dipanggil oleh RifleAttack)
    /// </summary>
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
    
    /// <summary>
    /// Set speed dari luar (dipanggil oleh RifleAttack)
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Jangan hit shooter sendiri
        if (other.CompareTag("Player"))
        {
            return;
        }
        
        // Cek apakah target bisa di-damage
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            Debug.Log($"Bullet hit {other.name} for {damage} damage!");
        }
        else
        {
            // Fallback: kirim message
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }
        
        // Spawn hit effect
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Destroy bullet
        Destroy(gameObject);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Handle collision (jika tidak menggunakan trigger)
        OnTriggerEnter(collision.collider);
    }
}
