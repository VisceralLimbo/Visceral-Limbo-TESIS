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
    [SerializeField] float SkillKnockback;
    [SerializeField]  GameObject _effectHability;

    [SerializeField] SoundEmitter _SoundEmit;
    public override void Initialize(Visceral_AbilitySO data, Visceral_SkillManager Skmanager, PlayerContext UserContext = null)
    {
        base.Initialize(data, Skmanager, UserContext);

        _Anim = _UserContext.PlayerGameObject.transform.root.GetComponentInChildren<Animator>();

    }


    public override void ActivateSkill()
    {
        _Anim.speed = SkillSpeedMod;
        _Anim.SetTrigger("Exe_Skill1");

        Vector3 offsetPos = _UserContext.PlayerGameObject.transform.position + Vector3.up * 0.1f; // cambiá el 1.0f según lo alto que lo quieras
        GameObject vfx = Instantiate(_effectHability, offsetPos, Quaternion.identity, _UserContext.PlayerGameObject.transform);

        //activo bloqueo
        _PlayerBase.SetSkillActiveState(true);

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
        //var Weapon = _UserContext.PlayerTransform.root.GetComponentInChildren<Visceral_WeaponBase>();
        //Weapon.Damage = Damage;
        //Weapon.Attacking();
        _col.enabled = true;
        _col.transform.position = _UserContext.PlayerTransform.position;

        yield return new WaitForSeconds(SkillDuration);
        _Anim.speed = 1.0f;
        //Weapon.StopAttacking();
        SoundManager.Instance.ReturnToPool(_SoundEmit);
        _col.enabled = false;
        // desactivo bloqueo
        _PlayerBase.SetSkillActiveState(false);

    }


    private void OnTriggerEnter(Collider other)
    {
        print("Detected Enemy");

        if (other.name == _UserContext.name) return;
        if (other.gameObject == _UserContext.PlayerGameObject) return;
        if (other.GetComponent<PlayerContext>() == _UserContext) return;

        if (other.TryGetComponent(out Health_Component HPComp))
        {
            Vector3 dir = other.gameObject.transform.position - _UserContext.PlayerTransform.transform.position;

            DamageScore DamageDT = new DamageScore();
            DamageDT.Attacker = _UserContext;
            DamageDT.DamageAmount = Damage;
            DamageDT.Victim = other.GetComponent<PlayerContext>();
            DamageDT.ElementalDamage = ElementType.Physical;
            DamageDT.FactionID = FactionID.LimboMonster1;
            DamageDT.AddTag(ScoreFlags.Skill1Kill);

            if (HPComp.Context == null) { HPComp.SimpleDamage(Damage); return; }
            HPComp.TakeDamageWithKnockback(dir.normalized,SkillKnockback , DamageDT);

        }
    }
}
