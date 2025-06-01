using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage, Vector2 impactPoint, Vector2 impactDirection);
    float Health { get; }
    bool IsAlive { get; }
} 