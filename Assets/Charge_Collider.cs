using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charge_Collider : Visceral_Script
{
    [Header("References")]
    [SerializeField]PlayerContext _OwnerContext;
    [SerializeField]GameObject _OwnerGameObject;
    [SerializeField] GameObject _TargetGameObject;
    [Space]

    [Header("Variables")]
    [SerializeField] float _ChargeDamage,_BiteDamage,_knockback;
    [SerializeField] float _DelayBetweenAttacksInsideCollider;
    [SerializeField] bool _Active;



    public override void VS_InitializeWithParameters(params object[] a)
    {
        _OwnerContext = (PlayerContext)a[0];
        _OwnerGameObject = (GameObject)a[1];
    }


    public void ToggleAttack(bool NewState)
    {
        _Active = NewState;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!_Active) return;
        if (other.TryGetComponent(out PlayerContext OtherContext))
        {
            // el enemigo de dash no puede hacer daño a aliados, por como funciona causa que 
            // se clumpeen y se maten al atacar, lo cual es algo que el jugador no maneja
            if (OtherContext.faction == _OwnerContext.faction) return;
            if (OtherContext.PlayerGameObject== _OwnerGameObject) return;
            if (OtherContext.name == _OwnerContext.name) return;
        }

        //guardo gameobject de mi target
        _TargetGameObject = other.gameObject;
        DealDamage(_TargetGameObject,_ChargeDamage);
        StartCoroutine(PlayerInsideRadius());
    }

    /// <summary>
    /// Esta coroutina es para que el enemigo le haga daño a su target
    /// cuando este se encuentra muy cerca como para hacer un dash
    /// </summary>
    IEnumerator PlayerInsideRadius()
    {
        while(_TargetGameObject != null)
        {
            yield return new WaitForSeconds(_DelayBetweenAttacksInsideCollider);
            print("Bite");
            DealDamage(_TargetGameObject,_BiteDamage);
        }
    
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject == _TargetGameObject)
        {
            print("se escapo mi enemigo");
            StopAllCoroutines();
            _TargetGameObject = null;
        }
    }

    private void DealDamage(GameObject Target,float Damage)
    {
        //realizar daño
        if (Target.TryGetComponent(out Health_Component HPComp))
        {
            Vector3 dir = Target.transform.position - this.transform.position;

            DamageScore DamageDT = new DamageScore();
            DamageDT.Attacker = _OwnerContext;
            DamageDT.DamageAmount = Damage;
            DamageDT.ElementalDamage = ElementType.Physical;
            DamageDT.FactionID = FactionID.LimboMonster1;

            if (HPComp.Context == null) { HPComp.SimpleDamage(Damage);return; }
            HPComp.TakeDamageWithKnockback(dir.normalized, 5, DamageDT);

        }
    }

}
