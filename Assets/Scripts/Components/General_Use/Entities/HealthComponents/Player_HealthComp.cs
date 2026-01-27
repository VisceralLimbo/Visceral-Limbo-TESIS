using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_HealthComp : Health_Component
{
    [SerializeField] SoundData _LowHP;

    [Header("Stats")]
    [SerializeField] StatIdentifier _MaxHealthID;

    private void Start()
    {
       
        // setteamos que la salud máxima es la del stat system.
        MaxHealth = _Context.Stats.GetFloatStatValue(_MaxHealthID);

        //suscribimos a stats cuando se cambian
        _Context.Stats.OnStatChanged += UpdateStatValues;

        CurrentHealth = MaxHealth;

        _RB = GetComponent<Rigidbody>();
        _Context = GetComponentInParent<PlayerContext>();

        OnDamaged += updateHealthBar;
        OnHealed += updateHealthBar;  //se suscribe para curación igual q arriba para el damage xdxd
    }

    public override void HealHP(float ExtraHP, bool OverHeal = false)
    {
        base.HealHP(ExtraHP, OverHeal);

        if (CurrentHealth > MaxHealth * 0.3F && Emit != null && Emit.isActiveAndEnabled)
        {
            SoundManager.Instance?.ReturnToPool(Emit);
            Emit = null;
            return;
        }



        //esto es mas facil y te evitas el evento si no lo queres (es lo que haces en updatestatvalue) lo hice para seguir tu logica pero hace la q pinte 
        //updateHealthBar();
    }

    public override void SimpleDamage(float Damage)
    {
        base.SimpleDamage(Damage);
    }

    public override void SimpleDamage(Tuple<Vector3, float, float> MyTuple)
    {
        base.SimpleDamage(MyTuple);
    }

    public override void TakeDamage(DamageScore DamageDT)
    {
        base.TakeDamage(DamageDT);
    }

    public override void TakeDamageWithKnockback(Vector3 Direction, float knockback, DamageScore DamageDT)
    {
        base.TakeDamageWithKnockback(Direction, knockback, DamageDT);
    }

    SoundEmitter Emit;
    protected override void InternalDamage(float damage, Vector3? KnockbarDir, float force, DamageScore Score = null)
    {
        if (CurrentHealth - damage < MaxHealth * 0.3f && SoundManager.Instance != null)
        {
            if (Emit == null || !Emit.isActiveAndEnabled)
            {
                SoundManager.Instance.CreateSound()
                    .WithSoundData(_LowHP)
                    .WithRandomPitch(true)
                    .WithPosition(_Context.PlayerTransform.position)
                    .play(out SoundEmitter Emitter);
                Emit = Emitter;
                return;
            }
        }
        base.InternalDamage(damage, KnockbarDir, force, Score);

       
    }


    private void updateHealthBar()
    {
            Combat_UI_Manager._Instance.UpdatePlayerHealthBar(CurrentHealth, MaxHealth);
            if (CurrentHealth <= 0)
            {
                Combat_UI_Manager._Instance.DisplayLose(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void UpdateStatValues(StatIdentifier statID, float values)
    {
        if (statID == _MaxHealthID)
        {
            MaxHealth = values;
            Combat_UI_Manager._Instance.UpdatePlayerHealthBar(CurrentHealth, MaxHealth, false);
        }
    }


}
