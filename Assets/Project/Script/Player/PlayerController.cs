using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float jumpDelay = 0.75f; // Delay sebelum apply force
    [SerializeField] private bool useRootMotionForJump = true; // Toggle root motion untuk lompat
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.3f;
    [SerializeField] private LayerMask groundMask;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    // Input
    private Vector2 moveInput;

    // Components
    private Rigidbody rb;
    private Camera mainCam;

    // State
    private bool isGrounded;
    private bool isJumping; // Flag untuk prevent double jump during animation

    // Animation Hashes
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpHash = Animator.StringToHash("Jump");

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

        // Disable root motion by default (hanya enable saat lompat jika useRootMotionForJump = true)
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }
    }

    void Update()
    {
        CheckGround();
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        ApplyMovement();
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
        if (value.isPressed && isGrounded && !isJumping)
        {
            StartCoroutine(JumpWithDelay());
        }
    }

    System.Collections.IEnumerator JumpWithDelay()
    {
        isJumping = true;
        
        // Enable root motion untuk lompat jika diaktifkan
        if (useRootMotionForJump && animator != null)
        {
            animator.applyRootMotion = true;
        }
        
        // Trigger animation first
        if (animator != null)
        {
            animator.SetTrigger(JumpHash);
        }
        
        // Wait for animation to reach jump point
        yield return new WaitForSeconds(jumpDelay);
        
        // Apply physics force (vertical boost)
        // Jika pakai root motion, horizontal movement sudah dari animasi
        if (useRootMotionForJump)
        {
            // Hanya tambah sedikit boost vertical jika perlu
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce * 0.5f, ForceMode.Impulse); // Kurangi force karena animasi sudah handle
        }
        else
        {
            // Full physics jump seperti sebelumnya
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        
        // Reset jump flag after landing
        yield return new WaitUntil(() => isGrounded);
        isJumping = false;
        
        // Disable root motion setelah landing
        if (useRootMotionForJump && animator != null)
        {
            animator.applyRootMotion = false;
        }
    }

    void Jump()
    {
        // Legacy method - now just calls coroutine
        if (!isJumping)
        {
            StartCoroutine(JumpWithDelay());
        }
    }

    void ApplyMovement()
    {
        // Calculate camera-relative direction
        Vector3 camForward = mainCam.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = mainCam.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 moveDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        // Move character
        if (moveDirection.magnitude > 0.1f)
        {
            Vector3 movement = moveDirection * moveSpeed;
            rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);

            // Rotate character
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    void UpdateAnimation()
    {
        // Re-get animator in case it changed (for role switching)
        if (animator == null || !animator.gameObject.activeInHierarchy)
            animator = GetComponentInChildren<Animator>();
        
        if (animator == null)
            return;

        animator.SetFloat(SpeedHash, moveInput.magnitude);
        animator.SetBool(IsGroundedHash, isGrounded);
    }
    
    // Set animator (dipanggil dari PlayerRole saat ganti character model)
    public void SetAnimator(Animator newAnimator)
    {
        animator = newAnimator;
    }

    // Public methods for role system
    public void SetMaxSpeed(float speed)
    {
        moveSpeed = speed;
    }
    
    public void SetJumpForce(float force)
    {
        jumpForce = force;
    }
    
    public float GetMaxSpeed() => moveSpeed;
    public float GetJumpForce() => jumpForce;
    
    // Public jump method - dapat dipanggil dari script lain
    public void PerformJump()
    {
        if (isGrounded)
        {
            Jump();
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