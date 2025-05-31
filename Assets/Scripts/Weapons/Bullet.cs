using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Properties")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private LayerMask targetLayers = -1; // What layers can be hit
    
    [Header("Effects")]
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private bool destroyOnImpact = true;
    
    private Rigidbody2D rb;
    private Vector2 direction;
    private bool hasCollided = false;
    
    // Event for bullet impact - can be used for sound effects, etc.
    public System.Action<Vector3, float> OnBulletImpact;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Destroy bullet after lifetime expires
        Destroy(gameObject, lifetime);
    }
    
    public void Initialize(Vector2 fireDirection, float bulletSpeed = -1f, float bulletDamage = -1f, bool mirrorSprite = false)
    {
        direction = fireDirection.normalized;
        
        // Use provided values or defaults
        if (bulletSpeed > 0) speed = bulletSpeed;
        if (bulletDamage > 0) damage = bulletDamage;
        
        // Set initial velocity
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
        
        // Rotate bullet to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        // Mirror sprite if requested
        if (mirrorSprite)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.collider);
    }
    
    private void HandleCollision(Collider2D other)
    {
        // Prevent multiple collisions
        if (hasCollided) return;
        
        // Check if we should collide with this layer
        if (!IsInLayerMask(other.gameObject.layer, targetLayers)) return;
        
        hasCollided = true;
        
        // Calculate impact point and direction
        Vector2 impactPoint = transform.position;
        Vector2 impactDirection = direction;
        
        // Try to get a more accurate impact point from the collider
        Vector2 closestPoint = other.ClosestPoint(transform.position);
        if (closestPoint != (Vector2)transform.position)
        {
            impactPoint = closestPoint;
        }
        
        // Check if target can take damage
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage, impactPoint, impactDirection);
        }
        
        // Apply physics impact to rigidbodies
        Rigidbody2D targetRb = other.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            float impactForce = damage * 0.5f; // You can adjust this multiplier
            targetRb.AddForceAtPosition(impactDirection * impactForce, impactPoint, ForceMode2D.Impulse);
        }
        
        // Trigger impact event
        OnBulletImpact?.Invoke(impactPoint, damage);
        
        // Spawn impact effect
        if (impactEffect != null)
        {
            GameObject effect = Instantiate(impactEffect, impactPoint, Quaternion.LookRotation(Vector3.forward, impactDirection));
            Destroy(effect, 2f); // Clean up effect after 2 seconds
        }
        
        // Destroy bullet if configured to do so
        if (destroyOnImpact)
        {
            Destroy(gameObject);
        }
        else
        {
            // Stop the bullet
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
        }
    }
    
    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return layerMask == (layerMask | (1 << layer));
    }
    
    // Public getters for external access
    public float Damage => damage;
    public float Speed => speed;
    public Vector2 Direction => direction;
} 