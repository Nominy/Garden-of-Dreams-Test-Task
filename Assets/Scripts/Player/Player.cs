using UnityEngine;

/// <summary>
/// Main Player class that inherits from DamageableEntity and coordinates all player controllers.
/// This is the central hub for all player functionality.
/// </summary>
public class Player : DamageableEntity
{
    [Header("Player Controllers")]
    [SerializeField] private PlayerMovementController movementController;
    [SerializeField] private PlayerWeaponController weaponController;
    [SerializeField] private PlayerInventoryController inventoryController;
    
    [Header("Player Settings")]
    [SerializeField] private bool autoFindControllers = true;
    [SerializeField] private bool enableControllerDebugLogs = false;
    
    // Controller references (cached for performance)
    private PlayerControllerBase[] allControllers;
    
    // Public properties for external access
    public PlayerMovementController Movement => movementController;
    public PlayerWeaponController Weapon => weaponController;
    public PlayerInventoryController Inventory => inventoryController;
    
    // Player state properties
    public bool IsMoving => movementController != null && movementController.IsMovingHorizontally;
    public bool FacingRight => movementController != null && movementController.FacingRight;
    public Vector2 Position => transform.position;
    public Vector2 Velocity => movementController != null ? movementController.Velocity : Vector2.zero;
    
    protected override void Awake()
    {
        base.Awake(); // Initialize DamageableEntity
        
        // Disable melee attacks for player (as requested)
        SetMeleeAttackEnabled(false);
        
        // Auto-find controllers if enabled
        if (autoFindControllers)
        {
            FindControllers();
        }
        
        // Initialize all controllers
        InitializeControllers();
    }
    
    void Start()
    {
        // Validate setup
        ValidateControllers();
        
        // Subscribe to death event for game over logic
        OnDeath += HandlePlayerDeath;
    }
    
    private void FindControllers()
    {
        if (movementController == null)
            movementController = GetComponent<PlayerMovementController>();
        
        if (weaponController == null)
            weaponController = GetComponent<PlayerWeaponController>();
        
        if (inventoryController == null)
            inventoryController = GetComponent<PlayerInventoryController>();
    }
    
    private void InitializeControllers()
    {
        // Get all player controller components
        allControllers = GetComponents<PlayerControllerBase>();
        
        // Initialize each controller
        foreach (var controller in allControllers)
        {
            if (controller != null)
            {
                controller.Initialize(this);
                
                if (enableControllerDebugLogs)
                    Debug.Log($"Player: Initialized {controller.GetType().Name}");
            }
        }
    }
    
    private void ValidateControllers()
    {
        bool hasErrors = false;
        
        if (movementController == null)
        {
            Debug.LogError("Player: No PlayerMovementController found! Player movement will not work.");
            hasErrors = true;
        }
        
        if (weaponController == null)
        {
            Debug.LogWarning("Player: No PlayerWeaponController found. Player will not be able to use weapons.");
        }
        
        if (inventoryController == null)
        {
            Debug.LogWarning("Player: No PlayerInventoryController found. Player will not be able to manage inventory.");
        }
        
        if (hasErrors)
        {
            Debug.LogError("Player: Critical controllers are missing! Please assign them in the inspector or enable autoFindControllers.");
        }
    }
    
    private void HandlePlayerDeath()
    {
        Debug.Log("Player has died!");
        
        // Disable all controllers when player dies
        foreach (var controller in allControllers)
        {
            if (controller != null)
                controller.enabled = false;
        }
        
        // You can add game over logic here
        // GameManager.Instance.GameOver();
    }
    
    // Public methods for external systems to interact with the player
    
    /// <summary>
    /// Trigger attack animation on the player.
    /// </summary>
    public void TriggerAttack()
    {
        if (movementController != null)
            movementController.TriggerAttack();
    }
    
    /// <summary>
    /// Trigger hurt animation on the player.
    /// </summary>
    public void TriggerHurt()
    {
        if (movementController != null)
            movementController.TriggerHurt();
    }
    
    /// <summary>
    /// Try to fire the current weapon.
    /// </summary>
    /// <returns>True if weapon was fired successfully</returns>
    public bool TryFireWeapon()
    {
        return weaponController != null && weaponController.TryFire();
    }
    

    
    /// <summary>
    /// Add an item to the player's inventory.
    /// </summary>
    /// <param name="item">Item to add</param>
    /// <returns>True if item was added successfully</returns>
    public bool AddToInventory(InventoryItem item)
    {
        return inventoryController != null && inventoryController.AddItem(item);
    }
    
    /// <summary>
    /// Check if player has inventory space.
    /// </summary>
    /// <returns>True if inventory has space</returns>
    public bool HasInventorySpace()
    {
        return inventoryController != null && inventoryController.HasInventorySpace();
    }
    
    /// <summary>
    /// Get the player's current aim direction.
    /// </summary>
    /// <returns>Normalized aim direction vector</returns>
    public Vector2 GetAimDirection()
    {
        return weaponController != null ? weaponController.AimDirection : Vector2.right;
    }
    
    // Override TakeDamage to add player-specific damage response
    public override void TakeDamage(float damage, Vector2 impactPoint, Vector2 impactDirection)
    {
        base.TakeDamage(damage, impactPoint, impactDirection);
        
        // Trigger hurt animation
        TriggerHurt();
        
        // You can add player-specific damage effects here
        // Camera shake, screen flash, etc.
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (OnDeath != null)
            OnDeath -= HandlePlayerDeath;
    }
} 