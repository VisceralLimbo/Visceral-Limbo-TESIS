using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwordTest : Visceral_WeaponBase
{
    PlayerContext _UserContext;
  
    [SerializeField] private PlayerRayCast playerRay;

    private void Start()
    {
        _UserContext = this.transform.root.GetComponentInChildren<PlayerContext>();
        playerRay = _UserContext.GetComponentInChildren<PlayerRayCast>();
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
            Victim = HPComp.Context,
            ElementalDamage = ElementType.Physical,
            IsAirBorneKill = _UserContext.KCCMotor.GroundingStatus.IsStableOnGround,
            FactionID = _UserContext.faction = FactionID.Player,
        };

        var Dir = playerRay.transform.forward;

        _UserContext.Inventory.ProcOnHitEffects(_UserContext, damageScore, HPComp);
   
        HPComp.TakeDamageWithKnockback(Dir,KnockBack,damageScore);

        //
        PlayerEvents.PlayerSucessfulHit();

        // notifico al flash sobre el bleed
        EnemyDamageFlash damageFlash = HPComp.GetComponent<EnemyDamageFlash>();
        if (damageFlash != null)
        {
            damageFlash.SetBleedStatus(true);
        }



        SlowMotion.Stop(0.1f, 0.05f, false);


    }




}
