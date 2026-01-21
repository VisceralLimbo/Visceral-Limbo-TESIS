using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class Decayed_MoveState : BaseState
{
    [Header("References")]
    [SerializeField] KinematicCharacterMotor _KCC;
    [SerializeField] AnimatorHandler _AnimatorHandler;
    [SerializeField] Transform _Target;
    IMovementStrategy _MovementStrategy;

    [Space]

    [Header("variables")]
    [SerializeField] float _Speed;
    [SerializeField] float _MovementAccel;
    [SerializeField] float _MaxRotationSpeed;
    [SerializeField] float _MinDistance;
    [SerializeField] float _MaxDistance;
    [SerializeField] float _RunDirection = 1f; // direccion de movimiento, si es -1 signfica huir

    [Space]
    [Header("Stats")]
    [SerializeField] string _MovementStatID;

    public override void OnInitialize(VisceralStateMachine CTX)
    {
        base.OnInitialize(CTX);
        if (_KCC == null)
        {
            _KCC = CTX.GetComponent<KinematicCharacterMotor>();
            if (_KCC == null)
            {
                _KCC = CTX.GetComponentInChildren<KinematicCharacterMotor>();
            }
            _Target = FindObjectOfType<Player_Movement>().transform;
        }

        if (_MovementStrategy == null)
        {
            if (CTX.gameObject.TryGetComponent<IMovementStrategy>(out IMovementStrategy _Movement))
            {
                _MovementStrategy = _Movement;
            }
            else
            {
                _MovementStrategy = CTX.GetComponentInChildren<IMovementStrategy>();
            }
        }

        if (_AnimatorHandler == null)
        {
            if (CTX.gameObject.TryGetComponent(out AnimatorHandler Handler))
            {
                _AnimatorHandler = Handler;
            }
            else
            {
                _AnimatorHandler = CTX.gameObject.GetComponentInChildren<AnimatorHandler>();
            }
        }

        StatsManager _Stats = CTX.GetComponent<StatsManager>();
        if (_Stats == null)
        {
            CTX.GetComponentInChildren<StatsManager>();
        }

        if(_Stats != null)
        {
            _Stats.OnStatChanged += UpdateStats;
        }
        


        _MovementStrategy.Initialize(_KCC, CTX.gameObject);
        _Target = FindObjectOfType<Player_Movement>().transform;
    }

    public override void OnEnter(VisceralStateMachine CTX)
    {
        _MovementStrategy.SetActiveState(true);
        _AnimatorHandler.SetParameter("Decayed", "Walking", AnimatorControllerParameterType.Trigger);

    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        stateMachine.SetGlobalCondition("Moving", false);
        _MovementStrategy.KillAllMovement();
    }


    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        if (!isActiveAndEnabled || !CTX.gameObject.activeSelf || !_Target)
        {
            return;
        }

        Vector3 TargetDirection = (_Target.transform.position - _KCC.Capsule.transform.position);

        float TargetDistance = TargetDirection.magnitude;

        //
        // determinar acciones
        //

        bool IsTooFar = TargetDistance > _MaxDistance;
        bool IsTooClose = TargetDistance < _MinDistance;
        bool InAttackRange = !IsTooClose && !IsTooFar;
        //
        // movimiento
        //

        if (IsTooFar)
        {
            _RunDirection = 1;
        }
        else if (IsTooClose)
        {
            _RunDirection = -1;
        }
        else
        {
            _RunDirection = -1f;
        }

        
        Vector3 FinalMovementDirector = TargetDirection.normalized;
        FinalMovementDirector *= _RunDirection;
     

        Debug.Log(FinalMovementDirector);
        _MovementStrategy.UpdateVelocity(FinalMovementDirector);

        //
        // animaciones
        //

        bool ShouldAttack = InAttackRange;
        bool ShouldMove = IsTooClose || IsTooFar;

        _AnimatorHandler.SetParameter("Decayed", "Walking", AnimatorControllerParameterType.Trigger);

        stateMachine.SetGlobalCondition("Moving", ShouldMove);
        stateMachine.SetGlobalCondition("Attack", ShouldAttack);

    }

    float pulseLifeTime;
    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {

        if (pulseLifeTime < _MinStateLifetime) //stopgap to avoid the state from switching to fast
        {
            pulseLifeTime += Time.unscaledDeltaTime;
            TO = null;
            return false;
        }

        if (GlobalParams != null)
        {
            if (Mytransitions.Length > 0)
            {
                foreach (var transition in Mytransitions)
                {
                    if (transition.ShouldTransition(GlobalParams, out BaseState _TO))
                    {
                        print("Should transition to " + _TO.name);
                        TO = _TO;

                        pulseLifeTime = 0;
                        return true;
                    }
                }
            }
        }

        //NO SE PUDO TRANSICIONAR
        TO = null;
        return false;
    }


    private void UpdateStats(string StatID,float Value)
    {
        if(StatID == _MovementStatID) _Speed = Value;
    }

}
