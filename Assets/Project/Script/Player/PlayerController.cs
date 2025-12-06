using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;

    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private TrailRenderer trailRenderer;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    // Input
    private Vector2 moveInput;

    // Components
    private Rigidbody rb;
    private Camera mainCam;

    // State
    private bool isGrounded;
    private bool isDashing = false;
    private float dashTimeLeft = 0f;
    private float nextDashTime = 0f;
    private Vector3 dashDirection;

    // Animation Hashes
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int DashHash = Animator.StringToHash("Dash");

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCam = Camera.main;

        // Try to get animator from component or children
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }

    void Update()
    {
        CheckGround();
        UpdateAnimation();
        
        if (isDashing)
        {
            PerformDash();
        }
    }

    void FixedUpdate()
    {
        // Skip normal movement during dash
        if (!isDashing)
        {
            ApplyMovement();
        }
    }

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    // Input System Callbacks
    public void OnMovement(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            Jump();
        }
    }

    public void OnSkill1(InputValue value)
    {
        if (value.isPressed)
        {
            StartDash();
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        
        if (animator != null)
        {
            animator.SetTrigger(JumpHash);
        }
    }

    void ApplyMovement()
    {
        // Camera-relative movement
        Vector3 camForward = mainCam.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = mainCam.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 moveDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        // Apply movement
        Vector3 movement = moveDirection * moveSpeed;
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);

        // Rotate character
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    void UpdateAnimation()
    {
        if (animator == null)
            return;

        animator.SetFloat(SpeedHash, moveInput.magnitude);
        animator.SetBool(IsGroundedHash, isGrounded);
    }

    /// <summary>
    /// Start dash movement
    /// </summary>
    void StartDash()
    {
        if (Time.time < nextDashTime)
        {
            return; // Cooldown
        }
        
        if (isDashing)
        {
            return; // Already dashing
        }
        
        // Calculate dash direction
        if (moveInput.magnitude < 0.1f)
        {
            // No input - dash forward
            dashDirection = transform.forward;
        }
        else
        {
            // Dash in movement direction (camera-relative)
            Vector3 camForward = mainCam.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();
            
            Vector3 camRight = mainCam.transform.right;
            camRight.y = 0f;
            camRight.Normalize();
            
            dashDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;
        }
        
        // Set dash state
        isDashing = true;
        dashTimeLeft = dashDuration;
        nextDashTime = Time.time + dashCooldown;
        
        // Trigger animation
        if (animator != null)
        {
            animator.SetTrigger(DashHash);
        }
        
        // Enable trail
        if (trailRenderer != null)
        {
            trailRenderer.emitting = true;
        }
    }
    
    /// <summary>
    /// Perform dash movement
    /// </summary>
    void PerformDash()
    {
        dashTimeLeft -= Time.deltaTime;
        
        if (dashTimeLeft <= 0f)
        {
            EndDash();
            return;
        }
        
        // Calculate dash speed and movement
        float dashSpeed = dashDistance / dashDuration;
        Vector3 movement = dashDirection * dashSpeed * Time.deltaTime;
        
        // Apply movement via Rigidbody
        rb.MovePosition(rb.position + movement);
    }
    
    /// <summary>
    /// End dash movement
    /// </summary>
    void EndDash()
    {
        isDashing = false;
        dashTimeLeft = 0f;
        
        // Disable trail
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }
    }

    public bool IsGrounded() => isGrounded;

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
#endif
}