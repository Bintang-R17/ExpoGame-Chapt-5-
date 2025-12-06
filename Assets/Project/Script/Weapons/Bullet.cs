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
    
    [Header("Targeting")]
    [SerializeField] private bool onlyDamageLockedTarget = true;
    [SerializeField] private bool enableHoming = true; // Track target while flying
    [SerializeField] private float homingStrength = 5f; // Rotation speed toward target
    private Transform lockedTarget; // Target yang di-lock
    
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
    
    private void Update()
    {
        // Homing behavior - track locked target
        if (enableHoming && lockedTarget != null && rb != null)
        {
            // Check if target is still valid (not destroyed)
            if (lockedTarget.gameObject == null || !lockedTarget.gameObject.activeInHierarchy)
            {
                // Target destroyed - disable homing
                enableHoming = false;
                lockedTarget = null;
                return;
            }
            
            // Check if target is dead (has EnemyHealth component)
            EnemyHealth enemyHealth = lockedTarget.GetComponent<EnemyHealth>();
            if (enemyHealth != null && enemyHealth.IsDead())
            {
                // Target is dead - disable homing
                Debug.Log($"<color=yellow>[Bullet] Target {lockedTarget.name} is dead - stop homing</color>");
                enableHoming = false;
                lockedTarget = null;
                return;
            }
            
            Vector3 directionToTarget = (lockedTarget.position - transform.position).normalized;
            Vector3 currentDirection = rb.linearVelocity.normalized;
            
            // Check if direction is valid (prevent NaN)
            if (directionToTarget.sqrMagnitude < 0.01f || currentDirection.sqrMagnitude < 0.01f)
            {
                return;
            }
            
            // Smoothly rotate toward target
            Vector3 newDirection = Vector3.Slerp(currentDirection, directionToTarget, homingStrength * Time.deltaTime);
            rb.linearVelocity = newDirection * speed;
            
            // Update bullet rotation to face movement direction
            transform.rotation = Quaternion.LookRotation(newDirection);
        }
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
    
    /// <summary>
    /// Set locked target (bullet hanya damage target ini)
    /// </summary>
    public void SetLockedTarget(Transform target)
    {
        if (target != null && target.gameObject != null)
        {
            lockedTarget = target;
            onlyDamageLockedTarget = true;
            Debug.Log($"<color=cyan>[Bullet] Locked to target: {target.name}</color>");
        }
        else
        {
            Debug.LogWarning("[Bullet] SetLockedTarget called with null/destroyed target!");
            lockedTarget = null;
            onlyDamageLockedTarget = false;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Jangan hit shooter sendiri
        if (other.CompareTag("Player"))
        {
            return;
        }
        
        // Check if target is locked target or its child
        bool isLockedTarget = false;
        if (onlyDamageLockedTarget)
        {
            if (lockedTarget == null)
            {
                // No locked target - bullet passes through
                Debug.Log($"<color=orange>[Bullet] No locked target - pass through {other.name}</color>");
                return;
            }
            
            // Strict check: must be locked target or direct child
            Transform checkTransform = other.transform;
            while (checkTransform != null)
            {
                if (checkTransform == lockedTarget)
                {
                    isLockedTarget = true;
                    break;
                }
                checkTransform = checkTransform.parent;
            }
            
            if (!isLockedTarget)
            {
                Debug.Log($"<color=orange>[Bullet] Hit {other.name} but NOT locked target ({lockedTarget.name}) - ignored!</color>");
                return; // Tidak damage enemy yang tidak di-lock
            }
        }
        
        // Cek apakah target bisa di-damage
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            Debug.Log($"<color=lime>[Bullet] Hit LOCKED target {other.name} for {damage} damage!</color>");
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
        
        // Destroy bullet immediately
        Destroy(gameObject);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Handle collision (jika tidak menggunakan trigger)
        OnTriggerEnter(collision.collider);
    }
}
