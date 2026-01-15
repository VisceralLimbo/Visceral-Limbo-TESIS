using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilitySkillBoost : BuffBehavior
{
    [SerializeField] string StatID = "BaseAttack";
    [SerializeField] float _StartingDamageMultiplerValue = 1;

    [SerializeField] Player_MeleeAttack _MeleeAttack;

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
        _StatMan = StatMan;

        if(_BuffSO == null || _StatMan== null)
        {
            print("_buffSO is null" );
        }

        PlayerEvents.OnPlayerSuccesfulHit += ActivatedBuff;
        _MeleeAttack = StatMan.GetComponentInChildren<Player_MeleeAttack>();

        float TotalPotency = _StartingDamageMultiplerValue * (1 * 0.15f * (_BuffPotency - 1));


        StatModifierFloat _Mod = new StatModifierFloat();

        _Mod.ModifierValueFloat = TotalPotency;
        _Mod.ModifierValue = TotalPotency;
        _Mod.ModType = ModifierType.percentAdd;
        _Mod.Source = manager;
        _Mod.EffectName = "AttackBuff";

        if(_Mod == null)
        {
            Debug.Log("_mod is null");
        }
        if(StatID == null)
        {
            Debug.Log("StatID is null");
        }
        if(StatMan == null)
        {
            Debug.Log("StatMan is null");
        }

        StatMan.UpdateFloatStatValue("BaseAttack", _Mod);

        if(_MeleeAttack != null)
        {
            _MeleeAttack.IsEvasionBoostActive = true;
        }
    }

    public override void OnExpire()
    {
        _StatMan.RemoveFloatStatModifier("BaseAttack", _BuffSO.BuffID);

        if (_MeleeAttack != null)
        {
            _MeleeAttack.IsEvasionBoostActive = false;
        }
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

    private void ActivatedBuff()
    {
        _buffManager.ForceExpirationBuff(_BuffSO.BuffID);
    }

   
}
