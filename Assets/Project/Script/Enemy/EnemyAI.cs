using UnityEngine;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float attackRadius = 3.5f;
    [SerializeField] private float fieldOfView = 90f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRangeBuffer = 1.2f;

    [Header("Alert Settings")]
    [SerializeField] private GameObject alertIcon;
    [SerializeField] private float alertDuration = 0.5f;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;

    // State Management
    private enum EnemyState { Idle, Patrol, Alert, Chase, Attack, Hit, Die, Victory, Taunt, Sense }
    private EnemyState currentState = EnemyState.Idle;
    private EnemyState previousState = EnemyState.Idle;

    // Timers
    private float lastAttackTime;
    private float alertTimer;
    private bool isInAlertState;
    private bool hasAttackedInThisState = false;
    private float tauntTimer;
    private float hitStunTimer;
    
    // Animation States
    private bool isDead = false;
    private bool isInHitStun = false;

    // Cached Animator Parameters (from your Animator Controller)
    private static readonly int IdleNormalHash = Animator.StringToHash("IdleNormal");
    private static readonly int IdleBattleHash = Animator.StringToHash("IdleBattle");
    private static readonly int WalkFwdHash = Animator.StringToHash("WalkFWD");
    private static readonly int WalkBwdHash = Animator.StringToHash("WalkBWD");
    private static readonly int WalkLeftHash = Animator.StringToHash("WalkLeft");
    private static readonly int WalkRightHash = Animator.StringToHash("WalkRight");
    private static readonly int RunFwdHash = Animator.StringToHash("RunFWD");
    private static readonly int Attack01Hash = Animator.StringToHash("Attack01");
    private static readonly int Attack02Hash = Animator.StringToHash("Attack02");
    private static readonly int SenseSomethingHash = Animator.StringToHash("SenseSomethingST");
    private static readonly int TauntHash = Animator.StringToHash("Taunt");
    private static readonly int GetHitHash = Animator.StringToHash("GetHit");
    private static readonly int DieHash = Animator.StringToHash("Die");
    private static readonly int DizzyHash = Animator.StringToHash("Dizzy");
    private static readonly int VictoryHash = Animator.StringToHash("Victory");

    private Vector3 startPosition;
    private bool playerDetected;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (alertIcon != null)
            alertIcon.SetActive(false);

        startPosition = transform.position;
        lastAttackTime = -attackCooldown;
    }

    void Update()
    {
        if (player == null || isDead) return;
        
        // Handle hit stun
        if (isInHitStun)
        {
            hitStunTimer -= Time.deltaTime;
            if (hitStunTimer <= 0)
            {
                isInHitStun = false;
            }
            return;
        }

        previousState = currentState;

        UpdateState();

        if (currentState == EnemyState.Attack && previousState != EnemyState.Attack)
        {
            hasAttackedInThisState = false;
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdle();
                break;
            case EnemyState.Patrol:
                HandlePatrol();
                break;
            case EnemyState.Alert:
                HandleAlert();
                break;
            case EnemyState.Chase:
                HandleChase();
                break;
            case EnemyState.Attack:
                HandleAttack();
                break;
            case EnemyState.Taunt:
                HandleTaunt();
                break;
        }

        UpdateAnimator();
    }

    void UpdateState()
    {
        if (isInAlertState)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRadius)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            bool inFieldOfView = playerDetected || angleToPlayer <= fieldOfView / 2f;

            if (inFieldOfView)
            {
                if (!Physics.Raycast(transform.position + Vector3.up, directionToPlayer, distanceToPlayer, obstacleLayer))
                {
                    if (!playerDetected)
                    {
                        playerDetected = true;
                        currentState = EnemyState.Alert;
                        isInAlertState = true;
                        ShowAlert();
                        return;
                    }

                    if (distanceToPlayer <= attackRadius)
                    {
                        currentState = EnemyState.Attack;
                    }
                    else if (distanceToPlayer > attackRadius * attackRangeBuffer)
                    {
                        currentState = EnemyState.Chase;
                    }

                    return;
                }
            }
        }

        if (playerDetected && distanceToPlayer > detectionRadius * 1.2f)
        {
            playerDetected = false;
            currentState = EnemyState.Idle;
        }
    }

    void HandleIdle()
    {
        if (Vector3.Distance(transform.position, startPosition) > 0.5f)
        {
            MoveTowards(startPosition, moveSpeed);
        }
    }

    void HandlePatrol()
    {
        HandleIdle();
    }

    void HandleAlert()
    {
        LookAtPlayer();

        alertTimer += Time.deltaTime;
        if (alertTimer >= alertDuration)
        {
            alertTimer = 0f;
            isInAlertState = false;
            
            // Random chance to taunt after detecting player
            if (Random.value < 0.3f) // 30% chance
            {
                currentState = EnemyState.Taunt;
                tauntTimer = 1.5f;
            }
            else
            {
                currentState = EnemyState.Chase;
            }
            HideAlert();
        }
    }

    void HandleChase()
    {
        MoveTowards(player.position, chaseSpeed);
        LookAtPlayer();
    }
    
    void HandleTaunt()
    {
        LookAtPlayer();
        
        tauntTimer -= Time.deltaTime;
        if (tauntTimer <= 0)
        {
            currentState = EnemyState.Chase;
        }
    }

    void HandleAttack()
    {
        LookAtPlayer();

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRadius * attackRangeBuffer)
        {
            hasAttackedInThisState = false;
            currentState = EnemyState.Chase;
            return;
        }

        if (distanceToPlayer > attackRadius * 0.7f)
        {
            MoveTowards(player.position, moveSpeed * 0.3f);
        }

        if (!hasAttackedInThisState)
        {
            PerformAttack();
            lastAttackTime = Time.time;
            hasAttackedInThisState = true;
            return;
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
    }

    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            transform.position += direction * speed * Time.deltaTime;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void PerformAttack()
    {
        // Random attack animation (Attack01 or Attack02)
        if (Random.value < 0.5f)
            animator.SetTrigger(Attack01Hash);
        else
            animator.SetTrigger(Attack02Hash);

        // Verify player is still in attack range before dealing damage
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > attackRadius)
        {
            Debug.Log($"⚔️ {gameObject.name} attacked but player is too far ({distanceToPlayer:F2}m > {attackRadius}m)");
            return;
        }

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            Debug.Log($"⚔️ {gameObject.name} dealing {attackDamage} damage to player at distance {distanceToPlayer:F2}m");
            playerHealth.TakeDamage(attackDamage);
        }
        else
        {
            Debug.LogWarning($"⚠️ {gameObject.name} cannot find PlayerHealth component!");
        }
    }

    void ShowAlert()
    {
        if (alertIcon != null)
            alertIcon.SetActive(true);
    }

    void HideAlert()
    {
        if (alertIcon != null)
            alertIcon.SetActive(false);
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        // Play appropriate animation based on state
        switch (currentState)
        {
            case EnemyState.Idle:
                if (!playerDetected)
                    PlayAnimation(IdleNormalHash);
                else
                    PlayAnimation(IdleBattleHash);
                break;
                
            case EnemyState.Patrol:
                PlayAnimation(WalkFwdHash);
                break;
                
            case EnemyState.Alert:
                PlayAnimation(SenseSomethingHash);
                break;
                
            case EnemyState.Chase:
                PlayAnimation(RunFwdHash);
                break;
                
            case EnemyState.Attack:
                PlayAnimation(IdleBattleHash); // Idle between attacks
                break;
                
            case EnemyState.Taunt:
                PlayAnimation(TauntHash);
                break;
        }
    }
    
    void PlayAnimation(int animHash)
    {
        if (HasParameter(animator, animHash))
        {
            animator.SetTrigger(animHash);
        }
    }
    
    // Public method untuk dipanggil dari EnemyHealth saat kena hit
    public void PlayHitReaction()
    {
        if (isDead || animator == null) return;
        
        animator.SetTrigger(GetHitHash);
        isInHitStun = true;
        hitStunTimer = 0.3f; // 0.3 second stun
    }
    
    // Public method untuk dipanggil dari EnemyHealth saat mati
    public void PlayDeathAnimation()
    {
        if (isDead) return;
        
        isDead = true;
        if (animator != null)
        {
            animator.SetTrigger(DieHash);
        }
        
        // Disable AI and collisions
        enabled = false;
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    private bool HasParameter(Animator anim, int paramHash)
    {
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.nameHash == paramHash)
                return true;
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attackRadius * attackRangeBuffer);

        Gizmos.color = Color.green;
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2f, 0) * transform.forward * detectionRadius;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2f, 0) * transform.forward * detectionRadius;

        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        if (Application.isPlaying && player != null && playerDetected)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position + Vector3.up, player.position + Vector3.up);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f,
                $"State: {currentState}\nDist: {Vector3.Distance(transform.position, player.position):F2}");
#endif
        }
    }
}