using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class Corpus_MovementState : BaseState
{

    [Header("References")]
    [SerializeField] Transform Model;
    [SerializeField] Transform Target;

    [SerializeField] Corpus_Thinking_Main_State thinkingMain;
    [SerializeField] AnimatorHandler _AnimHandler;

    [SerializeField] IMovementStrategy MovementStrategy;

    [Space]
    [Header("Variables")]
    [SerializeField] float movementSpeed;

    float pulse = 0;
    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        if(_MinStateLifetime > pulse)
        {
            pulse = pulse + (Time.deltaTime * TimeDilationManager.GlobalTimeScale);
            TO = null;
            return false;
        }

        pulse = 0;
        return base.EvaluateTransitions(GlobalParams, out TO);
    }

    public override void OnDeInitialize(VisceralStateMachine CTX)
    {
        base.OnDeInitialize(CTX);
    }

    public override void OnEnter(VisceralStateMachine CTX)
    {

        base.OnEnter(CTX);
        Target = thinkingMain.GetTargetTransform;
        Model = thinkingMain.GetModel.transform;
        MovementStrategy.SetActiveState(true);

        thinkingMain.DeactivateEnergy(false);
        CTX.SetGlobalCondition("ShouldMove", false);
        _AnimHandler.SetParameter("Corpus_Anim", "Walk", AnimatorControllerParameterType.Bool, true);
    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        if(thinkingMain.GetCurrentEnergy > 3)
        {
            _AnimHandler.SetParameter("Corpus_Anim", "Walk", AnimatorControllerParameterType.Bool, false);
        }
        base.OnExit(CTX);
    }

    public override void OnInitialize(VisceralStateMachine CTX)
    {
        base.OnInitialize(CTX);
        if(thinkingMain == null)
        {
            thinkingMain = CTX.GetComponentInChildren<Corpus_Thinking_Main_State>();
        }

        if(MovementStrategy == null)
        {
            MovementStrategy = CTX.GetComponentInChildren<IMovementStrategy>();
            MovementStrategy.Initialize(CTX.GetComponentInChildren<KinematicCharacterMotor>(),thinkingMain.GetModel);
        }
    }

    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        base.OnTick(CTX, TickRate);

        //CALCULO DE DIRECCION DE MOVIMIENTO
        Vector3 Dir = Target.transform.position - Model.transform.position;

        MovementStrategy.UpdateVelocity(Dir);

        float DistanceToPlayer = Vector3.Distance(Target.transform.position, Model.transform.position);

        if (DistanceToPlayer <= thinkingMain.MinimumAttackRange)
        {
            stateMachine.SetGlobalCondition("InRange", true);
        }
        else
        {
            stateMachine.SetGlobalCondition("InRange", false);
        }

    }
}
