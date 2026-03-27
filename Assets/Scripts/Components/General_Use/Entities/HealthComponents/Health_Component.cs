using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using Random = UnityEngine.Random;


public class Health_Component : Visceral_Component, IDamageable
{
    [SerializeField] protected SoundData[] soundData;
    [SerializeField] protected SoundData[] HealingSoundData;    

    public float CurrentHealth, MaxHealth;

    [Tooltip("Damage Reduction es un reductor porcentual del daño entrante que se aplica tras la reduccion directa de defensa (ej: 100 daño - (100 * 0.5f)) = 50 daño entrante")]
    [SerializeField] protected float DamageReduction;

    [Tooltip("Defense es un reductor directo al daño entrante que se aplica antes de la reduccion porcentual (ej: 100 daño entrante - 15 defensa = 85 daño total)")]
    [SerializeField] protected float Defense;

    [Tooltip("Damage Invulnerability es un damage windown donde se ignora daño recibido tras recibir daño")]

    [SerializeField] protected float DamageInvulnerability;

    public Rigidbody _RB;
    public bool DestroyOnDeath,DesactivateOnDeath,Died;

    [SerializeField] protected PlayerContext _Context;
    public PlayerContext Context { get { return _Context; } }

    public event Action OnDeath, OnDamaged, OnHealed; //agregue onhealed porque habia evento para todo menos para recibir cura xd
    public event Action<Vector3, float> OnKnockbackTaken;

    private Coroutine bleedCoroutine;

    //[SerializeField] GameObject _RagdollPrefab;

    private void Start()
    {
        CurrentHealth = MaxHealth;
        _RB= GetComponent<Rigidbody>();
        _Context= GetComponentInParent<PlayerContext>();

    }

    /// <summary>
    /// funcion 1 de recibir daño, pide un damagescore para saber que tipo de daño recibio
    /// </summary>
    /// <param name="DamageDT"> clase de dato para comunicar toda la informacion del daño</param>
    public virtual void TakeDamage(DamageScore DamageDT)
    {
        InternalDamage(DamageDT.DamageAmount, null, 0, DamageDT);

    }

    /// <summary>
    /// funcion 1 de recibir daño, pide un damagescore para saber que tipo de daño recibio
    /// a su vez, permite el uso de knockback
    /// </summary>
    /// <param name="Direction"> direccion del ataque</param>
    /// <param name="knockback"> fuerza del knockback</param>
    /// <param name="DamageDT"> clase de dato para comunicar toda la informacion del daño</param>
    public virtual void TakeDamageWithKnockback(Vector3 Direction,float knockback,DamageScore DamageDT)
    {
        InternalDamage(DamageDT.DamageAmount,Direction, knockback, DamageDT);
    
    }

    /// <summary>
    /// funcion de daño puro, usar con cuidado porque no hace interfaz con varios sistemas
    /// </summary>
    /// <param name="Damage"> daño puro</param>
    public virtual void SimpleDamage(float Damage)
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
    public virtual void SimpleDamage(Tuple<Vector3,float,float> MyTuple)
    {
        InternalDamage(MyTuple.Item2, MyTuple.Item1, MyTuple.Item3, null);

    }


