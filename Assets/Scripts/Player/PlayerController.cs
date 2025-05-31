using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
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
    private const string ANIM_FACING_RIGHT = "facingRight"; // New parameter for flipping
    private const string ANIM_HORIZONTAL_SPEED = "horizontalSpeed"; // Alternative approach
    
    private bool isGrounded;
    private bool isMovingHorizontally;
    private bool facingRight = true;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError("No Animator component found on " + gameObject.name + ". Animator-based flipping requires an Animator!");
        }
    }
    
    void Update()
    {
        CheckGrounded();
        HandleMovement();
        HandleAnimations();
    }
    
    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);
    }
    
    private void HandleMovement()
    {
        Vector2 moveDirection = joystick.InputDirection;
        
        // Handle horizontal movement
        float horizontalInput = moveDirection.x;
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        
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
        
        // Set standard animation parameters
        animator.SetBool(ANIM_IS_WALKING, isMovingHorizontally && isGrounded);
        animator.SetBool(ANIM_IS_GROUNDED, isGrounded);
        animator.SetFloat(ANIM_VERTICAL_VELOCITY, rb.velocity.y);
        
        // Method 1: Use boolean parameter for facing direction
        animator.SetBool(ANIM_FACING_RIGHT, facingRight);
        
        // Method 2: Alternative - Use signed horizontal speed
        // This allows the animator to handle direction more smoothly
        float signedHorizontalSpeed = facingRight ? Mathf.Abs(rb.velocity.x) : -Mathf.Abs(rb.velocity.x);
        animator.SetFloat(ANIM_HORIZONTAL_SPEED, signedHorizontalSpeed);
    }
    
    // Public methods for other scripts to trigger animations
    public void Jump()
    {
        if (isGrounded && animator != null)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            animator.SetTrigger("jump");
        }
    }
    
    public void TriggerAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("attack");
        }
    }
    
    public void TriggerHurt()
    {
        if (animator != null)
        {
            animator.SetTrigger("hurt");
        }
    }
    
    // Getters for other scripts
    public bool IsGrounded => isGrounded;
    public bool IsMovingHorizontally => isMovingHorizontally;
    public bool FacingRight => facingRight;
    public float CurrentHorizontalSpeed => rb.velocity.x;
    public float CurrentVerticalSpeed => rb.velocity.y;
    
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