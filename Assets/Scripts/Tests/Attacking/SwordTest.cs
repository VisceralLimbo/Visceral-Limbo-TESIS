using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwordTest : Visceral_WeaponBase
{
    PlayerContext _UserContext;


    private void Start()
    {
        _UserContext = this.transform.root.GetComponentInChildren<PlayerContext>();
    }

    public override void Attacking()
    {
        EnableWeaponCollision();
        StartAttack?.Invoke();
    }

    public override void StopAttacking()
    {
        DisableWeaponCollision();
        EndAttack?.Invoke();
    }

    protected override void DisableWeaponCollision()
    {

        foreach (var Item in _WeaponColliders)
        {
            Item.DeactivateCollider();
        }
    }

    protected override void EnableWeaponCollision()
    {
        foreach (var Item in _WeaponColliders)
        {
            Item.activateCollider();
        }

    }

    public override void NotifyHit(Collider other, Health_Component HPComp)
    {
        //hitting user
        if (HPComp.Context != null && HPComp.Context == _UserContext) return;

        DamageScore damageScore = new DamageScore()
        {
            DamageAmount = Damage,
            Attacker = _UserContext,
            ElementalDamage = ElementType.Physical,
            IsAirBorneKill = _UserContext.KCCMotor.GroundingStatus.IsStableOnGround,
            FactionID = _UserContext.faction = FactionID.Player,
        };


        Vector3 Dir = other.transform.position - this.transform.position;
        Dir = Dir.normalized;
       
        
        HPComp.TakeDamageWithKnockback(Dir,KnockBack,damageScore);

        BloodScreenManager.Instance?.ShowRandomBloodSplash();

        SlowMotion.Stop(0.08f, 0.05f);

    }

}
