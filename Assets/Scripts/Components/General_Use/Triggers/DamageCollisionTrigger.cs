using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCollisionTrigger : MonoBehaviour
{
    [SerializeField]Collider _Collider;
    [SerializeField]float Damage, KnockBack;
    [SerializeField]bool CanDealDamage;


    [Header("SetUp")]
    [SerializeField]PlayerContext _Context;
    [SerializeField] bool DealKnockback;
    private void Start()
    {
        _Collider= GetComponent<Collider>();
    }
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.TryGetComponent(out Health_Component HPComp) && CanDealDamage)
        {
          if(_Context == null)
          {
                SimplifiedDamage(HPComp, other);
          }
          else
          {
                DealDamage(HPComp,other);
          }

        }
       
    }

    private void SimplifiedDamage(Health_Component HPComp,Collider other)
    {
        var dir = (other.transform.position - this.transform.position).normalized;
        if (DealKnockback)
        {
            HPComp.SimpleDamage(Damage);

        }
        else
        {
            print("dealing damage! to" + other.name);
            HPComp.SimpleDamage(Damage);
        }
    }

    private void DealDamage(Health_Component HPComp,Collider other)
    {
        var dir = (other.transform.position - this.transform.position).normalized;

        DamageScore DMSCORE = new DamageScore();
        DMSCORE.Attacker= _Context;
        DMSCORE.DamageAmount = Damage;
        DMSCORE.ElementalDamage = ElementType.Physical;
        DMSCORE.FactionID = _Context.faction;

        if (DealKnockback)
        {
            HPComp.TakeDamageWithKnockback(dir, KnockBack, DMSCORE);

        }
        else
        {

            HPComp.TakeDamage(DMSCORE);
        }
    }

    public void UpdateValues(float NDamage,float NKnockBack,bool KNOCK)
    {
        Damage = NDamage;
        KnockBack = NKnockBack;
        DealKnockback = KNOCK;
    }

    public void Activate(bool NewValue)
    {
        CanDealDamage = NewValue;
        _Collider.enabled = true;
    }
}

/*  var dir = (other.transform.position - this.transform.position).normalized;
            if (DealKnockback)
            {
                HPComp.SimpleDamage(Damage);
                
            }
            else
            {
                print("dealing damage! to" + other.name);
                HPComp.SimpleDamage(Damage);
            }
*/
