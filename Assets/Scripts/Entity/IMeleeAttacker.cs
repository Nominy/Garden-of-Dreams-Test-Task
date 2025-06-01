using UnityEngine;

/// <summary>
/// Interface for entities that can perform melee attacks.
/// </summary>
public interface IMeleeAttacker
{
    /// <summary>
    /// Attempt to perform a melee attack at the specified target.
    /// </summary>
    /// <param name="target">The target to attack</param>
    /// <param name="attackDamage">Damage to deal</param>
    /// <returns>True if attack was successful</returns>
    bool TryMeleeAttack(IDamageable target, float attackDamage);
    
    /// <summary>
    /// Attempt to perform a melee attack in a specific direction.
    /// </summary>
    /// <param name="attackPosition">Position where the attack occurs</param>
    /// <param name="attackDirection">Direction of the attack</param>
    /// <param name="attackRange">Range of the attack</param>
    /// <param name="attackDamage">Damage to deal</param>
    /// <param name="targetLayers">Layers to hit</param>
    /// <returns>True if any target was hit</returns>
    bool TryMeleeAttack(Vector2 attackPosition, Vector2 attackDirection, float attackRange, float attackDamage, LayerMask targetLayers);
    
    /// <summary>
    /// Check if this entity can currently perform a melee attack.
    /// </summary>
    bool CanMeleeAttack { get; }
} 