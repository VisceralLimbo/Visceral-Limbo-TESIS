using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Exec_Skill_Whirlwind : Visceral_SkillLogic
{
    [Header("Animation & Visuals")]
    [SerializeField] AnimatorOverrideController _ANCO;
    [SerializeField] Animator _Anim;
    [SerializeField] GameObject _effectHability;
    [SerializeField] SoundEmitter _SoundEmit;

    [Header("Skill Parameters")]
    [SerializeField] PlayerContext _Context;
    [SerializeField] float SkillAttackRadius;
    [SerializeField] float SkillDuration;
    [SerializeField] float Damage;
    [SerializeField] float SkillKnockback;
    [SerializeField] LayerMask _AttackMask;
    [SerializeField] private float hitCooldownPerEnemy = 0.5f;

    [Header("Stats Key")]
    [SerializeField] StatIdentifier _Skill1DamageID;
    [SerializeField] StatIdentifier _Skill1DurationID;

    // Buffer pre-asignado para OverlapSphereNonAlloc. 
    // 32 es un buen número mágico, pero súbelo si esperas hordas más densas.
    private Collider[] _hitResults = new Collider[50];

    private Dictionary<Health_Component, float> lastHitTime = new Dictionary<Health_Component, float>();

    public override void Initialize(Visceral_AbilitySO data, Visceral_SkillManager Skmanager, PlayerContext UserContext = null)
    {
        base.Initialize(data, Skmanager, UserContext);
        _Anim = _UserContext.PlayerGameObject.transform.root.GetComponentInChildren<Animator>();
        _Context = _UserContext;

        if(_Context != null)
        {
            if(_Context.Stats != null)
            {
                _Context.Stats.OnStatChanged += UpdateStats;
            }
        }
    }

    public override void ActivateSkill()
    {
        _Anim.speed = SkillSpeedMod;
        _Anim.SetTrigger("Exe_Skill1");

        Vector3 offsetPos = _UserContext.PlayerGameObject.transform.position + Vector3.up * 0.1f;
        GameObject vfx = Instantiate(_effectHability, offsetPos, Quaternion.identity, _UserContext.PlayerGameObject.transform);

        _PlayerBase.SetSkillActiveState(true);

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
     
        lastHitTime.Clear();

        float timer = 0;

        while (timer < SkillDuration)
        {
            // calculo de la cantidad de hits que tuvimos
            int hits = Physics.OverlapSphereNonAlloc(_Context.PlayerTransform.position, SkillAttackRadius, _hitResults, _AttackMask);

            for (int i = 0; i < hits; i++)
            {
                Collider hitCol = _hitResults[i];

                if (hitCol == null || hitCol.gameObject == _UserContext.PlayerGameObject) continue;

                ProcessHit(hitCol);
            }

            timer += (Time.deltaTime * TimeDilationManager.GlobalTimeScale);
            yield return null;
        }

        _Anim.speed = 1.0f;

        if (_SoundEmit != null && _SoundEmit.isActiveAndEnabled)
        {
            SoundManager.Instance.ReturnToPool(_SoundEmit);
            _SoundEmit = null;
        }

        _PlayerBase.SetSkillActiveState(false);
    }

    private void ProcessHit(Collider other)
    {
        print("whirlwinding " + other.name + " root: "+ other.transform.root.name);

        DamageScore DMS = new DamageScore
        {
            Attacker = _UserContext,
            DamageAmount = Damage,
            ElementalDamage = ElementType.Physical,
            FactionID = FactionID.LimboMonster1
        };
        DMS.AddTag(ScoreFlags.Skill1Kill);

        Vector3 knockbackDir = (other.transform.position - _Context.PlayerTransform.position);
        knockbackDir.y = 0;

        // El Dispatcher se encarga de checkear el diccionario y aplicar el cooldown
        bool damageDealt = DamageDispatcher.ProcessContinuousHit
            (
                other,
                ref DMS,
                knockbackDir.normalized,
                SkillKnockback,
                lastHitTime,
                hitCooldownPerEnemy,
                false
            );

        // Si el dispatcher confirma que se aplico danio, disparamos el hitstop visual
        if (damageDealt)
        {
            SlowMotion.Stop(0.1f, 0.02f, false);
        }
    }


    private void UpdateStats(StatIdentifier ID, float Value)
    {
        if(ID == _Skill1DamageID)
        {
            SkillDamageMod = Value;
            Damage *= SkillDamageMod;
        }
        else if(ID == _Skill1DurationID)
        {
            SkillDurationMod = Value;
            SkillDurationMod *= SkillDurationMod;
        }
    }
}
