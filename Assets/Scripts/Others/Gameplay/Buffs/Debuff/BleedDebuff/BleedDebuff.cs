using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BleedDebuff : BuffBehavior
{
    [Header("Variables")]
    [Tooltip("Controla cuanto tiempo pasa entre proc. de daño")]
    [SerializeField] float _TicksPerProc = 0.5f;
    [Tooltip("Controla cuanto daño se recibe al proc.")]
    [SerializeField] float _DamagePerProc = 2f;


    [Space]
    [SerializeField] Health_Component _Hp;
    public override void OnAddPotency(int ExtraPotency)
    {
        _BuffPotency += ExtraPotency;
    }

    public override void OnApply(BuffManager manager, StatsManager StatMan)
    {
       if(_Hp == null)
       {
            _Hp = manager.HPComponent;
       }

    }

    public override void OnExpire()
    {
        // vacio ya que por ahora no tengo efectos a suscribir, creo
    }

    float pulse = 0;
    public override void OnUpdate(float deltaTime)
    {
        if(pulse < _TicksPerProc)
        {
            pulse += deltaTime * TimeDilationManager.GlobalTimeScale;
            return;
        }
        pulse = 0;



        DamageScore DMS = new DamageScore
        {
            Attacker = Origin,
            DamageAmount = _DamagePerProc * _BuffPotency,
            ElementalDamage = ElementType.Physical,
            IsAirBorneKill = false,
            FactionID = Origin.faction,
        };

        print("Damage dealt by DMS" + DMS.DamageAmount + " Damage calculated " + _DamagePerProc * _BuffPotency);

        _Hp.TakeDamage(DMS);
    }
}
