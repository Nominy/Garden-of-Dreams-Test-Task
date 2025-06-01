using UnityEngine;

public class DamageableEntity : MonoBehaviour, IDamageable, IMeleeAttacker
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("Damage Response")]
    [SerializeField] private bool showDamageNumbers = true;
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private float invulnerabilityTime = 0.5f;
    
    [Header("Effects")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;
    
    [Header("Melee Attack")]
    [SerializeField] private float meleeAttackCooldown = 1f;
    [SerializeField] private bool canPerformMeleeAttacks = true;
    
    protected SpriteRenderer spriteRenderer;
    
    private bool isDead = false;
    private bool isInvulnerable = false;
    private Color originalColor;
    private AudioSource audioSource;
    private float lastMeleeAttackTime = 0f;
    
    // Events
    public System.Action<float, float> OnHealthChanged; // currentHealth, maxHealth
    public System.Action<float, Vector2, Vector2> OnDamageTaken; // damage, impactPoint, impactDirection
    public System.Action OnDeath;
    
    // Properties
    public float Health => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsAlive => !isDead;
    public bool IsInvulnerable => isInvulnerable;
    public bool CanMeleeAttack => IsAlive && canPerformMeleeAttacks && Time.time >= lastMeleeAttackTime + meleeAttackCooldown;
    
    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    
    void Start()
    {
        // Notify initial health state
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public virtual void TakeDamage(float damage, Vector2 impactPoint, Vector2 impactDirection)
    {
        // Check if already dead or invulnerable
        if (isDead || isInvulnerable) return;
        
        // Apply damage
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        // Trigger events
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamageTaken?.Invoke(damage, impactPoint, impactDirection);
        
        // Visual/audio feedback
        PlayDamageEffects(damage, impactPoint);
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Start invulnerability period
            if (invulnerabilityTime > 0)
            {
                StartCoroutine(InvulnerabilityCoroutine());
            }
        }
        
        // Show damage numbers
        if (showDamageNumbers)
        {
            ShowDamageNumber(damage, impactPoint);
        }
        
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
    }
    
    private void PlayDamageEffects(float damage, Vector2 impactPoint)
    {
        // Play damage sound
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
        
        // Flash sprite
        if (spriteRenderer != null)
        {
            StartCoroutine(DamageFlashCoroutine());
        }
    }
    
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        // Play death sound
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Spawn death effect
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f); // Clean up after 3 seconds
        }
        
        // Trigger death event
        OnDeath?.Invoke();
        
        Debug.Log($"{gameObject.name} has died!");
        
        // Disable or destroy the object
        // You might want to play a death animation first
        gameObject.SetActive(false);
        // Or: Destroy(gameObject, 1f); // Delay to allow death effects
    }
    
    private System.Collections.IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        
        // Optional: Make sprite flicker during invulnerability
        float flickerInterval = 0.1f;
        float elapsed = 0f;
        
        while (elapsed < invulnerabilityTime)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(originalColor, Color.clear, 0.5f);
            }
            
            yield return new WaitForSeconds(flickerInterval);
            elapsed += flickerInterval;
            
            if (elapsed >= invulnerabilityTime) break; // Exit early if time exceeded
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
            
            yield return new WaitForSeconds(flickerInterval);
            elapsed += flickerInterval;
        }
        
        // Ensure sprite is visible
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        isInvulnerable = false;
    }
    
    private System.Collections.IEnumerator DamageFlashCoroutine()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = damageFlashColor;
            
            yield return new WaitForSeconds(flashDuration);
            
            spriteRenderer.color = originalColor;
        }
    }
    
    private void ShowDamageNumber(float damage, Vector2 position)
    {
        if (damageNumberPrefab != null)
        {
            GameObject damageNumber = Instantiate(damageNumberPrefab, position, Quaternion.identity);
            
            // Try to set the damage value if the prefab has a text component
            var textComponent = damageNumber.GetComponentInChildren<TMPro.TextMeshPro>();
            if (textComponent != null)
            {
                textComponent.text = damage.ToString("F0");
            }
            else
            {
                var uiText = damageNumber.GetComponentInChildren<UnityEngine.UI.Text>();
                if (uiText != null)
                {
                    uiText.text = damage.ToString("F0");
                }
            }
            
            // Destroy damage number after a short time
            Destroy(damageNumber, 1f);
        }
    }
    
    // Utility methods
    public void Heal(float amount)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }
    
    public void SetMaxHealth(float newMaxHealth, bool healToFull = false)
    {
        maxHealth = newMaxHealth;
        
        if (healToFull)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void Revive(float healthPercentage = 1f)
    {
        if (!isDead) return;
        
        isDead = false;
        currentHealth = maxHealth * Mathf.Clamp01(healthPercentage);
        gameObject.SetActive(true);
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    // IMeleeAttacker implementation
    public virtual bool TryMeleeAttack(IDamageable target, float attackDamage)
    {
        if (!CanMeleeAttack || target == null || !target.IsAlive) return false;
        
        lastMeleeAttackTime = Time.time;
        
        // Calculate attack direction toward target
        Vector2 attackDirection = Vector2.zero;
        if (target is Component targetComponent)
        {
            attackDirection = (targetComponent.transform.position - transform.position).normalized;
        }
        
        // Apply damage
        Vector2 impactPoint = transform.position;
        if (target is Component comp)
        {
            impactPoint = comp.transform.position;
        }
        
        target.TakeDamage(attackDamage, impactPoint, attackDirection);
        
        return true;
    }
    
    public virtual bool TryMeleeAttack(Vector2 attackPosition, Vector2 attackDirection, float attackRange, float attackDamage, LayerMask targetLayers)
    {
        if (!CanMeleeAttack) return false;
        
        lastMeleeAttackTime = Time.time;
        
        // Check for targets in attack range using circle cast
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(attackPosition, attackRange, targetLayers);
        
        bool hitAnyTarget = false;
        
        foreach (var hitCollider in hitTargets)
        {
            // Don't hit self
            if (hitCollider.gameObject == gameObject) continue;
            
            IDamageable damageable = hitCollider.GetComponent<IDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                Vector2 impactPoint = hitCollider.transform.position;
                damageable.TakeDamage(attackDamage, impactPoint, attackDirection);
                hitAnyTarget = true;
            }
        }
        
        return hitAnyTarget;
    }
    
    // Utility method for setting melee attack capabilities
    public void SetMeleeAttackEnabled(bool enabled)
    {
        canPerformMeleeAttacks = enabled;
    }
} 