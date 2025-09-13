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

    public void AttackAnim()
    {
        _anim.SetTrigger("Attack");
    }
    public void DashAnim()
    {
        _anim.SetTrigger("Dash");
    }
    public void ParryAnim()
    {
        _anim.SetTrigger("Parry");
    }
    public void WhirlwindAnim()
    {
        _anim.SetTrigger("Whirlwind");
    }
}
