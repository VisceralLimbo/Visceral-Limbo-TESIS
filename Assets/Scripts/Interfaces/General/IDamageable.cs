using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable 
{
    public void TakeDamage(DamageScore DamageDT);
    public void TakeDamageWithKnockback(Vector3 KnockbackDir, float KnockbackForce, DamageScore DamageDT);

    public void SimpleDamage(float Damage);

    public bool GetHealthComponent(out Health_Component HPComp);
    public bool GetPlayerContext(out PlayerContext playerContext);
}
