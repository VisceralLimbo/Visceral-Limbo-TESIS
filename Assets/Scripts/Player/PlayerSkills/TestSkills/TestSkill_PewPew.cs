using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSkill_PewPew : Visceral_SkillLogic
{
    [SerializeField] float Damage;
    public override void ActivateSkill()
    {
        print("Pew Pewd for " + Damage);
    }

    public override void Initialize(Visceral_AbilitySO data, Visceral_SkillManager Skmanager, PlayerContext UserContext = null)
    {
        base.Initialize(data, Skmanager, UserContext);
    }


}
