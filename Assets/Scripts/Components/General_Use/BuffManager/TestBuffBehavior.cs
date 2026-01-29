using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBuffBehavior : BuffBehavior
{

    [SerializeField] Health_Component _HP;
    [SerializeField] float timer = 5;
    [SerializeField] int HPHealth = 10;
    [SerializeField] StatIdentifier _HPIdentifier;
    public override void OnAddPotency(int ExtraPotency)
    {
        _BuffPotency += ExtraPotency;
        HPHealth += _BuffPotency;
        timer = timer / 1 * _BuffPotency;

    }

    public override void OnApply(BuffManager manager, StatsManager StatMan)
    {
        _buffManager = manager;
        _StatMan = StatMan;
        _HPIdentifier = _BuffSO.statIdentifiers[0];

        _HP = StatMan.GetComponentInChildren<Health_Component>();
        print("healing buff");
    }

    public override void OnExpire()
    {
        print("ah shit, that buff was laced yo!");
        _HP.SimpleDamage(HPHealth * _BuffPotency);
    }

    [SerializeField] float pulse;
    public override void OnUpdate(float deltaTime)
    {
        if(pulse < timer)
        {
            pulse += deltaTime;
            return;
        }
        else
        {
            pulse = 0;
            _HP.HealHP(HPHealth);
        }

    }
}
