using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class Corpus_MovementState : BaseState
{

    [Header("References")]
    [SerializeField] private Corpus_Controller _controller;
    [SerializeField] private AnimatorHandler _animHandler;
    [SerializeField] private IMovementStrategy _movementStrategy;

    [Space]
    [Header("Variables")]
    private float _pulse = 0;

    public override void OnInitialize(VisceralStateMachine CTX)
    {
        base.OnInitialize(CTX);

        if (_controller == null)
        {
            _controller = CTX.GetComponentInChildren<Corpus_Controller>();
        }

        if (_movementStrategy == null)
        {
            _movementStrategy = CTX.GetComponentInChildren<IMovementStrategy>();

            // Precaución: Inicializamos solo si tenemos ambas referencias
            if (_movementStrategy != null && _controller != null)
            {
                _movementStrategy.Initialize(CTX.GetComponentInChildren<KinematicCharacterMotor>(), _controller.playerContext.PlayerGameObject);
            }
        }
    }

    public override void OnEnter(VisceralStateMachine CTX)
    {
        base.OnEnter(CTX);

        _pulse = 0;

        if (_movementStrategy != null)
        {
            _movementStrategy.SetActiveState(true);
        }

        // Limpiamos las variables que nos trajeron a este estado
        CTX.SetGlobalCondition("ShouldMove", false);
        CTX.SetGlobalCondition("ShouldMoveAround", false);

        _animHandler.SetParameter("Corpus_Anim", "Walk", AnimatorControllerParameterType.Bool, true);

        _controller.NotifyAttackFinished();
    }

    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        base.OnTick(CTX, TickRate);

        if (_controller == null || _controller.Target == null) return;

        // CALCULO DE DIRECCION DE MOVIMIENTO
        Vector3 dir = _controller.Target.position - _controller.playerContext.PlayerGameObject.transform.position;

        if (_movementStrategy != null)
        {
            _movementStrategy.UpdateVelocity(dir);
            _movementStrategy.UpdateRotation(dir);
        }
    
    }

    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        // El tiempo de vida minimo evita que la FSM parpadee entre estados muy rapido
        if (_MinStateLifetime > _pulse)
        {
            _pulse += (Time.deltaTime * TimeDilationManager.GlobalTimeScale);
            TO = null;
            return false;
        }

        _pulse = 0;
        return base.EvaluateTransitions(GlobalParams, out TO);
    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        _animHandler.SetParameter("Corpus_Anim", "Walk", AnimatorControllerParameterType.Bool, false);
        base.OnExit(CTX);
    }
}
