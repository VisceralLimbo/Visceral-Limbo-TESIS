using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class Health_Component : Visceral_Component
{
    [SerializeField] SoundData soundData;

    public float CurrentHealth, MaxHealth;
    public Rigidbody _RB;
    public bool DestroyOnDeath,DesactivateOnDeath,Died;
    [SerializeField] PlayerContext _Context;
    public PlayerContext Context { get { return _Context; } }

    public event Action OnDeath, OnDamaged;
    public event Action<Vector3, float> OnKnockbackTaken;

    //public ParticleSystem bloodParticles;

 

    private void Start()
    {
        if(_Context?.faction == FactionID.Player)
        {
            MaxHealth = _Context.Stats.GetFloatStatValue("MaxHealth");
            _Context.Stats.OnStatChanged += UpdateStatValues;
        }

        CurrentHealth = MaxHealth;
        _RB= GetComponent<Rigidbody>();
        _Context= GetComponentInParent<PlayerContext>();

        if (_Context?.faction == FactionID.Player) OnDamaged += updateHealthBar;
            
    }

    public void TakeDamage(DamageScore DamageDT)
    {
        InternalDamage(DamageDT.DamageAmount, null, 0, DamageDT);

    }

    public void TakeDamageWithKnockback(Vector3 Direction,float knockback,DamageScore DamageDT)
    {
        InternalDamage(DamageDT.DamageAmount,Direction, knockback, DamageDT);
    
    }

    public void SimpleDamage(float Damage)
    {
        InternalDamage(Damage, null, 0, null);

    }

    //use of tuples - patricio malvasio
    /// <summary>
    /// vector3 : la direccion del ataque
    /// float 1 : el daño del ataque
    /// float 2 : el knockback del ataque
    /// </summary>
    /// <param name="MyTuple"></param>
    public void SimpleDamage(Tuple<Vector3,float,float> MyTuple)
    {
        InternalDamage(MyTuple.Item2, MyTuple.Item1, MyTuple.Item3, null);

    }


    private void InternalDamage(float damage, Vector3? KnockbarDir, float force, DamageScore Score = null)
    {
        if (Died) return;

        CurrentHealth -= damage;
        CameraShake.instance.ShakeCamera(0.5f, 0.5f, ShakeType.Horizontal);
        OnDamaged?.Invoke();

        if (soundData != null)
        {
            PlaySounds(); // feedback de sonidos
        }

        
        // better implementation, why the F should the HPComp even know whats knockbackeable
        // use a public Event, that way the HPComp doenst know who needs it
        // seriously, why pato? - patoh
        if( OnKnockbackTaken != null && KnockbarDir.HasValue)
        {
            OnKnockbackTaken.Invoke(KnockbarDir.Value, force);
        }
        else if (KnockbarDir.HasValue && _RB != null)
        {
            print("rigidbody recieving knockback");
            _RB.AddForce(KnockbarDir.Value * force, ForceMode.Impulse);
        }


        if (CurrentHealth <= 0f && _Context != null && Score != null && Score.Attacker != null)
        {
            Died = true;
            DamageScore FinalScore = Score != null
                ? DamageScoreBuilder.Complete(Score, _Context, CurrentHealth, MaxHealth) : null;

            if (ScoreManager.Instance != null)
                ScoreManager.Instance.ProcessKill(FinalScore);
            OnDeath?.Invoke();

            

            if (DesactivateOnDeath) _Context.PlayerGameObject.SetActive(false);
            if (DestroyOnDeath) Destroy(_Context.PlayerGameObject);
        }
        else if (CurrentHealth <= 0f)
        {
            OnDeath?.Invoke();

            Died = true;
            if (_Context == null)
            {
                if (DesactivateOnDeath) gameObject.SetActive(false);
                if (DestroyOnDeath) Destroy(gameObject);
            }
            else
            {
                if (DesactivateOnDeath) _Context.PlayerGameObject.SetActive(false);
                if (DestroyOnDeath) Destroy(_Context.PlayerGameObject);
            }
        }
    }


    /// <summary>
    /// Funcion para curar al Componente
    /// </summary>
    /// <param name="ExtraHP">valor de curacion</param>
    /// <param name="OverHeal">Si esta curacion puede curar más que la vida maxima.
    /// si esta en falso,la vida sera clampeada a la maxima salud posible</param>
    public void HealHP(float ExtraHP,bool OverHeal = false)
    {
        if (OverHeal)
        {
            CurrentHealth += ExtraHP;
        }
        else
        {
            CurrentHealth += ExtraHP;
            if(CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }
        }


        if(_Context.faction == FactionID.Player)
        {
            updateHealthBar();
        }
    }

    void PlaySounds()
    {
        if (soundData == null || Context == null) return;
        var SoundCLip = SoundManager.Instance.CreateSound();
        SoundCLip.WithSoundData(soundData);
        SoundCLip.WithRandomPitch(true);
        SoundCLip.WithPosition(Context.transform.position);
        SoundCLip.WithSpatialBlend(1);
        SoundCLip.play();
    }

    private void updateHealthBar()
    {
        if(_Context.faction == FactionID.Player)
        {
            Combat_UI_Manager._Instance.UpdatePlayerHealthBar(CurrentHealth,MaxHealth);
            if(CurrentHealth <= 0)
            {
                Combat_UI_Manager._Instance.DisplayLose(true);
            }
        }
    }

    private void UpdateStatValues(string statID,float values)
    {
        if(statID == "MaxHealth")
        {
            MaxHealth = values;
            Combat_UI_Manager._Instance.UpdatePlayerHealthBar(CurrentHealth,MaxHealth,false);
        }
    }

}
