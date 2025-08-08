using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class Dash_Skill : Visceral_SkillLogic
{

    [SerializeField] Player_Movement _KCCMotorMovement;
    [SerializeField] Player_Base _Base;
    [SerializeField] private float _DashStrenght;

    public override void ActivateSkill()
    {
        var Input = _Base._Player_InputActions.Gameplay.Movement.ReadValue<Vector2>();
        var MovementDir = new
        {
            DirX = Input.x,
            DirY = Input.y,
        };
        Transform UpDirector = _KCCMotorMovement.KKCMotor.Transform;

        Vector3 DashMovement = new Vector3(MovementDir.DirX, 0, MovementDir.DirY).normalized;
        Vector3 FinalDashMovement = Quaternion.Euler(0, UpDirector.eulerAngles.y, 0) * DashMovement;
        FinalDashMovement *= _DashStrenght;
        _KCCMotorMovement.AddExternalVelocity(FinalDashMovement, true);



    }

    public override void Initialize(Visceral_AbilitySO data, Visceral_SkillManager Skmanager, PlayerContext UserContext = null)
    {
        base.Initialize(data, Skmanager, UserContext);

        _KCCMotorMovement = UserContext.GetComponent<Player_Movement>();
        if(_KCCMotorMovement == null)
        {
            _KCCMotorMovement = UserContext.GetComponentInChildren<Player_Movement>();
        }
        _Base = UserContext.GetComponent<Player_Base>();

        if(_Base == null)
        {
            _Base = UserContext.GetComponentInChildren<Player_Base>(true);
        }

    }
}
