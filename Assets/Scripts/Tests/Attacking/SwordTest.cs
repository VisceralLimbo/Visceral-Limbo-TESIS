using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwordTest : Visceral_WeaponBase
{
    PlayerContext _UserContext;
    Player_MeleeAttack _meleeAttack; // refe

    [SerializeField] private PlayerRayCast playerRay;

    private void Start()
    {
        _UserContext = this.transform.root.GetComponentInChildren<PlayerContext>();
        playerRay = _UserContext.GetComponentInChildren<PlayerRayCast>();
        _meleeAttack = _UserContext.GetComponent<Player_MeleeAttack>();
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

    /// <summary>
    /// Funcion de notificar que se realizo un golpe exitoso, aplica daño directo al HPComponent
    /// </summary>
    /// <param name="other"></param>
    /// <param name="HPComp"></param>
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

        // veo que particulas usa dependiendo si esta activo el item de bleed
        EnemyDamageFlash damageFlash = HPComp.GetComponent<EnemyDamageFlash>();
        if (damageFlash != null && _meleeAttack != null)
        {
            damageFlash.SetBleedStatus(_meleeAttack.IsBleedEffectActive);
        }
        SlowMotion.Stop(0.1f, 0.05f, false);
    }

    /// <summary>
    /// Funcion de notificar que se realizo un golpe exitoso, aplica daño al IDamageable.
    /// sirve principalmente en casos de que tengamos Logica especial de daño.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="IDamage"></param>
    public override void NotifyHit(Collider other, IDamageable IDamage = null)
    {
        if (IDamage == null) return;

        if (IDamage.GetHealthComponent(out Health_Component HPComp) == false) return;

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

        IDamage.TakeDamageWithKnockback(Dir, KnockBack, damageScore);

        //
        PlayerEvents.PlayerSucessfulHit();

        // veo que particulas usa dependiendo si esta activo el item de bleed
        EnemyDamageFlash damageFlash = HPComp.GetComponent<EnemyDamageFlash>();
        if (damageFlash != null && _meleeAttack != null)
        {
            damageFlash.SetBleedStatus(_meleeAttack.IsBleedEffectActive);
        }
        SlowMotion.Stop(0.1f, 0.05f, false);



    }



}
