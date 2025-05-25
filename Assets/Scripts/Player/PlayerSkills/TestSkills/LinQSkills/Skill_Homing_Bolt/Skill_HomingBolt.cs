using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;



public class Skill_HomingBolt : Visceral_SkillLogic
{
    [SerializeField] float AoeRadius;
    [SerializeField] float Damage;
    [SerializeField] int ChainLightning;
    [SerializeField] Collider[] HTCollider;
    [SerializeField] Health_Component[] health_Components;

    public override void ActivateSkill()
    {
        print("homingbolt");
        Collider[] HitColliders = Physics.OverlapSphere(_UserContext.KCCMotor.Capsule.transform.position, AoeRadius);
        HTCollider = HitColliders;

        Health_Component[] components = HitColliders.Where(X => X.GetComponentInParent<Health_Component>()).Select(X => X.GetComponentInParent<Health_Component>()).ToArray();
        components.OrderBy(X => X.CurrentHealth);
        components.Take(ChainLightning); // este ataque realiza menos daño por cadena
        health_Components = components;
        foreach(Health_Component component in components)
        {
            component.SimpleDamage(Damage - ChainLightning);
            print("dealt damage to " + component.name + " equal to " + (Damage - ChainLightning));
        }

    }

    public override void Initialize(Visceral_AbilitySO data, Visceral_SkillManager Skmanager, PlayerContext UserContext = null)
    {
        base.Initialize(data, Skmanager, UserContext);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(_UserContext.KCCMotor.Capsule.transform.position, AoeRadius);
    }

}


// log: Creado por Patricio Malvasio
//
// implementaciones usadas => select(1) / where(1) / take(1) / orderby(2) / tolist(3)
//