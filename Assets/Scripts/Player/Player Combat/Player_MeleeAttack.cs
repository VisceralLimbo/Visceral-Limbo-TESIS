using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_MeleeAttack : Visceral_Script
{
    public SwordTestSO[] SwordCombo;
    [SerializeField]float _LastClickedTime;
    [SerializeField] float _LastComboEnd;
    [SerializeField] int _ComboCounter;
    [SerializeField] float _TimeBetweenCombos,_TimebetweenAttacks;

    [SerializeField]Animator _Anim;
    [SerializeField] Visceral_WeaponBase _Weapon;


    public override void VS_Initialize()
    {
        _Anim ??= GetComponent<Animator>();
    }

    public void RunData(InputMovement PlayerInputs)
    {
        if (PlayerInputs.LeftMouseClick)
        {
            Attack();
        }
    }

    private void Attack()
    {
        CancelInvoke(nameof(EndCombo));
        if (Time.time - _LastComboEnd > _TimeBetweenCombos && _ComboCounter <= SwordCombo.Length)
        {

            if(Time.time - _LastClickedTime >= _TimebetweenAttacks)
            {

                _Anim.runtimeAnimatorController = SwordCombo[_ComboCounter]._AnimatorOV;
                _Anim.Play("Attack", 0, 0);
                _Weapon.Damage = SwordCombo[_ComboCounter].Damage;
                _Weapon.KnockBack = SwordCombo[_ComboCounter].KnockBack;
                _ComboCounter++;
                _LastClickedTime = Time.time;

                if(_ComboCounter + 1 > SwordCombo.Length)
                {
                    _ComboCounter = 0;
                }

                Debug.Log("Attacking!");
            }

        }
    }

    private void ExitAttack()
    {
        if(_Anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f && _Anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            Invoke(nameof(EndCombo), 1);
        }
    }

    private void EndCombo()
    {
        _ComboCounter = 0;
        _LastComboEnd = Time.time;
        Debug.Log("EndingCombo");
    }

}
