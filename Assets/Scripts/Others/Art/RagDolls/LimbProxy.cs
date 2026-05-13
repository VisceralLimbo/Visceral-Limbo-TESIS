using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbProxy : MonoBehaviour, IDamageable
{
    [SerializeField] LimbDesmemberComponent _Dismember;
    IDamageable _DismemberDamageable;

    public bool GetHealthComponent(out Health_Component HPComp)
    {
        return _Dismember.GetHealthComponent(out HPComp);
    }

    public void SimpleDamage(float Damage)
    {
        _DismemberDamageable.SimpleDamage(Damage);
    }

    public void TakeDamage(DamageScore DamageDT)
    {
        _DismemberDamageable.TakeDamage(DamageDT);
    }

    public void TakeDamageWithKnockback(Vector3 KnockbackDir, float KnockbackForce, DamageScore DamageDT)
    {
        _DismemberDamageable.TakeDamageWithKnockback(KnockbackDir,KnockbackForce,DamageDT);
    }

    private void Start()
    {
        if(_Dismember == null) GetComponentInParent<LimbDesmemberComponent>();
        if (_Dismember != null) _DismemberDamageable = _Dismember;
    }

}
