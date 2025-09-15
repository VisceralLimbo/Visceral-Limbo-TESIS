using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimHandler : MonoBehaviour
{
    Animator _anim;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        PlayerEvents.OnAttack += AttackAnim;
        PlayerEvents.OnDash += DashAnim;
        PlayerEvents.OnParry += ParryAnim;
        PlayerEvents.OnWhirlwind += WhirlwindAnim;
    }
    private void AttackAnim() => _anim.SetTrigger("Attack");
    private void DashAnim() => _anim.SetTrigger("Dash");
    private void ParryAnim() => _anim.SetTrigger("Parry");
    private void WhirlwindAnim() => _anim.SetTrigger("Whirlwind");
    private void OnDisable()
    {
        PlayerEvents.OnAttack -= AttackAnim;
        PlayerEvents.OnDash -= DashAnim;
        PlayerEvents.OnParry -= ParryAnim;
        PlayerEvents.OnWhirlwind -= WhirlwindAnim;
    }
}
