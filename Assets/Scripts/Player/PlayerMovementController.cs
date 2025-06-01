using UnityEngine;

/// <summary>
/// Handles player movement, animations, and ground detection.
/// Refactored from PlayerController to work with the new Player architecture.
/// </summary>
public class PlayerMovementController : PlayerControllerBase
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private VirtualJoystick joystick;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayerMask = 1;
    
    [Header("Animation")]
    [SerializeField] private float animationDeadzone = 0.1f;
    
    private Rigidbody2D rb;
    private Animator animator;
    
    // Animation parameter names
    private const string ANIM_IS_WALKING = "isWalking";
    private const string ANIM_IS_GROUNDED = "isGrounded";
    private const string ANIM_VERTICAL_VELOCITY = "verticalVelocity";
    private const string ANIM_FACING_RIGHT = "facingRight";
    private const string ANIM_HORIZONTAL_SPEED = "horizontalSpeed";
    
    private bool isGrounded;
    private bool isMovingHorizontally;
    private bool facingRight = true;
    
    // Public properties for Player and other systems to access
    public bool IsGrounded => isGrounded;
    public bool IsMovingHorizontally => isMovingHorizontally;
    public bool FacingRight => facingRight;
    public float CurrentHorizontalSpeed => rb != null ? rb.velocity.x : 0f;
    public float CurrentVerticalSpeed => rb != null ? rb.velocity.y : 0f;
    public Vector2 Velocity => rb != null ? rb.velocity : Vector2.zero;
    
    protected override void OnInitialize()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        if (rb == null)
        {
            Debug.LogError("PlayerMovementController: No Rigidbody2D component found!");
        }
        
        if (animator == null)
        {
            Debug.LogError("PlayerMovementController: No Animator component found! Animation-based flipping requires an Animator!");
        }
        
        if (joystick == null)
        {
            Debug.LogWarning("PlayerMovementController: No VirtualJoystick assigned. Player movement may not work.");
        }
        
        if (groundCheck == null)
        {
            Debug.LogWarning("PlayerMovementController: No ground check transform assigned. Ground detection will not work.");
        }
    }
    
    void FixedUpdate()
    {
        if (!IsReady) return;
        
        CheckGrounded();
        HandleAnimations();
        HandleMovement();
    }
    
    private void CheckGrounded()
    {
        if (groundCheck == null) return;
        
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);
    }
    
    private void HandleMovement()
    {
        if (joystick == null) return;
        
        Vector2 moveDirection = joystick.InputDirection;
        
        // Handle horizontal movement
        float horizontalInput = moveDirection.x;
        const int PPU = 32; // Pixels Per Unit for pixel-perfect movement
        
        if (rb != null)
        {
            rb.position = new Vector2(
                Mathf.Round(rb.position.x * PPU) / PPU,
                rb.position.y);
            rb.velocity = new Vector2(
                Mathf.Round(horizontalInput * moveSpeed) / PPU,
                rb.velocity.y);
        }
        
        // Store movement state for animations
        isMovingHorizontally = Mathf.Abs(horizontalInput) > animationDeadzone;
        
        // Update facing direction based on input
        UpdateFacingDirection(horizontalInput);
    }
    
    private void UpdateFacingDirection(float horizontalInput)
    {
        // Only change direction when there's significant input
        if (horizontalInput > animationDeadzone)
        {
            facingRight = true;
        }
        else if (horizontalInput < -animationDeadzone)
        {
            facingRight = false;
        }
        // Don't change direction when input is in deadzone
    }
    
    private void HandleAnimations()
    {
        if (animator == null) return;
        
        animator.SetBool(ANIM_IS_WALKING, isMovingHorizontally && isGrounded);
        animator.SetBool(ANIM_IS_GROUNDED, isGrounded);
        animator.SetFloat(ANIM_VERTICAL_VELOCITY, rb != null ? rb.velocity.y : 0f);
        animator.SetBool(ANIM_FACING_RIGHT, facingRight);
        
        float signedHorizontalSpeed = facingRight ? Mathf.Abs(CurrentHorizontalSpeed) : -Mathf.Abs(CurrentHorizontalSpeed);
        animator.SetFloat(ANIM_HORIZONTAL_SPEED, signedHorizontalSpeed);
    }
    
    /// <summary>
    /// Trigger attack animation.
    /// Called by Player class when attacking.
    /// </summary>
    public void TriggerAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("attack");
        }
    }
    
    /// <summary>
    /// Trigger hurt animation.
    /// Called by Player class when taking damage.
    /// </summary>
    public void TriggerHurt()
    {
        if (animator != null)
        {
            animator.SetTrigger("hurt");
        }
    }
    
    /// <summary>
    /// Set movement speed (useful for power-ups, debuffs, etc.)
    /// </summary>
    /// <param name="newSpeed">New movement speed</param>
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = Mathf.Max(0f, newSpeed);
    }
    
    /// <summary>
    /// Get current movement speed
    /// </summary>
    public float GetMoveSpeed()
    {
        return moveSpeed;
    }
    
    // Draw ground check gizmo for debugging
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
} 