    /// <summary>
    /// funcion de daño interna, procesa los datos recibidos y realiza el daño
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="KnockbarDir"></param>
    /// <param name="force"></param>
    /// <param name="Score"></param>
    protected virtual void InternalDamage(float damage, Vector3? KnockbarDir, float force, DamageScore Score = null)
    {
        if (Died) return;

        // primero aplicamos defensa
        float DamageAfterDefense = damage - Defense;

        // a mi parecer, la defensa no debe de ser capaz de completamente anular el daño entrante.
        // en caso de anular daño, en su lugar el jugador va a recibir 1 de daño.
        if (DamageAfterDefense > 1)
        {
            // ahora aplicamos reduccion de daño
            float FinalDamage = DamageAfterDefense - (DamageAfterDefense * (DamageReduction/100));
            int FinalEffectiveDamage = (int)FinalDamage;

            if(FinalDamage >= 1)
            {
                CurrentHealth -= FinalEffectiveDamage;
            }
            else
            {
                CurrentHealth -= 1;
            }
        }
        else
        {
                CurrentHealth -= 1;
        }

        OnDamaged?.Invoke();

        if (soundData.Length > 0)
        {
            PlaySounds(); // feedback de sonidos
        }


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
            DamageScore FinalScore = Score != null
                ? DamageScoreBuilder.Complete(Score, _Context, CurrentHealth, MaxHealth) : null;

            if (ScoreManager.Instance != null)
                ScoreManager.Instance.ProcessKill(FinalScore);
            OnDeath?.Invoke();
            StartCoroutine(DeathCoroutine());
        }
        else if (CurrentHealth <= 0f)
        {
            OnDeath?.Invoke();
            StartCoroutine(DeathCoroutine());
        }
    }

    /// <summary>
    /// empujamos la desactivacion del death HASTA el end del frame
    /// de esta manera le dejamos tiempo a que todos los scripts necesarios puedan procesar
    /// la subscripcion del death.
    /// </summary>
    /// <returns></returns>
    IEnumerator DeathCoroutine()
    {

        Died = true;

        yield return new WaitForEndOfFrame();
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

        yield return null;
    }


    /// <summary>
    /// Funcion para curar al Componente
    /// </summary>
    /// <param name="ExtraHP">valor de curacion</param>
    /// <param name="OverHeal">Si esta curacion puede curar más que la vida maxima.
    /// si esta en falso,la vida sera clampeada a la maxima salud posible</param>
    public virtual void HealHP(float ExtraHP,bool OverHeal = false)
    {
        float oldHP = CurrentHealth; // guardo la vida vieja por que si agarro la cura tendria una nueva

        if (OverHeal)
        {
            CurrentHealth += ExtraHP;
        }
        else
        {
            CurrentHealth += ExtraHP;
            if (CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }
        }

        // hago el evento si la vida actual es mayor que la vieja. (osea q recibio cura xd 1+1=2) lo hago por fuera del overheal porque si no se bugea 
        if (CurrentHealth > oldHP)
        {
            OnHealed?.Invoke();
        }

        if(HealingSoundData.Length > 0)
        {
            if(HealingSoundData.Length > 1)
            {
                int Sneed = Random.Range(0, HealingSoundData.Length);

                SoundManager.Instance?.CreateSound()
                .WithSoundData(HealingSoundData[Sneed])
                .WithPosition(this.transform.position)
                .WithRandomPitch(true)
                .play();
            }
            else
            {
                SoundManager.Instance?.CreateSound()
               .WithSoundData(HealingSoundData[0])
               .WithPosition(this.transform.position)
               .WithRandomPitch(true)
               .play();
            }
        }

    }

    protected void PlaySounds()
    {
        if (soundData == null || Context == null) return;
        if (soundData.Length == 0) { return; }
        int Sneed;
        if (soundData.Length == 1) Sneed = 0;
        else
        {
            Sneed = Random.Range(0, soundData.Length);
        }

        var SoundCLip = SoundManager.Instance.CreateSound();
        SoundCLip.WithSoundData(soundData[Sneed]);
        SoundCLip.WithRandomPitch(true);
        SoundCLip.WithPosition(Context.transform.position);
        SoundCLip.WithSpatialBlend(soundData[Sneed].SpatialBlend, soundData[Sneed].MinimunSoundDistance, soundData[Sneed].MaximunSoundDistance);
        SoundCLip.play();
    }

  

    void IDamageable.TakeDamageWithKnockback(UnityEngine.Vector3 KnockbackDir, float KnockbackForce, DamageScore DamageDT)
    {
        TakeDamageWithKnockback(KnockbackDir,KnockbackForce, DamageDT);
    }

    void IDamageable.TakeDamage(DamageScore DamageDT)
    {
        TakeDamage(DamageDT);
    }

    public bool GetHealthComponent(out Health_Component HPComp)
    {

        HPComp = this;
        return true;
    }

}
