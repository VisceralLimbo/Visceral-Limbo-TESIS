using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostileNPC_HealthComp : Health_Component
{
    [Space]
    [Header("Stats Setup")]
    [SerializeField] StatsManager _StatMan;
    [SerializeField] string _MaxHPStat;
    [SerializeField] string _DefenseStat;
    [SerializeField] string _DamageReductionStat;
    [SerializeField] string _DamageInvulnerability;


    public override void VS_Initialize()
    {
        base.VS_Initialize();
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

    private void Start()
    {
        if(_StatMan == null)
        {
            _StatMan = _Context?.GetComponent<StatsManager>();
            if(_StatMan == null)
            {
                _StatMan = _Context?.GetComponentInChildren<StatsManager>();
            }
        }

        if(_StatMan != null)
        {
            _StatMan.OnStatChanged += UpdateStats;
        }
    }

    private void UpdateStats(string StatID, float value)
    {
        if (StatID == _MaxHPStat) MaxHealth= value;
        if (StatID == _DefenseStat) return;
        if (StatID == _DamageReductionStat) return;
        if (StatID == _DamageInvulnerability) return;


    }

}
