using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwordTest : Visceral_WeaponBase
{
    PlayerContext _UserContext;
    private BleedItemLogic _BleedItem; //ref del item

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

        // nuevo para las nuevas particulas
        bool isBleedActive = false;

        if (_BleedItem == null && _UserContext != null)
        {
            _BleedItem = _UserContext.GetComponentInChildren<BleedItemLogic>();
        }

        if (_BleedItem != null)
        {
            isBleedActive = true;
        }

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

        // efecto del bleed
        ApplyBleedEffect(HPComp);

        HPComp.TakeDamageWithKnockback(Dir,KnockBack,damageScore);

        // notifico al flash sobre el bleed
        EnemyDamageFlash damageFlash = HPComp.GetComponent<EnemyDamageFlash>();
        if (damageFlash != null)
        {
            damageFlash.SetBleedStatus(isBleedActive);
        }

        SlowMotion.Stop(0.2f, 0.05f, false);
    }

    // efecto del bleed
    private void ApplyBleedEffect(Health_Component targetHP)
    {
        // chequeo ref del item
        if (_BleedItem == null && _UserContext != null)
        {
            // busco la logica para evitar null
            _BleedItem = _UserContext.GetComponentInChildren<BleedItemLogic>();
        }

        if (_BleedItem != null && _BleedItem.Stacks > 0)
        {
            // llamo a func en hpcomponent para hacer el bleed
            targetHP.ApplyBleed(
            _BleedItem.GetBleedDamage(),
            _BleedItem.GetBleedDuration(),
            _BleedItem.GetBleedTickRate(),
            _UserContext
            );
        }
    }
}
