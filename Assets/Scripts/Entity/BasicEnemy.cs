using UnityEngine;
using System.Collections;

public class BasicEnemy : DamageableEntity, ISaveable
{
    [Header("Enemy Behavior")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 2f;
    
    [Header("Loot")]
    [SerializeField] private EnemyLootTable lootTable;
    [SerializeField] private Transform lootSpawnPoint;
    
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private EnemyHealthBar healthBar;
    
    private Rigidbody2D rb;
    private Animator animator;
    private bool canAttack = true;
    private bool playerInRange = false;
    private bool facingRight = true;
    
    // Animation parameter names (add these to your enemy animator)
    private const string ANIM_IS_WALKING = "isWalking";
    private const string ANIM_ATTACK = "attack";
    private const string ANIM_FACING_RIGHT = "facingRight";
    
    protected override void Awake()
    {
        base.Awake(); // Call parent Awake to initialize spriteRenderer
        
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // If no player reference is set, try to find it
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
        
        // If no loot spawn point is set, use own transform
        if (lootSpawnPoint == null)
            lootSpawnPoint = transform;
    }
    
    void Start()
    {
        
        // Setup health bar
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(MaxHealth);
            healthBar.SetHealth(Health);
            
            // Subscribe to health changes
            OnHealthChanged += UpdateHealthBar;
        }
        
        // Subscribe to death event for loot dropping
        OnDeath += DropLoot;
    }
    
