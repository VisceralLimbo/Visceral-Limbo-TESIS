using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Health_Component : Visceral_Component
{
    public float CurrentHealth, MaxHealth;
    public Rigidbody _RB;
    public bool DestroyOnDeath,DesactivateOnDeath,Died;
    [SerializeField] PlayerContext _Context;
    public PlayerContext Context { get { return _Context; } }

    public event Action OnDeath, OnDamaged;

    //public ParticleSystem bloodParticles;

    private void Start()
    {
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

   
    private void InternalDamage(float damage,Vector3? KnockbarDir, float force, DamageScore Score = null)
    {
        if (Died) return;

        CurrentHealth -= damage;
        OnDamaged?.Invoke();

        // tengo un kccmotor con rigidbody?
        if(KnockbarDir.HasValue && _Context.KCCMotor?.AttachedRigidbody != null)
        {
            _Context.KCCMotor.AttachedRigidbody.AddForce(KnockbarDir.Value * force,ForceMode.Impulse);
        }
        else if( KnockbarDir.HasValue && _RB != null)
        {
            _RB.AddForce(KnockbarDir.Value * force, ForceMode.Impulse);
        }

        if(CurrentHealth <= 0f && _Context != null)
        {
            Died = true;
            DamageScore FinalScore = Score != null
            ? DamageScoreBuilder.Complete(Score, _Context,CurrentHealth,MaxHealth) : null;

            ScoreManager.Instance.ProcessKill(FinalScore);
            OnDeath?.Invoke();
            if(DesactivateOnDeath) _Context.PlayerGameObject.SetActive(false);
            if(DestroyOnDeath) Destroy(Context.PlayerGameObject);

        }
        else if(CurrentHealth <= 0f)
        {
            if (DesactivateOnDeath) this.gameObject.SetActive(false);
            if (DestroyOnDeath) Destroy(this.gameObject);
        }
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

}
