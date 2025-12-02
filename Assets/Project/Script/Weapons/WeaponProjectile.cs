using UnityEngine;

public class WeaponProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float homingStrength = 5f;
    [SerializeField] private bool enableHoming = true;
    
    [Header("Hit Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float hitEffectLifetime = 1f;
    [SerializeField] private bool destroyOnHit = true;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float hitSoundVolume = 0.7f;
    [SerializeField] private float pitchMin = 0.9f;
    [SerializeField] private float pitchMax = 1.1f;
    [SerializeField] private bool use3DSound = true;

    private Transform target;
    private Vector3 direction;
    private float aliveTime;
    private bool hasHit;
    private TrailRenderer trail;
    private PlayerRole playerRole;

    public void Initialize(Transform targetTransform, Vector3 initialDirection, float damageAmount, PlayerRole role = null)
    {
        target = targetTransform;
        direction = initialDirection.normalized;
        damage = damageAmount;
        hasHit = false;
        aliveTime = 0f;
        playerRole = role;

        trail = GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.enabled = true;
        }
    }

    void Update()
    {
        if (hasHit) return;

        aliveTime += Time.deltaTime;

        // Auto destroy after lifetime
        if (aliveTime >= lifetime)
        {
            ReturnToPool();
            return;
        }

        // Homing behavior
        if (enableHoming && target != null)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            direction = Vector3.Lerp(direction, directionToTarget, homingStrength * Time.deltaTime).normalized;
        }

        // Move forward
        transform.position += direction * speed * Time.deltaTime;

        // Rotate for visual effect
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime, Space.Self);

        // Point towards direction
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f) * Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        // Calculate hit position (closest point on collider to weapon)
        Vector3 hitPosition = other.ClosestPoint(transform.position);

        // Check for enemy by component (safer than tag check)
        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null && !enemyHealth.IsDead())
        {
            float finalDamage = damage;
            
            // Check for critical hit
            if (playerRole != null && playerRole.HasCritical())
            {
                float critChance = playerRole.GetCritChance();
                if (Random.value < critChance)
                {
                    float critMultiplier = playerRole.GetCritMultiplier();
                    finalDamage *= critMultiplier;
                    Debug.Log($"💥 CRITICAL HIT! {damage} x {critMultiplier} = {finalDamage}");
                }
            }
            
            // Apply damage
            enemyHealth.TakeDamage(finalDamage);
            
            // Check for stun effect
            if (playerRole != null && playerRole.HasStun())
            {
                EnemyAI enemyAI = other.GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    // Trigger stun animation/effect
                    enemyAI.PlayHitReaction();
                    Debug.Log("⚡ Enemy stunned!");
                }
            }
            
            SpawnHitEffect(hitPosition, other.transform);
            PlayHitSound(hitPosition);
            
            hasHit = true;
            
            if (destroyOnHit)
            {
                ReturnToPool();
            }
            return;
        }
        
        // Hit obstacle/wall/ground - check by layer
        int groundLayer = LayerMask.NameToLayer("Ground");
        int defaultLayer = LayerMask.NameToLayer("Default");
        
        if (other.gameObject.layer == groundLayer || 
            other.gameObject.layer == defaultLayer ||
            other.gameObject.isStatic)
        {
            SpawnHitEffect(hitPosition, null);
            PlayHitSound(hitPosition);
            hasHit = true;
            ReturnToPool();
        }
    }
    
    void PlayHitSound(Vector3 position)
    {
        if (hitSound == null) return;
        
        // Create temporary AudioSource at hit position
        GameObject audioObject = new GameObject("HitSound");
        audioObject.transform.position = position;
        
        AudioSource audioSource = audioObject.AddComponent<AudioSource>();
        audioSource.clip = hitSound;
        audioSource.volume = hitSoundVolume;
        audioSource.pitch = Random.Range(pitchMin, pitchMax); // Random pitch for variety
        
        if (use3DSound)
        {
            audioSource.spatialBlend = 1f; // Full 3D sound
            audioSource.minDistance = 5f;
            audioSource.maxDistance = 50f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
        }
        else
        {
            audioSource.spatialBlend = 0f; // 2D sound
        }
        
        audioSource.Play();
        
        // Destroy after sound finishes
        Destroy(audioObject, hitSound.length + 0.1f);
    }
    
    void SpawnHitEffect(Vector3 position, Transform parent)
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
            
            // Optional: Rotate to face impact direction
            if (direction != Vector3.zero)
            {
                effect.transform.rotation = Quaternion.LookRotation(direction);
            }
            
            // Parent to hit object for movement (optional - disable if you want effect to stay in world space)
            // if (parent != null)
            // {
            //     effect.transform.SetParent(parent);
            // }
            
            // Auto-destroy effect
            Destroy(effect, hitEffectLifetime);
        }
    }
    
    void CreateSimpleHitEffect(Vector3 position)
    {
        // Create simple particle-like effect using GameObject
        GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        spark.transform.position = position;
        spark.transform.localScale = Vector3.one * 0.5f;
        
        // Set color
        Renderer renderer = spark.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(1f, 0.8f, 0.2f); // Orange/yellow
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", Color.yellow * 2f);
        }
        
        // Remove collider
        Collider col = spark.GetComponent<Collider>();
        if (col != null) Destroy(col);
        
        // Animate and destroy
        StartCoroutine(AnimateHitEffect(spark));
    }
    
    System.Collections.IEnumerator AnimateHitEffect(GameObject effect)
    {
        float elapsed = 0f;
        Vector3 startScale = effect.transform.localScale;
        
        while (elapsed < hitEffectLifetime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / hitEffectLifetime;
            
            // Scale up then fade
            effect.transform.localScale = startScale * (1f + progress * 2f);
            
            // Fade out
            Renderer renderer = effect.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color color = renderer.material.color;
                color.a = 1f - progress;
                renderer.material.color = color;
            }
            
            yield return null;
        }
        
        Destroy(effect);
    }

    void ReturnToPool()
    {
        // Disable trail before destroying
        if (trail != null)
        {
            trail.enabled = false;
        }

        Destroy(gameObject);
    }
}
