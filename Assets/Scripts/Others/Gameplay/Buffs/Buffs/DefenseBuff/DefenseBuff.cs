using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseBuff : BuffBehavior
{
    public override PlayerContext GetOrigin()
    {
        return base.GetOrigin();
    }

    public override BuffSO GetSO()
    {
        return base.GetSO();
    }

    public override void OnAddPotency(int ExtraPotency)
    {
        _BuffPotency += ExtraPotency;
    }

    public override void OnApply(BuffManager manager, StatsManager StatMan)
    {
        
    }

    public override void OnExpire()
    {

    }

    public override void OnUpdate(float deltaTime)
    {
        
    }

    public override void SetOrigin(PlayerContext inflictor)
    {
        base.SetOrigin(inflictor);
    }

    public override void SetSO(BuffSO SO)
    {
        base.SetSO(SO);
    }
}
