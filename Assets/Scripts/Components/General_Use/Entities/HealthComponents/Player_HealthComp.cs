using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_HealthComp : Health_Component
{

    private void Start()
    {
       
        // setteamos que la salud máxima es la del stat system.
        MaxHealth = _Context.Stats.GetFloatStatValue("MaxHealth");

        //suscribimos a stats cuando se cambian
        _Context.Stats.OnStatChanged += UpdateStatValues;

        CurrentHealth = MaxHealth;

        _RB = GetComponent<Rigidbody>();
        _Context = GetComponentInParent<PlayerContext>();

       OnDamaged += updateHealthBar;

    }

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


    private void updateHealthBar()
    {
            Combat_UI_Manager._Instance.UpdatePlayerHealthBar(CurrentHealth, MaxHealth);
            if (CurrentHealth <= 0)
            {
                Combat_UI_Manager._Instance.DisplayLose(true);
            }
    }

    private void UpdateStatValues(string statID, float values)
    {
        if (statID == "MaxHealth")
        {
            MaxHealth = values;
            Combat_UI_Manager._Instance.UpdatePlayerHealthBar(CurrentHealth, MaxHealth, false);
        }
    }


}
