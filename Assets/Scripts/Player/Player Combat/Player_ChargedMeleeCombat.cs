using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_ChargedMeleeCombat : Visceral_Script
{
    public SwordTestSO[] SwordAttacks;
    [SerializeField] float _ChargeAmount,_MaximumCharge,_MinimunToAttack;
    int _ComboCounter;

    [SerializeField] Animator _Anim;
    [SerializeField] Visceral_WeaponBase _Weapon;
    [SerializeField] Slider _ChargeSlider;

    /// <summary>
    /// valor que cambia la velocidad de animacion de ataque, valor 1 = normal
    /// </summary>
    public float AttackSpeedMod = 1f;

    /// <summary>
    /// valor que cambia la velocidad de recarga de ataque
    /// </summary>
    public float RechargeRate;

    InputMovement PlayerInputs;

    public override void VS_Initialize()
    {
        _Weapon.EndAttack += FinishAttack;

        var PlBS=GetComponent<Player_Base>();

        PlBS.OnPlayerSkillUse += ResetAnimation;
    }

    public override void VS_Runlogic(params object[] a)
    {
        if (a == null || a.Length == 0)
        {
            Debug.LogError("PlayerChargeAttackScript recieving null parameters");
            return;
        }
        PlayerInputs = (InputMovement)a[0];

        CancelInvoke(nameof(FinishAttack));

        if (PlayerInputs.SustainedLeftMouseClick && !_Anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            _ChargeAmount += Time.deltaTime; 
            if(_ChargeAmount> _MaximumCharge)
            {
                _ChargeAmount= _MaximumCharge;
                //poner aca la animacion de carga.
            }
        }
        else
        {
            _ChargeAmount -= Time.deltaTime;
            if(_ChargeAmount <= 0) _ChargeAmount = 0;
        }

        if(_ChargeAmount > _MinimunToAttack && PlayerInputs.ReleasedLeftMouseClick)
        {
            Attack();
            _ChargeAmount = 0;
        }

        Invoke(nameof(FinishAttack), 0);

        UpdateUI();
    }

    private void Attack()
    {
        
        _Anim.runtimeAnimatorController = SwordAttacks[_ComboCounter]._AnimatorOV;
        _Anim.speed = AttackSpeedMod;
        _Anim.Play("Attack", 0, 0);
        _Weapon.Damage = SwordAttacks[_ComboCounter].Damage;
        _Weapon.KnockBack = SwordAttacks[_ComboCounter].KnockBack;
        _Weapon.Attacking();
        _ComboCounter++;
        if (_ComboCounter + 1 > SwordAttacks.Length)
        {
            _ComboCounter = 0;
        }

    }

    public void FinishAttack()
    {
        if(_Anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack") && _Anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f)
        {
            _Weapon.StopAttacking();
            _Anim.SetTrigger("AttackTrigger");
            Debug.Log("finishing attack");
        }
    }

    private void ResetAnimation()
    {
        _ComboCounter= 0;
    }

    private void UpdateUI()
    {
        if(_ChargeAmount <= 0)
        {
            _ChargeSlider.gameObject.SetActive(false);
        }
        else
        {
            _ChargeSlider.value = _ChargeAmount / _MaximumCharge;
            _ChargeSlider.gameObject.SetActive(true);
        }
    }

}