    void Update()
    {
        if (rb == null) // Defensive check and re-fetch
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogError($"Critical: Rigidbody2D still not found on {gameObject.name} in Update. Disabling enemy.");
                enabled = false; // Disable this script to prevent further errors
                return;
            }
        }

        if (!IsAlive || player == null) return;
        
        CheckPlayerDistance();
        HandleMovement();
        HandleAttack();
        UpdateAnimations();
    }
    
    private void CheckPlayerDistance()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Check if player is in detection range
        playerInRange = distanceToPlayer <= detectionRange;
        
        // Show/hide health bar based on player proximity or if damaged
        if (healthBar != null)
        {
            bool shouldShowHealthBar = playerInRange || Health < MaxHealth;
            healthBar.SetVisible(shouldShowHealthBar);
        }
    }
    
    private void HandleMovement()
    {
        if (!playerInRange) 
        {
            // Stop moving if player not in range
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Move toward player if not in attack range
        if (distanceToPlayer > attackRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
            
            // Update facing direction
            UpdateFacingDirection(direction.x);
        }
        else
        {
            // Stop moving when in attack range
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }
    
    private void HandleAttack()
    {
        if (!playerInRange || !canAttack || !CanMeleeAttack) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= attackRange)
        {
            StartCoroutine(AttackPlayer());
        }
    }
    
    private IEnumerator AttackPlayer()
    {
        canAttack = false;
        
        // Play attack animation
        if (animator != null)
            animator.SetTrigger(ANIM_ATTACK);
        
        // Wait a bit for attack animation to play
        yield return new WaitForSeconds(0.5f);
        
        // Check if player is still in range (they might have moved)
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            // Try to damage the player using melee attack system
            IDamageable playerDamageable = player.GetComponent<IDamageable>();
            if (playerDamageable != null)
            {
                // Use the inherited melee attack method
                TryMeleeAttack(playerDamageable, attackDamage);
            }
        }
        
        // Wait for cooldown
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    
    private void UpdateFacingDirection(float horizontalDirection)
    {
        bool shouldFaceRight = horizontalDirection > 0;
        
        if (shouldFaceRight != facingRight)
        {
            facingRight = shouldFaceRight;
            
            // Flip sprite
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !facingRight;
            }
        }
    }
    
    private void UpdateAnimations()
    {
        if (animator == null) return;
        
        bool isWalking = Mathf.Abs(rb.velocity.x) > 0.1f;
        animator.SetBool(ANIM_IS_WALKING, isWalking);
        animator.SetBool(ANIM_FACING_RIGHT, facingRight);
    }
    
    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }
    }
    
    private void DropLoot()
    {
        if (lootTable != null)
        {
            var itemsToDrop = lootTable.GetLootDrops();
            foreach (var item in itemsToDrop)
            {
                SpawnItem(item);
            }
        }
    }
    
    private void SpawnItem(InventoryItem item)
    {
        if (item == null) return;
        
        // Create a new pickup object
        GameObject pickupObj = new GameObject($"Pickup_{item.ItemName}");
        pickupObj.transform.position = lootSpawnPoint.position + 
            (Vector3)Random.insideUnitCircle * 0.5f; // Scatter items slightly
        
        // Add necessary components
        SpriteRenderer sr = pickupObj.AddComponent<SpriteRenderer>();
        sr.sprite = item.ItemSprite;
        sr.sortingLayerName = "Default"; // Adjust as needed
        sr.sortingOrder = 10;
        
        CircleCollider2D col = pickupObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;
        
        ItemPickup pickup = pickupObj.AddComponent<ItemPickup>();
        pickup.SetupItem(item);
        
        // Add some physics for a nice effect
        Rigidbody2D pickupRb = pickupObj.AddComponent<Rigidbody2D>();
        pickupRb.gravityScale = 0.5f;
        Vector2 randomForce = Random.insideUnitCircle * 2f + Vector2.up * 3f;
        pickupRb.AddForce(randomForce, ForceMode2D.Impulse);
        
        // Remove physics after a short time
        StartCoroutine(RemovePhysicsAfterDelay(pickupRb, 1f));
    }
    
    private IEnumerator RemovePhysicsAfterDelay(Rigidbody2D rb, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }
    }
    
    // Gizmos for debugging
    private void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (OnHealthChanged != null)
            OnHealthChanged -= UpdateHealthBar;
        if (OnDeath != null)
            OnDeath -= DropLoot;
    }
    
    #region ISaveable Implementation
    
    public string GetSaveId()
    {
        // Generate a unique ID based on position and instance ID
        // This ensures enemies can be consistently identified across saves
        Vector2 pos = transform.position;
        return $"enemy_{GetInstanceID()}_{pos.x:F1}_{pos.y:F1}";
    }
    
    public object GetSaveData()
    {
        return new BasicEnemySaveData
        {
            position = transform.position,
            currentHealth = Health,
            maxHealth = MaxHealth,
            isAlive = IsAlive,
            isActive = gameObject.activeInHierarchy,
            facingRight = facingRight,
            playerInRange = playerInRange,
            // Save enemy stats
            detectionRange = detectionRange,
            attackRange = attackRange,
            moveSpeed = moveSpeed,
            attackDamage = attackDamage,
            attackCooldown = attackCooldown
        };
    }
    
    public void LoadSaveData(object data)
    {
        if (data is BasicEnemySaveData saveData)
        {
            // Restore position
            transform.position = saveData.position;
            
            // Restore health
            SetHealth(saveData.currentHealth);
            SetMaxHealth(saveData.maxHealth);
            
            // Restore state
            gameObject.SetActive(saveData.isActive);
            facingRight = saveData.facingRight;
            playerInRange = saveData.playerInRange;
            
            // Always reset canAttack to true when loading
            // This prevents getting stuck in "can't attack" state if save/load happened during attack coroutine
            canAttack = true;
            
            // Restore stats (in case they were modified during gameplay)
            detectionRange = saveData.detectionRange;
            attackRange = saveData.attackRange;
            moveSpeed = saveData.moveSpeed;
            attackDamage = saveData.attackDamage;
            attackCooldown = saveData.attackCooldown;
            
            // Handle death state
            if (!saveData.isAlive && IsAlive)
            {
                TakeDamage(Health, Vector2.zero, Vector2.zero);
            }
            else if (saveData.isAlive && !IsAlive)
            {
                Revive(saveData.currentHealth / saveData.maxHealth);
            }
            
            // Update visual state
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !facingRight;
            }
            
            Debug.Log($"BasicEnemy data loaded: Health={Health}/{MaxHealth}, Position={transform.position}, Alive={IsAlive}");
        }
    }
    
    #endregion
}

/// <summary>
/// Save data structure for BasicEnemy
/// </summary>
[System.Serializable]
public class BasicEnemySaveData
{
    public Vector3 position;
    public float currentHealth;
    public float maxHealth;
    public bool isAlive;
    public bool isActive;
    public bool facingRight;
    public bool playerInRange;
    
    // Enemy stats
    public float detectionRange;
    public float attackRange;
    public float moveSpeed;
    public float attackDamage;
    public float attackCooldown;
} 