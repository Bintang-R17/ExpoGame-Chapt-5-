using UnityEngine;

/// <summary>
/// Component untuk memberikan damage ke player saat collision/trigger
/// Attach ke GameObject enemy atau attack hitbox
/// </summary>
public class DamageDealer : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float damageInterval = 1f; // Cooldown antara damage
    [SerializeField] private bool destroyOnHit = false;
    [SerializeField] private bool disableOnHit = false;
    
    [Header("Target Layers")]
    [SerializeField] private LayerMask targetLayers = ~0; // All layers by default
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private float lastDamageTime;
    
    void Start()
    {
        lastDamageTime = -damageInterval;
    }
    
    void OnCollisionEnter(Collision collision)
    {
        TryDealDamage(collision.gameObject);
    }
    
    void OnCollisionStay(Collision collision)
    {
        // Continuous damage while in contact (with cooldown)
        if (Time.time >= lastDamageTime + damageInterval)
        {
            TryDealDamage(collision.gameObject);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        TryDealDamage(other.gameObject);
    }
    
    void OnTriggerStay(Collider other)
    {
        // Continuous damage while in trigger (with cooldown)
        if (Time.time >= lastDamageTime + damageInterval)
        {
            TryDealDamage(other.gameObject);
        }
    }
    
    void TryDealDamage(GameObject target)
    {
        // Check if we can damage this target based on layer
        if (((1 << target.layer) & targetLayers) == 0)
            return;
            
        // Try to find PlayerHealth component
        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
        
        if (playerHealth != null)
        {
            if (showDebugLogs)
            {
                Debug.Log($"💥 {gameObject.name} dealt {damageAmount} damage to {target.name}");
            }
            
            playerHealth.TakeDamage(damageAmount);
            lastDamageTime = Time.time;
            
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
            else if (disableOnHit)
            {
                gameObject.SetActive(false);
            }
        }
        else if (showDebugLogs)
        {
            Debug.LogWarning($"⚠️ {gameObject.name} collided with {target.name} but no PlayerHealth found");
        }
    }
    
    // Public method to set damage (for dynamic damage)
    public void SetDamage(float damage)
    {
        damageAmount = damage;
    }
    
    // Public method to enable/disable
    public void SetActive(bool active)
    {
        enabled = active;
    }
}
