using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main Player class that inherits from DamageableEntity and coordinates all player controllers.
/// This is the central hub for all player functionality.
/// </summary>
public class Player : DamageableEntity, ISaveable
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
        
        // Trigger game over
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        else
        {
            Debug.LogWarning("GameManager not found! Game over not triggered.");
        }
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
    
    #region ISaveable Implementation
    
    public string GetSaveId()
    {
        // Player should have a consistent ID since there's only one player
        return "player_main";
    }
    
    public object GetSaveData()
    {
        // Collect inventory data
        var inventoryItems = new List<SerializableInventoryItem>();
        if (inventoryController != null)
        {
            var inventorySystem = inventoryController.GetInventorySystem();
            if (inventorySystem != null)
            {
                var items = inventorySystem.GetAllItems();
                Debug.Log($"Player.GetSaveData: Found {items.Count} total inventory slots");
                
                int nonNullCount = 0;
                foreach (var item in items)
                {
                    if (item != null)
                    {
                        nonNullCount++;
                        inventoryItems.Add(SerializableInventoryItem.FromInventoryItem(item));
                        Debug.Log($"Player.GetSaveData: Saving item {item.ItemName} x{item.Quantity}");
                    }
                    // Skip null items entirely - we don't need to maintain slot positions in save data
                }
                Debug.Log($"Player.GetSaveData: Saved {nonNullCount} items to inventory data");
            }
            else
            {
                Debug.LogWarning("Player.GetSaveData: InventorySystem not found via inventoryController.GetInventorySystem()!");
            }
        }
        else
        {
            Debug.LogWarning("Player.GetSaveData: inventoryController is null!");
        }
        
        // Get weapon ammo data
        int currentAmmo = 0;
        int maxAmmo = 0;
        if (weaponController?.CurrentWeapon != null)
        {
            currentAmmo = weaponController.CurrentWeapon.CurrentAmmo;
            maxAmmo = weaponController.CurrentWeapon.MaxAmmo;
        }

        return new PlayerSaveDataDetailed
        {
            position = Position,
            currentHealth = Health,
            maxHealth = MaxHealth,
            isAlive = IsAlive,
            facingRight = FacingRight,
            inventoryItems = inventoryItems,
            maxInventorySlots = inventoryController?.GetInventorySystem()?.MaxSlots ?? 20,
            currentAmmo = currentAmmo,
            maxAmmo = maxAmmo,
            hasWeapon = weaponController?.HasWeapon ?? false
        };
    }
    
    public void LoadSaveData(object data)
    {
        if (data is PlayerSaveDataDetailed saveData)
        {
            // Restore position
            transform.position = saveData.position;
            
            // Restore health
            SetHealth(saveData.currentHealth);
            SetMaxHealth(saveData.maxHealth);
            
            // Handle death state
            if (!saveData.isAlive && IsAlive)
            {
                TakeDamage(Health, Vector2.zero, Vector2.zero);
            }
            else if (saveData.isAlive && !IsAlive)
            {
                Revive(saveData.currentHealth / saveData.maxHealth);
            }
            
            // Restore facing direction
            if (movementController != null && saveData.facingRight != FacingRight)
            {
                // This depends on your movement controller implementation
                // You might need to adjust this based on how facing direction is handled
            }
            
            // Restore inventory
            if (inventoryController != null && saveData.inventoryItems != null)
            {
                var inventorySystem = inventoryController.GetInventorySystem();
                if (inventorySystem != null)
                {
                    // Clear current inventory
                    var currentItems = inventorySystem.GetAllItems();
                    for (int i = 0; i < currentItems.Count; i++)
                    {
                        if (currentItems[i] != null)
                        {
                            inventorySystem.RemoveItemAt(i);
                        }
                    }
                    
                    // Load saved items (add them sequentially)
                    foreach (var savedItem in saveData.inventoryItems)
                    {
                        if (savedItem != null)
                        {
                            var item = savedItem.ToInventoryItem();
                            if (item != null)
                            {
                                inventorySystem.AddItem(item);
                                Debug.Log($"Player.LoadSaveData: Loaded item {item.ItemName} x{item.Quantity}");
                            }
                        }
                    }
                }
            }
            
            // Restore weapon ammo
            if (weaponController?.CurrentWeapon != null && saveData.hasWeapon)
            {
                weaponController.CurrentWeapon.SetAmmo(saveData.currentAmmo);
            }
            
            Debug.Log($"Player data loaded: Health={Health}/{MaxHealth}, Position={Position}, Alive={IsAlive}, Ammo={saveData.currentAmmo}/{saveData.maxAmmo}");
        }
    }
    
    #endregion
}

/// <summary>
/// Detailed save data for Player including inventory and weapon ammo
/// </summary>
[System.Serializable]
public class PlayerSaveDataDetailed
{
    public Vector2 position;
    public float currentHealth;
    public float maxHealth;
    public bool isAlive;
    public bool facingRight;
    public List<SerializableInventoryItem> inventoryItems = new List<SerializableInventoryItem>();
    public int maxInventorySlots;
    
    // Weapon data
    public bool hasWeapon;
    public int currentAmmo;
    public int maxAmmo;
} 