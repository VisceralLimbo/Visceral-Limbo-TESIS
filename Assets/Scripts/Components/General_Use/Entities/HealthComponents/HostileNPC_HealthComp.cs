using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostileNPC_HealthComp : Health_Component
{
    public override void HealHP(float ExtraHP, bool OverHeal = false)
    {
        base.HealHP(ExtraHP, OverHeal);
    }

    public override void SimpleDamage(float Damage)
    {
        base.SimpleDamage(Damage);
    }

    public override void SimpleDamage(Tuple<Vector3, float, float> MyTuple)
    {
        base.SimpleDamage(MyTuple);
    }

    public override void TakeDamage(DamageScore DamageDT)
    {
        base.TakeDamage(DamageDT);
    }

    public override void TakeDamageWithKnockback(Vector3 Direction, float knockback, DamageScore DamageDT)
    {
        base.TakeDamageWithKnockback(Direction, knockback, DamageDT);
    }

    protected override void InternalDamage(float damage, Vector3? KnockbarDir, float force, DamageScore Score = null)
    {
        base.InternalDamage(damage, KnockbarDir, force, Score);
    }
}
