using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DamageDispatcher
{

    /// <summary>
    /// Procesa instancias de danio, devuelve True si hizo danio correctamente.
    /// </summary>
    /// <param name="HitCollider"> el collider a analizar</param>
    /// <param name="DMScore">el damagescore del ataque </param>
    /// <param name="KnockbackDir"></param>
    /// <param name="KnockbackForce"></param>
    /// <param name="HitCache"> el cache de elementos daniados</param>
    /// <param name="ProcOnHit"> player: si este evento deberia proquear items "on hit"</param>
    /// <returns></returns>
    public static bool ProcessSingleHit(Collider HitCollider
        , ref DamageScore DMScore
        , Vector3? KnockbackDir
        , float KnockbackForce
        , HashSet<Health_Component> HitCache
        , bool ProcOnHit = false)
    {
        // tratamos de obtener el healthComponent y el Idamageable
        if(TryExtractHealth(HitCollider,out IDamageable Damageable,out Health_Component HPComp))
        {
            // si podemos aniadir el HPComponent al hitcache, ejecutamos danio
            if (HitCache.Add(HPComp))
            {
                return ExecuteDamage(Damageable, HPComp,ref DMScore, KnockbackDir, KnockbackForce, ProcOnHit);
            }

        }
        return false;
    }
    
    public static bool ProcessContinuousHit
        (
            Collider HitCollider,
            ref DamageScore DMScore,
            Vector3? KnockbackDir,
            float KnockbackForce,
            Dictionary<Health_Component, float> cooldownTimer,
            float cooldownTime,
            bool ProcOnHit = false
            
        )
    {
        // obtenemos health y Idamage del collider
        if(TryExtractHealth(HitCollider, out IDamageable IDamage, out Health_Component HPComp))
        {

            // tratamos de obtener el damageCooldown usando nuestro HPComp
            if (cooldownTimer.TryGetValue(HPComp, out float LastTime))
            {

                if(Time.time - LastTime < cooldownTime)
                {
                    return false;
                }
            }
            cooldownTimer[HPComp] = Time.time;

            return ExecuteDamage(IDamage, HPComp, ref DMScore, KnockbackDir, KnockbackForce, ProcOnHit);
        }

        return false;

    }


    private static bool TryExtractHealth(Collider Col, out IDamageable IDamage, out Health_Component HPComp)
    {
        IDamage = null;
        HPComp = null;


        // prioridad IDamageable
        if(Col.TryGetComponent(out IDamageable Damage))
        {
            IDamage = Damage;
            bool HasHealth =  Damage.GetHealthComponent(out Health_Component hp);
            HPComp = hp;
            return HasHealth;
        }

        // fallback HealthComponent => potencialmente caro!
        if(Col.TryGetComponent(out Health_Component HPComponent))
        {
            HPComp = HPComponent;
            Damage = HPComp as IDamageable;
            return HPComp != null;
        }

        return false;
    }

    private static bool ExecuteDamage
        (
            IDamageable IDamage,
            Health_Component HP_Comp,
            ref DamageScore DMScore,
            Vector3? KnockbackDir,
            float KnockbackForce,
            bool OnProcHit
        )
    {

        if(HP_Comp.Context != null && DMScore.Attacker != null)
        {
            if(HP_Comp.Context == DMScore.Attacker)
            {
                // evitar doble hit
                return false;
            }
        }



        DMScore.Victim = HP_Comp.Context;


        if (DMScore.Victim != null)
        {
            if(DMScore.Victim == DMScore.Attacker)
            {
                // evitar self hit
                return false;
            }
        }

        // si proqueamos efectos y nuestro atacker no es null
        if(OnProcHit && DMScore.Attacker != null)
        {
            // si nuestro attacker es de la faccion player
            if(DMScore.Attacker.faction == FactionID.Player)
            {
                DMScore.Attacker.Inventory.ProcOnHitEffects(DMScore.Attacker, DMScore, HP_Comp);
            }
        }


        // caso a) golpeamos algo que no tiene context, ejemplo: props
        if(HP_Comp.Context == null)
        {
            HP_Comp.SimpleDamage(DMScore.DamageAmount);
            PlayerEvents.PlayerSucessfulHit();
            return true;
        }
        // caso b) priorizamos IDamageable interface

        // B.1) tenemos knockback
        if (KnockbackDir.HasValue)
        {
            
            if (IDamage != null)
            {
                IDamage.TakeDamageWithKnockback(KnockbackDir.Value, KnockbackForce, DMScore);
            }
            else
            {
                HP_Comp.TakeDamageWithKnockback(KnockbackDir.Value, KnockbackForce, DMScore);
            }
        }
        else
        {

            if (IDamage != null)
            {
                IDamage.TakeDamage(DMScore);
            }
            else
            {
                HP_Comp.TakeDamage(DMScore);
            }
        }
       
        PlayerEvents.PlayerSucessfulHit();
        return true; 
    }


}
