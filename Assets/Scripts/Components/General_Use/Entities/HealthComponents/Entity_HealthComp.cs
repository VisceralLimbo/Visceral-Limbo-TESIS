using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity_HealthComp : Health_Component
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
        CurrentHealth -= damage;

        if (Died) return;

        CurrentHealth -= damage;

        if (soundData != null)
        {
            PlaySounds(); // feedback de sonidos
        }


       
        if (KnockbarDir.HasValue && _RB != null)
        {
            print("rigidbody recieving knockback");
            _RB.AddForce(KnockbarDir.Value * force, ForceMode.Impulse);
        }

        if (CurrentHealth <= 0f)
        {
            Died = true;
            if (_Context == null)
            {
                if (DesactivateOnDeath) gameObject.SetActive(false);
                if (DestroyOnDeath) Destroy(gameObject);
            }
            else
            {
                if (DesactivateOnDeath) _Context.PlayerGameObject.SetActive(false);
                if (DestroyOnDeath) Destroy(_Context.PlayerGameObject);
            }
        }

    }
}
