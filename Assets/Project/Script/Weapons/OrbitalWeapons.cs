using UnityEngine;

public class OrbitalWeapons : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private int weaponCount = 6;
    [SerializeField] private bool showDebugInfo = true;

    [Header("Formation Settings")]
    [SerializeField] private float orbitRadius = 2.5f;
    [SerializeField] private float modeTransitionSpeed = 5f;
    [SerializeField] private Vector3[] weaponPositions = new Vector3[]
    {
        new Vector3(0, 2.5f, 1.5f),      // Atas depan
        new Vector3(-1.5f, 2f, 0.5f),    // Atas kiri
        new Vector3(1.5f, 2f, 0.5f),     // Atas kanan
        new Vector3(-2f, 1f, 0),         // Samping kiri
        new Vector3(2f, 1f, 0),          // Samping kanan
        new Vector3(0, 0.5f, -1f)        // Bawah belakang
    };

    [Header("Floating Animation")]
    [SerializeField] private bool enableFloating = true;
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private float floatAmountY = 0.2f; // Vertical float
    [SerializeField] private float floatAmountX = 0.1f; // Horizontal sway
    [SerializeField] private float rotationSpeed = 30f; // Slow rotation around own axis

    [Header("Visual Effects")]
    [SerializeField] private bool enableTrail = true;
    [SerializeField] private Color trailColor = new Color(0.3f, 0.7f, 1f, 0.5f);
    [SerializeField] private float trailTime = 0.5f;

    [Header("Attack System")]
    [SerializeField] private bool enableShooting = true;
    [SerializeField] private float shootDamage = 25f;
    [SerializeField] private float shootCooldown = 0.3f;
    [SerializeField] private float reloadTime = 2f;
    [SerializeField] private float targetDetectionRange = 30f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private bool autoReload = true;

    private GameObject[] weapons;
    private TrailRenderer[] trails;
    private float[] floatOffsets;
    private PlayerRole playerRole;

    // Shooting system
    private int availableWeapons;
    private float lastShootTime;
    private bool isReloading;
    private float reloadTimer;
    private Transform currentTarget;

    void Start()
    {
        playerRole = GetComponent<PlayerRole>();
        if (playerRole != null)
        {
            weaponCount = playerRole.GetActiveStats().weaponCount;
        }
        
        InitializeWeapons();
        
        // Initialize shooting system
        availableWeapons = weaponCount;
        lastShootTime = Time.time;
        isReloading = false;
    }

    void InitializeWeapons()
    {
        if (weaponPrefab == null)
        {
            return;
        }

        weapons = new GameObject[weaponCount];
        trails = new TrailRenderer[weaponCount];
        floatOffsets = new float[weaponCount];

        for (int i = 0; i < weaponCount; i++)
        {
            weapons[i] = Instantiate(weaponPrefab, transform);
            weapons[i].name = $"Orbital_Weapon_{i + 1}";
            
            WeaponProjectile projScript = weapons[i].GetComponent<WeaponProjectile>();
            if (projScript != null)
            {
                projScript.enabled = false;
            }
            
            floatOffsets[i] = Random.Range(0f, Mathf.PI * 2f);
            weapons[i].SetActive(true);

            if (enableTrail)
            {
                trails[i] = AddTrailEffect(weapons[i]);
            }
        }
    }

    void Update()
    {
        if (weapons == null || weapons.Length == 0) return;

        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0)
            {
                FinishReload();
            }
        }

        if (enableShooting)
        {
            FindNearestEnemy();
        }

        UpdateWeaponPositions();
        UpdateTrailColors();
    }
    
    void UpdateWeaponPositions()
    {
        if (weapons == null || weapons.Length == 0) return;

        for (int i = 0; i < weaponCount; i++)
        {
            if (weapons[i] == null || !weapons[i].activeSelf) continue;

            Vector3 position = CalculateWeaponPosition(i);
            Quaternion rotation = CalculateWeaponRotation(i);

            weapons[i].transform.localPosition = Vector3.Lerp(
                weapons[i].transform.localPosition, 
                position, 
                modeTransitionSpeed * Time.deltaTime
            );
            
            weapons[i].transform.localRotation = Quaternion.Slerp(
                weapons[i].transform.localRotation,
                rotation,
                modeTransitionSpeed * Time.deltaTime
            );
            
            if (showDebugInfo && Application.isPlaying)
            {
                Debug.DrawLine(transform.position, weapons[i].transform.position, Color.cyan);
            }
        }
    }

    Vector3 CalculateWeaponPosition(int index)
    {
        float angleStep = 360f / weaponCount;
        float angle = angleStep * index;
        float radian = angle * Mathf.Deg2Rad;
        
        Vector3 basePosition = new Vector3(
            Mathf.Cos(radian) * orbitRadius,
            0f,
            Mathf.Sin(radian) * orbitRadius
        );
        
        if (enableFloating)
        {
            float time = Time.time * floatSpeed;
            float offset = floatOffsets[index];
            
            float floatY = Mathf.Sin(time + offset) * floatAmountY;
            float swayX = Mathf.Cos(time * 0.7f + offset) * floatAmountX;
            
            basePosition += new Vector3(swayX, floatY, 0);
        }

        return basePosition;
    }

    Quaternion CalculateWeaponRotation(int index)
    {
        float angleStep = 360f / weaponCount;
        float angle = angleStep * index;
        
        Quaternion baseRotation = Quaternion.Euler(0, angle + 90f, 90f);
        
        if (enableFloating)
        {
            float subtleRotation = Mathf.Sin(Time.time * rotationSpeed * 0.05f) * 5f;
            baseRotation *= Quaternion.Euler(0, 0, subtleRotation);
        }
        
        return baseRotation;
    }

    void UpdateTrailColors()
    {
        if (!enableTrail || trails == null) return;

        foreach (TrailRenderer trail in trails)
        {
            if (trail != null)
            {
                trail.startColor = Color.Lerp(trail.startColor, trailColor, modeTransitionSpeed * Time.deltaTime);
                Color endColor = trailColor;
                endColor.a = 0f;
                trail.endColor = endColor;
            }
        }
    }

    TrailRenderer AddTrailEffect(GameObject weapon)
    {
        TrailRenderer trail = weapon.AddComponent<TrailRenderer>();
        trail.time = trailTime;
        trail.startWidth = 0.15f;
        trail.endWidth = 0.01f;

        Material trailMat = new Material(Shader.Find("Sprites/Default"));
        trail.material = trailMat;

        trail.startColor = trailColor;
        Color endColor = trailColor;
        endColor.a = 0f;
        trail.endColor = endColor;

        trail.numCornerVertices = 5;
        trail.numCapVertices = 5;
        trail.minVertexDistance = 0.1f;

        return trail;
    }

    void OnDestroy()
    {
    }

    public void OnAttack(UnityEngine.InputSystem.InputValue value)
    {        
        if (value.isPressed)
        {
            PerformRoleBasedAttack();
        }
    }

    public void PerformRoleBasedAttack()
    {
        if (playerRole == null)
        {
            TryShootWeapon();
            return;
        }
        
        RoleType currentRole = playerRole.GetCurrentRole();
        RoleStats stats = playerRole.GetActiveStats();
        
        if (stats == null)
        {
            TryShootWeapon();
            return;
        }
        
        switch (currentRole)
        {
            case RoleType.Striker:
                PerformStrikerAttack(stats);
                break;
            case RoleType.Sniper:
                PerformSniperAttack(stats);
                break;
            default:
                TryShootWeapon();
                break;
        }
    }
    
    void PerformStrikerAttack(RoleStats stats)
    {
        // Striker: Burst attack - shoot multiple weapons at once (close range power)
        if (isReloading || availableWeapons <= 0) return;
        
        int burstCount = Mathf.Min(3, availableWeapons); // Shoot up to 3 weapons at once
        
        for (int i = 0; i < burstCount; i++)
        {
            if (availableWeapons > 0)
            {
                ShootWeapon();
            }
        }
        
        Debug.Log($"<color=red>🗡️ STRIKER BURST ATTACK! Fired {burstCount} weapons simultaneously!</color>");
    }
    
    void PerformSniperAttack(RoleStats stats)
    {
        // Sniper: Charged precision shot with guaranteed critical
        if (isReloading || availableWeapons <= 0) return;
        
        // Find furthest enemy for long-range shot
        Transform furthestTarget = FindFurthestEnemy();
        if (furthestTarget != null)
        {
            currentTarget = furthestTarget;
        }
        
        ShootWeapon();
        
        Debug.Log($"<color=cyan>🎯 SNIPER PRECISION STRIKE! Long-range critical shot!</color>");
    }
    
    Transform FindFurthestEnemy()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, targetDetectionRange, enemyLayer);
        
        if (enemies.Length == 0) return null;
        
        float furthestDistance = 0f;
        Transform furthestEnemy = null;
        
        foreach (Collider enemy in enemies)
        {
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null && health.IsDead()) continue;
            
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance > furthestDistance)
            {
                furthestDistance = distance;
                furthestEnemy = enemy.transform;
            }
        }
        
        return furthestEnemy;
    }
    
    // Removed PerformEngineerAttack - Engineer role deleted
    // Removed PerformGhostAttack - Ghost role deleted
    // Only Striker and Sniper remain

    public void TryShootWeapon()
    {
        if (!enableShooting) return;
        if (isReloading) return;
        
        if (availableWeapons <= 0)
        {
            if (autoReload)
            {
                StartReload();
            }
            return;
        }
        
        if (Time.time < lastShootTime + shootCooldown)
        {
            return;
        }

        ShootWeapon();
    }

    void ShootWeapon()
    {
        // Find first available weapon
        int weaponIndex = -1;
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null && weapons[i].activeSelf)
            {
                weaponIndex = i;
                break;
            }
        }

        if (weaponIndex == -1) return;

        GameObject weapon = weapons[weaponIndex];
        
        // Determine target direction
        Vector3 shootDirection;
        Transform target = currentTarget;
        
        if (target != null)
        {
            shootDirection = (target.position - weapon.transform.position).normalized;
        }
        else
        {
            // Shoot forward if no target
            shootDirection = transform.forward;
        }

        // Create projectile
        GameObject projectile = Instantiate(weapon, weapon.transform.position, weapon.transform.rotation);
        projectile.transform.SetParent(null); // Unparent from player
        
        // Add/Get projectile script and ENABLE it
        WeaponProjectile projScript = projectile.GetComponent<WeaponProjectile>();
        if (projScript == null)
        {
            projScript = projectile.AddComponent<WeaponProjectile>();
        }
        projScript.enabled = true; // Enable script for projectile
        
        // Add collider if not exists
        if (projectile.GetComponent<Collider>() == null)
        {
            BoxCollider col = projectile.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(0.2f, 0.2f, 1f);
        }

        // Initialize projectile with role stats
        float finalDamage = shootDamage;
        if (playerRole != null)
        {
            finalDamage = playerRole.GetWeaponDamage();
        }
        
        projScript.Initialize(target, shootDirection, finalDamage, playerRole);

        weapon.SetActive(false);
        
        availableWeapons--;
        lastShootTime = Time.time;
    }

    void FindNearestEnemy()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, targetDetectionRange, enemyLayer);
        
        if (enemies.Length == 0)
        {
            currentTarget = null;
            return;
        }

        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (Collider enemy in enemies)
        {
            // Skip dead enemies
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null && health.IsDead()) continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        currentTarget = closestEnemy;
    }

    void StartReload()
    {
        if (isReloading) return;
        
        isReloading = true;
        reloadTimer = reloadTime;
    }

    void FinishReload()
    {
        isReloading = false;
        availableWeapons = weaponCount;
    }

    public int GetAvailableWeapons() => availableWeapons;
    public int GetMaxWeapons() => weaponCount;
    public bool IsReloading() => isReloading;
    public float GetReloadProgress() => isReloading ? (1f - (reloadTimer / reloadTime)) : 1f;

    void CheckAndReload()
    {
        if (autoReload && availableWeapons <= 0 && !isReloading)
        {
            StartReload();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 1.5f, orbitRadius);
    }
}