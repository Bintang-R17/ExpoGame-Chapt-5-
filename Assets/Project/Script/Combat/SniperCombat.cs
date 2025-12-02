using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class SniperCombat : MonoBehaviour, IRoleCombat
{
    [Header("Precision Shot Settings")]
    [SerializeField] private float precisionShotDamage = 80f;
    [SerializeField] private float precisionShotRange = 50f;
    [SerializeField] private float precisionShotCooldown = 1.5f;
    [SerializeField] private LayerMask enemyLayer;
    private float lastPrecisionShotTime;
    
    [Header("Skill 1 - Piercing Shot")]
    [SerializeField] private float piercingShotDamage = 200f;
    [SerializeField] private float piercingShotRange = 100f;
    [SerializeField] private float piercingShotCooldown = 6f;
    [SerializeField] private int piercingShotMaxTargets = 5;
    [SerializeField] private float piercingShotChargeTime = 1f;
    private float lastPiercingShotTime;
    private bool isChargingShot;
    
    [Header("Skill 2 - Tactical Retreat")]
    [SerializeField] private float retreatDistance = 10f;
    [SerializeField] private float retreatCooldown = 8f;
    [SerializeField] private int smokeBombCount = 3;
    private float lastRetreatTime;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private GameObject piercingShotBeamPrefab;
    [SerializeField] private GameObject smokeBombPrefab;
    [SerializeField] private Color sniperTrailColor = new Color(0.3f, 0.7f, 1f);
    [SerializeField] private LineRenderer aimLaser;
    
    [Header("Audio")]
    [SerializeField] private AudioClip sniperShotSound;
    [SerializeField] private AudioClip piercingShotSound;
    [SerializeField] private AudioClip smokeBombSound;
    private AudioSource audioSource;
    
    [Header("References")]
    private PlayerRole playerRole;
    private Animator animator;
    private PlayerController playerController;
    private Camera playerCamera;
    private GameObject owner;
    [Header("AutoTargeting")]
    [SerializeField] private SniperAutoTargeting autoTargeting;
    
    // IRoleCombat implementation
    public RoleType GetRoleType() => RoleType.Sniper;
    
    public void Initialize(GameObject ownerObject)
    {
        owner = ownerObject;
        enabled = false; // Start disabled, RoleCombatManager will enable when needed
    }
    
    public void Cleanup()
    {
        if (aimLaser != null)
        {
            aimLaser.enabled = false;
        }
    }
    
    void Start()
    {
        playerRole = GetComponent<PlayerRole>();
        animator = GetComponentInChildren<Animator>();
        playerController = GetComponent<PlayerController>();
        audioSource = GetComponent<AudioSource>();
        playerCamera = Camera.main;
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Setup auto-targeting if not assigned
        if (autoTargeting == null)
        {
            autoTargeting = GetComponent<SniperAutoTargeting>();
            if (autoTargeting == null)
            {
                autoTargeting = gameObject.AddComponent<SniperAutoTargeting>();
                Debug.Log("🎯 SniperAutoTargeting component auto-created");
            }
        }
        
        // Set initial cooldowns
        lastPrecisionShotTime = -precisionShotCooldown;
        lastPiercingShotTime = -piercingShotCooldown;
        lastRetreatTime = -retreatCooldown;
        
        // Setup aim laser
        SetupAimLaser();
    }

    void OnEnable()
    {
        if (autoTargeting != null)
        {
            autoTargeting.Activate();
        }
    }

    void OnDisable()
    {
        // Deactivate auto-targeting if present
        if (autoTargeting != null)
        {
            autoTargeting.Deactivate();
        }

        // Hide aim laser when disabled
        if (aimLaser != null)
        {
            aimLaser.enabled = false;
        }
    }
    
    void SetupAimLaser()
    {
        if (aimLaser == null)
        {
            GameObject laserObj = new GameObject("SniperAimLaser");
            laserObj.transform.SetParent(transform);
            aimLaser = laserObj.AddComponent<LineRenderer>();
            
            aimLaser.startWidth = 0.02f;
            aimLaser.endWidth = 0.01f;
            aimLaser.material = new Material(Shader.Find("Sprites/Default"));
            aimLaser.startColor = sniperTrailColor;
            aimLaser.endColor = sniperTrailColor * 0.5f;
            aimLaser.positionCount = 2;
            aimLaser.enabled = false;
        }
    }
    
    void Update()
    {
        if (enabled)
        {
            UpdateAimLaser();
        }
    }
    
    void UpdateAimLaser()
    {
        if (aimLaser == null) return;
        
        // Show aim laser when aiming
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 direction = transform.forward;
        
        if (Physics.Raycast(origin, direction, out RaycastHit hit, precisionShotRange))
        {
            aimLaser.SetPosition(0, origin);
            aimLaser.SetPosition(1, hit.point);
            aimLaser.enabled = true;
        }
        else
        {
            aimLaser.SetPosition(0, origin);
            aimLaser.SetPosition(1, origin + direction * precisionShotRange);
            aimLaser.enabled = true;
        }
    }
    
    
    // IRoleCombat interface methods - Renamed to prevent Unity SendMessage conflicts
    public void HandleBasicAttack(InputAction.CallbackContext context)
    {
        Debug.Log("🎯 SNIPER HandleBasicAttack called!");
        TryPrecisionShot();
    }
    
    public void HandleSkill1(InputAction.CallbackContext context)
    {
        TryPiercingShot();
    }
    
    public void HandleSkill2(InputAction.CallbackContext context)
    {
        TryTacticalRetreat();
    }
    
    void TryPrecisionShot()
    {
        if (isChargingShot) return;
        
        if (Time.time - lastPrecisionShotTime < precisionShotCooldown)
        {
            Debug.Log($"⏳ Precision shot on cooldown! {(precisionShotCooldown - (Time.time - lastPrecisionShotTime)):F1}s remaining");
            return;
        }
        
        PerformPrecisionShot();
    }
    
    void PerformPrecisionShot()
    {
        lastPrecisionShotTime = Time.time;
        
        Debug.Log("🎯 SNIPER: Precision shot!");
        
        // Play animation
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }
        
        // Play sound
        if (sniperShotSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(sniperShotSound);
        }
        
        // Muzzle flash
        if (muzzleFlashPrefab != null)
        {
            Vector3 muzzlePos = transform.position + transform.forward * 1f + Vector3.up * 1.5f;
            GameObject flash = Instantiate(muzzleFlashPrefab, muzzlePos, transform.rotation);
            Destroy(flash, 0.5f);
        }
        
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 direction = transform.forward;

        // If auto-targeting system has a target, use it to apply damage and visuals
        if (autoTargeting != null && autoTargeting.HasTarget)
        {
            if (autoTargeting.TryShoot(out Transform target, out float damageMultiplier))
            {
                if (target != null)
                {
                    EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
                    if (enemyHealth != null)
                    {
                        float totalDamage = GetTotalDamage(precisionShotDamage) * damageMultiplier;

                        // Check for critical hit (still possible)
                        if (playerRole != null && playerRole.HasCritical())
                        {
                            if (Random.value < playerRole.GetCritChance())
                            {
                                totalDamage *= playerRole.GetCritMultiplier();
                                StartCoroutine(ShowCriticalHitEffect(target.position));
                            }
                        }

                        enemyHealth.TakeDamage(totalDamage);
                        Debug.Log($"🎯 Sniper hit {target.name} for {totalDamage} damage (mult x{damageMultiplier})!");
                        StartCoroutine(ShowImpactEffect(target.position));
                        StartCoroutine(ShowBulletTrail(origin, target.position));
                    }
                }
                else
                {
                    Debug.Log("🎯 Auto-target returned null target.");
                }
            }
            else
            {
                Debug.Log("🎯 Auto-target couldn't prepare shot, fallback to raycast.");
                // Fallback to raycast below
                if (Physics.Raycast(origin, direction, out RaycastHit hit, precisionShotRange, enemyLayer))
                {
                    ProcessRaycastHit(hit);
                }
                else
                {
                    Debug.Log("🎯 Sniper shot missed!");
                }
            }
        }
        else
        {
            // Fallback to original raycast behavior
            if (Physics.Raycast(origin, direction, out RaycastHit hit, precisionShotRange, enemyLayer))
            {
                ProcessRaycastHit(hit);
            }
            else
            {
                Debug.Log("🎯 Sniper shot missed!");
            }
        }
        
        // Show bullet trail
        StartCoroutine(ShowBulletTrail(origin, origin + direction * precisionShotRange));
    }

    void ProcessRaycastHit(RaycastHit hit)
    {
        Debug.Log($"🎯 Hit: {hit.collider.name} at distance {hit.distance}m");
        EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            float totalDamage = GetTotalDamage(precisionShotDamage);

            // Check for critical hit
            if (playerRole != null && playerRole.HasCritical())
            {
                if (Random.value < playerRole.GetCritChance())
                {
                    totalDamage *= playerRole.GetCritMultiplier();
                    Debug.Log($"💥 CRITICAL HIT! {totalDamage} damage!");
                    StartCoroutine(ShowCriticalHitEffect(hit.point));
                }
            }

            enemyHealth.TakeDamage(totalDamage);
            Debug.Log($"🎯 Sniper hit {hit.collider.name} for {totalDamage} damage!");
            StartCoroutine(ShowImpactEffect(hit.point));
        }
    }
    
    void TryPiercingShot()
    {
        if (isChargingShot)
        {
            Debug.Log("⏳ Already charging shot!");
            return;
        }
        
        if (Time.time - lastPiercingShotTime < piercingShotCooldown)
        {
            Debug.Log($"⏳ Piercing shot on cooldown! {(piercingShotCooldown - (Time.time - lastPiercingShotTime)):F1}s remaining");
            return;
        }
        
        StartCoroutine(PerformPiercingShot());
    }
    
    IEnumerator PerformPiercingShot()
    {
        isChargingShot = true;
        lastPiercingShotTime = Time.time;
        
        Debug.Log("⚡ SNIPER: Charging piercing shot...");
        
        // Lock movement during charge
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Charging visual
        StartCoroutine(ShowChargingEffect());
        
        // Wait for charge
        yield return new WaitForSeconds(piercingShotChargeTime);
        
        // Execute piercing shot
        ExecutePiercingShot();
        
        // Re-enable movement
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        isChargingShot = false;
    }
    
    void ExecutePiercingShot()
    {
        Debug.Log("💥 PIERCING SHOT FIRED!");
        
        // Play sound
        if (piercingShotSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(piercingShotSound);
        }
        
        // Spawn beam VFX
        if (piercingShotBeamPrefab != null)
        {
            Vector3 beamStart = transform.position + Vector3.up * 1.5f;
            GameObject beam = Instantiate(piercingShotBeamPrefab, beamStart, transform.rotation);
            Destroy(beam, 2f);
        }
        
        // Raycast and hit multiple enemies
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 direction = transform.forward;
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, piercingShotRange, enemyLayer);
        
        int hitCount = 0;
        float totalDamage = GetTotalDamage(piercingShotDamage);
        
        // Always critical
        if (playerRole != null && playerRole.HasCritical())
        {
            totalDamage *= playerRole.GetCritMultiplier();
        }
        
        Debug.Log($"⚡ Piercing shot hit {hits.Length} targets!");
        
        foreach (RaycastHit hit in hits)
        {
            if (hitCount >= piercingShotMaxTargets) break;
            
            EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(totalDamage);
                Debug.Log($"💥 Piercing shot hit {hit.collider.name} for {totalDamage} damage!");
                
                StartCoroutine(ShowImpactEffect(hit.point));
                hitCount++;
            }
        }
        
        // Show massive beam
        StartCoroutine(ShowPiercingBeam(origin, origin + direction * piercingShotRange));
    }
    
    void TryTacticalRetreat()
    {
        if (Time.time - lastRetreatTime < retreatCooldown)
        {
            Debug.Log($"⏳ Tactical retreat on cooldown! {(retreatCooldown - (Time.time - lastRetreatTime)):F1}s remaining");
            return;
        }
        
        PerformTacticalRetreat();
    }
    
    void PerformTacticalRetreat()
    {
        lastRetreatTime = Time.time;
        
        Debug.Log("💨 SNIPER: Tactical retreat!");
        
        // Play sound
        if (smokeBombSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(smokeBombSound);
        }
        
        // Drop smoke bombs
        Vector3 currentPos = transform.position;
        for (int i = 0; i < smokeBombCount; i++)
        {
            Vector3 smokePos = currentPos + new Vector3(
                Random.Range(-2f, 2f),
                0.5f,
                Random.Range(-2f, 2f)
            );
            
            if (smokeBombPrefab != null)
            {
                GameObject smoke = Instantiate(smokeBombPrefab, smokePos, Quaternion.identity);
                Destroy(smoke, 5f);
            }
            else
            {
                // Create simple smoke sphere
                StartCoroutine(ShowSmokeCloud(smokePos));
            }
        }
        
        // Dash backwards
        Vector3 retreatDirection = -transform.forward;
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.Move(retreatDirection * retreatDistance);
        }
        else
        {
            transform.position += retreatDirection * retreatDistance;
        }
        
        Debug.Log($"💨 Retreated {retreatDistance}m backwards!");
    }
    
    // Visual effects
    IEnumerator ShowChargingEffect()
    {
        GameObject chargeIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        chargeIndicator.transform.position = transform.position + transform.forward * 2f + Vector3.up * 1.5f;
        chargeIndicator.transform.localScale = Vector3.one * 0.5f;
        Destroy(chargeIndicator.GetComponent<Collider>());
        
        Renderer rend = chargeIndicator.GetComponent<Renderer>();
        rend.material.color = sniperTrailColor;
        
        float elapsed = 0f;
        while (elapsed < piercingShotChargeTime)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(0.5f, 2f, elapsed / piercingShotChargeTime);
            chargeIndicator.transform.localScale = Vector3.one * scale;
            chargeIndicator.transform.position = transform.position + transform.forward * 2f + Vector3.up * 1.5f;
            
            // Pulse effect
            float intensity = Mathf.Lerp(1f, 3f, Mathf.PingPong(elapsed * 5f, 1f));
            rend.material.color = sniperTrailColor * intensity;
            
            yield return null;
        }
        
        Destroy(chargeIndicator);
    }
    
    IEnumerator ShowBulletTrail(Vector3 start, Vector3 end)
    {
        LineRenderer trail = new GameObject("BulletTrail").AddComponent<LineRenderer>();
        trail.startWidth = 0.05f;
        trail.endWidth = 0.02f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = sniperTrailColor;
        trail.endColor = sniperTrailColor * 0.5f;
        trail.positionCount = 2;
        trail.SetPosition(0, start);
        trail.SetPosition(1, end);
        
        yield return new WaitForSeconds(0.1f);
        Destroy(trail.gameObject);
    }
    
    IEnumerator ShowPiercingBeam(Vector3 start, Vector3 end)
    {
        LineRenderer beam = new GameObject("PiercingBeam").AddComponent<LineRenderer>();
        beam.startWidth = 0.2f;
        beam.endWidth = 0.1f;
        beam.material = new Material(Shader.Find("Sprites/Default"));
        beam.startColor = sniperTrailColor * 2f;
        beam.endColor = sniperTrailColor;
        beam.positionCount = 2;
        beam.SetPosition(0, start);
        beam.SetPosition(1, end);
        
        yield return new WaitForSeconds(0.3f);
        Destroy(beam.gameObject);
    }
    
    IEnumerator ShowImpactEffect(Vector3 position)
    {
        GameObject impact = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        impact.transform.position = position;
        impact.transform.localScale = Vector3.one * 0.5f;
        Destroy(impact.GetComponent<Collider>());
        
        Renderer rend = impact.GetComponent<Renderer>();
        rend.material.color = Color.yellow;
        
        yield return new WaitForSeconds(0.2f);
        Destroy(impact);
    }
    
    IEnumerator ShowCriticalHitEffect(Vector3 position)
    {
        GameObject crit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crit.transform.position = position;
        crit.transform.localScale = Vector3.one * 1.5f;
        Destroy(crit.GetComponent<Collider>());
        
        Renderer rend = crit.GetComponent<Renderer>();
        rend.material.color = Color.red;
        
        yield return new WaitForSeconds(0.3f);
        Destroy(crit);
    }
    
    IEnumerator ShowSmokeCloud(Vector3 position)
    {
        GameObject smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        smoke.transform.position = position;
        smoke.transform.localScale = Vector3.one * 2f;
        Destroy(smoke.GetComponent<Collider>());
        
        Renderer rend = smoke.GetComponent<Renderer>();
        rend.material.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        
        yield return new WaitForSeconds(5f);
        Destroy(smoke);
    }
    
    float GetTotalDamage(float baseDamage)
    {
        float totalDamage = baseDamage;
        
        if (playerRole != null)
        {
            totalDamage += playerRole.GetTotalWeaponDamage();
        }
        
        return totalDamage;
    }
    
    // Public methods for UI/debugging
    public float GetPrecisionShotCooldownRemaining()
    {
        return Mathf.Max(0, precisionShotCooldown - (Time.time - lastPrecisionShotTime));
    }
    
    public float GetPiercingShotCooldownRemaining()
    {
        return Mathf.Max(0, piercingShotCooldown - (Time.time - lastPiercingShotTime));
    }
    
    public float GetRetreatCooldownRemaining()
    {
        return Mathf.Max(0, retreatCooldown - (Time.time - lastRetreatTime));
    }
    
    void OnDrawGizmosSelected()
    {
        // Show precision shot range
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, transform.forward * precisionShotRange);
        
        // Show piercing shot range
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, transform.forward * piercingShotRange);
    }
}
