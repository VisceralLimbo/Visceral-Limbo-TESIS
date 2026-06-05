using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Exec_Skill_Whirlwind : Visceral_SkillLogic
{
    // _UserContext => PlayerContext

    [SerializeField] AnimatorOverrideController _ANCO;
    [SerializeField] Animator _Anim;


    [SerializeField] PlayerContext _Context;

    [SerializeField] float SkillAttackRadius;
    [SerializeField] float SkillDuration;
    [SerializeField] float Damage;
    [SerializeField] float SkillKnockback;
    [SerializeField]  GameObject _effectHability;

    [SerializeField] SoundEmitter _SoundEmit;
    [SerializeField] LayerMask _AttackMask;

    //private HashSet<Collider> TaggedColliders = new HashSet<Collider>(); VIEJO
    //private HashSet<Health_Component> TaggedHealth = new HashSet<Health_Component>(); VIEJO

    [SerializeField] private float hitCooldownPerEnemy = 0.5f; //NUEVO

    private Dictionary<Health_Component, float> lastHitTime = new Dictionary<Health_Component, float>(); // NUEVO

    public override void Initialize(Visceral_AbilitySO data, Visceral_SkillManager Skmanager, PlayerContext UserContext = null)
    {
        base.Initialize(data, Skmanager, UserContext);

        _Anim = _UserContext.PlayerGameObject.transform.root.GetComponentInChildren<Animator>();

        _Context = _UserContext;
    }


    public override void ActivateSkill()
    {
        _Anim.speed = SkillSpeedMod;
        _Anim.SetTrigger("Exe_Skill1");

        Vector3 offsetPos = _UserContext.PlayerGameObject.transform.position + Vector3.up * 0.1f; // cambiá el 1.0f según lo alto que lo quieras
        GameObject vfx = Instantiate(_effectHability, offsetPos, Quaternion.identity, _UserContext.PlayerGameObject.transform);

        //activo bloqueo
        _PlayerBase.SetSkillActiveState(true);

        //sonidos
        SoundManager.Instance.CreateSound()
        .WithSoundData(_SoundDataList[0])
        .WithRandomPitch(true)
        .WithPosition(_UserContext.PlayerTransform.position)
        .play(out SoundEmitter emitter);
        _SoundEmit = emitter;

        StartCoroutine(LockSkill());
    }

    IEnumerator LockSkill()
    {
        //TaggedColliders.Clear(); //VIEJO
        //TaggedHealth.Clear(); // VIEJO

        lastHitTime.Clear(); //NUEVO

        float timer = 0;
     
        while(timer < SkillDuration)
        {
            Collider[] hitcolliders = Physics.OverlapSphere(_Context.PlayerTransform.position, SkillAttackRadius, _AttackMask);

            foreach(var HitCol in hitcolliders)
            {
                ProcessHit(HitCol);
            }

            timer += (Time.deltaTime * TimeDilationManager.GlobalTimeScale);

            yield return null;
        }

        _Anim.speed = 1.0f;

        if(_SoundEmit != null && _SoundEmit.isActiveAndEnabled)
        {
            SoundManager.Instance.ReturnToPool(_SoundEmit);
            _SoundEmit = null;
        }

        _PlayerBase.SetSkillActiveState(false);
    }

    private void ProcessHit(Collider other)
    {
        if (other.gameObject == _UserContext.PlayerGameObject) return;
        //if (TaggedColliders.Contains(other)) return; //VIEJO


        // priorizamos el Idamageable
        if(other.TryGetComponent(out IDamageable Idamage))
        {
            //if(Idamage.GetHealthComponent(out Health_Component IHealth) && !TaggedHealth.Contains(IHealth))//VIEJO
            if (Idamage.GetHealthComponent(out Health_Component IHealth) && CanHitHealth(IHealth)) //NUEVO
            {
                // stopgap ! player ataco algo sin Context
                if(IHealth.Context == null)
                {
                    IHealth.SimpleDamage(Damage);
                    PlayerEvents.PlayerSucessfulHit();
                    return;
                }

                if(IHealth.Context != null && IHealth.Context != _Context )
                {
                    //TaggedColliders.Add(other); //VIEJO
                    //TaggedHealth.Add(IHealth); //VIEJO

                    Vector3 Dir = IHealth.Context.PlayerTransform.position - _Context.PlayerTransform.position;
                    Dir.y = 0;


                    DamageScore DamageDT = new DamageScore
                    {
                        Attacker = _UserContext,
                        DamageAmount = Damage,
                        Victim = IHealth.Context, // Puede ser null si es un prop, lo manejamos abajo
                        ElementalDamage = ElementType.Physical,
                        FactionID = FactionID.LimboMonster1
                    };
                    DamageDT.AddTag(ScoreFlags.Skill1Kill);

                    PlayerEvents.PlayerSucessfulHit();
                    IHealth.TakeDamageWithKnockback(Dir.normalized, SkillKnockback, DamageDT);
                    SlowMotion.Stop(0.1f, 0.02f, false);
                }
      


            }


        }
        else if(other.TryGetComponent( out Health_Component HPComp))
        {
            if(HPComp.Context == null)
            {
                HPComp.SimpleDamage(Damage);
                PlayerEvents.PlayerSucessfulHit();
                return;
            }

            if (HPComp.Context != _Context)
            {
                if (!CanHitHealth(HPComp)) return; //NUEVO

                //TaggedColliders.Add(other); //VIEJO
                //TaggedHealth.Add(HPComp); //VIEJO

                Vector3 Dir = HPComp.Context.PlayerTransform.position - _Context.PlayerTransform.position;
                Dir.y = 0;


                DamageScore DamageDT = new DamageScore
                {
                    Attacker = _UserContext,
                    DamageAmount = Damage,
                    Victim = HPComp.Context, // Puede ser null si es un prop, lo manejamos abajo
                    ElementalDamage = ElementType.Physical,
                    FactionID = FactionID.LimboMonster1
                };
                DamageDT.AddTag(ScoreFlags.Skill1Kill);

                if (HPComp.Context == null)
                {
                    HPComp.SimpleDamage(Damage);
                }
                else
                {
                    HPComp.TakeDamageWithKnockback(Dir.normalized, SkillKnockback, DamageDT);
                }


                SlowMotion.Stop(0.1f, 0.02f, false);
            }



        }
    }

    //NUEVO METODO
    private bool CanHitHealth(Health_Component health)
    {
        if (health == null) return false;

        if (lastHitTime.TryGetValue(health, out float lastTime))
        {
            if (Time.time - lastTime < hitCooldownPerEnemy)
                return false;
        }

        lastHitTime[health] = Time.time;
        return true;
    }
}
