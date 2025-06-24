using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Exec_Skill_Whirlwind : Visceral_SkillLogic
{
    // _UserContext => PlayerContext

    [SerializeField] AnimatorOverrideController _ANCO;
    [SerializeField] Animator _Anim;
    [SerializeField] Collider _col;

    [SerializeField] float SkillDuration;
    [SerializeField] float Damage;
    [SerializeField]  VisualEffect _effectHability;

    [SerializeField] SoundEmitter _SoundEmit;
    public override void Initialize(Visceral_AbilitySO data, Visceral_SkillManager Skmanager, PlayerContext UserContext = null)
    {
        base.Initialize(data, Skmanager, UserContext);

        _Anim = _UserContext.PlayerGameObject.transform.root.GetComponentInChildren<Animator>();

    }


    public override void ActivateSkill()
    {
        _Anim.speed = SkillSpeedMod;
        _Anim.runtimeAnimatorController = _ANCO;
        _Anim.Play("Exe_Skill1", 0, 0);

        _effectHability.Play();

        StartCoroutine(LockSkill());



        //sonidos
        SoundManager.Instance.CreateSound()
        .WithSoundData(_SoundDataList[0])
        .WithRandomPitch(true)
        .WithPosition(_UserContext.PlayerTransform.position)
        .play(out SoundEmitter emitter);

        _SoundEmit = emitter;
    }

    IEnumerator LockSkill()
    {
        var Weapon = _UserContext.PlayerTransform.root.GetComponentInChildren<Visceral_WeaponBase>();
        Weapon.Damage = Damage;
        Weapon.Attacking();


        yield return new WaitForSeconds(SkillDuration);
        while (_Anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.9f)
        {
            yield return null;
        }
        _Anim.SetTrigger("Skill1Trigger");
        _Anim.speed = 1.0f;
        Weapon.StopAttacking();
        SoundManager.Instance.ReturnToPool(_SoundEmit);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.name == _UserContext.name) return;
        if (other.gameObject == _UserContext.PlayerGameObject) return;
        if (other.GetComponent<PlayerContext>() == _UserContext) return;

        if (other.TryGetComponent(out Health_Component HPComp))
        {
            Vector3 dir = other.gameObject.transform.position - this.transform.position;

            DamageScore DamageDT = new DamageScore();
            DamageDT.Attacker = _UserContext;
            DamageDT.DamageAmount = Damage;
            DamageDT.Victim = other.GetComponent<PlayerContext>();
            DamageDT.ElementalDamage = ElementType.Physical;
            DamageDT.FactionID = FactionID.LimboMonster1;
            DamageDT.AddTag(ScoreFlags.Skill1Kill);

            if (HPComp.Context == null) { HPComp.SimpleDamage(Damage); return; }
            HPComp.TakeDamageWithKnockback(dir.normalized, 5, DamageDT);

        }
    }
}
