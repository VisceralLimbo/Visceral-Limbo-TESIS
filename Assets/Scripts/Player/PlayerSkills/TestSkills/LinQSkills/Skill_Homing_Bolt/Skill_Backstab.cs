using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Skill_Backstab : Visceral_SkillLogic
{
    [SerializeField] float AoeRadius;
    [SerializeField] float Damage;
    [SerializeField] Collider[] HTCollider;
    [SerializeField] Health_Component[] health_Components;

    public override void ActivateSkill()
    {
        Debug.Log(">>> Skill_Backstab: Activated");

        Collider[] hitColliders = Physics.OverlapSphere(_UserContext.KCCMotor.Capsule.transform.position, AoeRadius);
        HTCollider = hitColliders;

        
        var playerHealth = _UserContext.KCCMotor.GetComponentInParent<Health_Component>();

       
        var components = hitColliders // Hecho por Lucas - Select, Where y ToArray
            .Select(c => c.GetComponentInParent<Health_Component>())
            .Where(hc => hc != null && hc != playerHealth)
            .ToArray();


        if (components.Length == 0)
            return;

        Debug.Log($">>> Skill_Backstab: Enemies Detected = {components.Length}");


        var furthestData = components // Hecho por Lucas - Select, OrderByDescending y FirstOrDefault y Tipo Anonimo
           .Select(hc => new { Health = hc, Distance = Vector3.Distance(hc.transform.position, _UserContext.KCCMotor.Capsule.transform.position) })
           .OrderByDescending(data => data.Distance)
           .FirstOrDefault();

        if (furthestData != null) //Hecho por Lucas - Tipo Anonimo y Tupla
        {
            Debug.Log($"Backstab aplicado a: {furthestData.Health.name} con distancia {furthestData.Distance}");

            Vector3 direction = (furthestData.Health.transform.position - _UserContext.KCCMotor.Capsule.transform.position).normalized;
            var damageData = new Tuple<Vector3, float, float>(direction, Damage, 0f);
            furthestData.Health.SimpleDamage(damageData);

        }
    }

}

//
// Script hecho por Lucas Torres
// se encarga de crear otra habilidad que funcionaria como un backstab
//
