using UnityEngine;

public class PlayerController : MonoBehaviour
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
    
    void FixedUpdate()
    {
        CheckGrounded();
        HandleAnimations();
        HandleMovement();
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
        const int PPU = 32;
        rb.position = new Vector2(
            Mathf.Round(rb.position.x * PPU) / PPU,
            rb.position.y );
        rb.velocity = new Vector2(
            Mathf.Round(horizontalInput * moveSpeed) / PPU,
            rb.velocity.y);

        
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
        animator.SetFloat(ANIM_VERTICAL_VELOCITY, rb.velocity.y);
        
        animator.SetBool(ANIM_FACING_RIGHT, facingRight);
        
        float signedHorizontalSpeed = facingRight ? Mathf.Abs(rb.velocity.x) : -Mathf.Abs(rb.velocity.x);
        animator.SetFloat(ANIM_HORIZONTAL_SPEED, signedHorizontalSpeed);
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