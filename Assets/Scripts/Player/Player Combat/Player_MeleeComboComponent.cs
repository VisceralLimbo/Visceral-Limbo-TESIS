using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player_MeleeComboComponent : Player_MeleeAttack
{
    [Header("References")]
    [SerializeField] AnimatorHandler _AnimHandler;
    [SerializeField] Visceral_WeaponBase _Weapon;
    [SerializeField] private PlayerContext _PlayerContext;
    public event Action OnMeleeAttackCompleted;

    private Animator _Anim;

    [Header("Attack Animations (Combo)")]
    public Player_AttackComboLoadoutSO Combo;

    [Header("Variables")]
    public float ComboResetTime = 1.2f;
    private float _lastAttackTime;
    private int _ComboCounter;
    private bool _HasFinishedAttack = true;

    [Header("Stats")]
    [SerializeField] private StatIdentifier _baseDamageStat;
    [SerializeField] private StatIdentifier _attackSpeedStat;

    public override void VS_Initialize()
    {
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.OnDialogueStart += sheateWeapon;
            DialogueManager.instance.OnDialogueEnd += UnsheateWeapon;
        }
        _AnimHandler.TryGetAnimator("PlayerWeapon", out Animator _Anima);
        _Anim = _Anima;

        print("Animator gotten");
    }

    public override void VS_Runlogic(params object[] a)
    {
        if (a == null || a.Length == 0) return;
        RunData((InputMovement)a[0]);
    }

    public void RunData(InputMovement PlayerInputs)
    {
        if (_HasFinishedAttack && Time.time - _lastAttackTime > ComboResetTime)
        {
            _ComboCounter = 0;
        }

        if (PlayerInputs.SustainedLeftMouseClick && _HasFinishedAttack)
        {
            _HasFinishedAttack = false;
            _lastAttackTime = Time.time;
            StartCoroutine(AttackHandle());
        }
    }

    private IEnumerator AttackHandle()
    {
        float currentDamageMultiplier = _PlayerContext?.Stats?.GetFloatStatValue(_baseDamageStat) ?? 1f;
        float attackSpeedMod = _PlayerContext?.Stats?.GetFloatStatValue(_attackSpeedStat) ?? 1f;

        if (_ComboCounter >= Combo.ComboSequence.Length) _ComboCounter = 0;

        var currentAttack = Combo.ComboSequence[_ComboCounter];

     
        _Weapon.Damage = currentAttack.Damage + currentDamageMultiplier;
        _Weapon.KnockBack = currentAttack.KnockBack;

        _Anim.speed = attackSpeedMod * TimeDilationManager.GlobalTimeScale;


        _AnimHandler.SetParameter("PlayerWeapon", "AttackID", AnimatorControllerParameterType.Int, currentAttack.HashedID);
        _AnimHandler.SetParameter("PlayerWeapon", "Attack", AnimatorControllerParameterType.Trigger);

        float startTime = (float)currentAttack.StartDealingDamageFrame / currentAttack.AnimFrameRate;
        float damageDuration = ((float)currentAttack.EndDealingDamageFrame - currentAttack.StartDealingDamageFrame) / currentAttack.AnimFrameRate;
        float remainingTime = ((float)currentAttack.AnimationLenght - currentAttack.EndDealingDamageFrame) / currentAttack.AnimFrameRate;

        yield return new WaitForSeconds(startTime / attackSpeedMod);

        // Disparamos el evento para que las visuales (trails, sonidos) reaccionen
        PlayerEvents.StartAttacking();
        _Weapon.Attacking();

        yield return new WaitForSeconds(damageDuration / attackSpeedMod);

        _Weapon.StopAttacking();
        PlayerEvents.EndAttacking();
        OnMeleeAttackCompleted?.Invoke();

        yield return new WaitForSeconds(remainingTime / attackSpeedMod);

        _ComboCounter++;
        _HasFinishedAttack = true;
    }

    void sheateWeapon() => _Weapon.gameObject.SetActive(false);
    void UnsheateWeapon() => _Weapon.gameObject.SetActive(true);



}
