using UnityEngine;
using System.Collections.Generic;

public class DamageZone : MonoBehaviour
{
    // la zona de daño del dash
    private PlayerContext _SourceContext;
    private float _DamageAmount;
    private float _TickRate;
    private FactionID _SourceFaction = FactionID.Player;

    // casi misma logica q spiketrap
    private Dictionary<Health_Component, float> _LastDamageTime = new Dictionary<Health_Component, float>();

    // se llama en dashzoneitemlogic cuando se instancai
    public void SetupZone(PlayerContext context, float baseDamage, float tickRate, float duration)
    {
        _SourceContext = context;
        _DamageAmount = baseDamage;
        _TickRate = tickRate;

        if (context != null)
        {
            _SourceFaction = context.faction;
        }
        Destroy(gameObject, duration);
    }


    private void OnTriggerStay(Collider other)
    {
        // no hay contexto chau
        if (_SourceContext == null) return;

        // agarro hp
        if (other.TryGetComponent(out Health_Component HPComp))
        {
            // q no me haga daño a mi
            if (HPComp.Context == _SourceContext) return;

            float lastTime = 0f;
            _LastDamageTime.TryGetValue(HPComp, out lastTime);

            // tiempo actual < q ultimo tiempo de daño y el intervalo, chau
            if (Time.time < lastTime + _TickRate) return;

            PlayerContext victimCtx = HPComp.Context;

            // dmgscore
            DamageScore dmg = new DamageScore
            {
                Attacker = _SourceContext,
                Victim = victimCtx,
                DamageAmount = _DamageAmount,
                ElementalDamage = ElementType.Physical,
                FactionID = _SourceFaction
            };

            // daño simple
            if (victimCtx != null)
            {
                HPComp.TakeDamageWithKnockback(Vector3.zero, 0, dmg);
            }
            else
            {
                HPComp.SimpleDamage(_DamageAmount);
            }
            _LastDamageTime[HPComp] = Time.time;
        }
    }
}