using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class StateMove : BaseState
{
    [Header("References")]
    [SerializeField] KinematicCharacterMotor _KCC;
    [SerializeField] Transform _Target;
    IMovementStrategy _MovementStrategy;

    [Space]

    [Header("variables")]
    [SerializeField] float _Speed;
    [SerializeField] float _MovementAccel;
    [SerializeField] float _MaxRotationSpeed;
    [SerializeField] float _MinDistance;
    [SerializeField] float _MaxDistance;

    Vector3 targetdirection;
    public override void OnInitialize(VisceralStateMachine CTX)
    {
        base.OnInitialize(CTX);
        if(_KCC == null )
        {
            _KCC = CTX.GetComponent<KinematicCharacterMotor>();
            if(_KCC == null)
            {
                _KCC = CTX.GetComponentInChildren<KinematicCharacterMotor>();
            }
            _Target = FindObjectOfType<Player_Movement>().transform;
        }

        if(_MovementStrategy == null)
        {
            if(CTX.gameObject.TryGetComponent<IMovementStrategy>(out IMovementStrategy _Movement))
            {
                _MovementStrategy = _Movement;
                print("Found movement strategy!");
            }
            else
            {
                _MovementStrategy = CTX.GetComponentInChildren<IMovementStrategy>();
                print("Found movement strategy in children!");
            }
        }

        _MovementStrategy.Initialize(_KCC, CTX.gameObject);
    }

    public override void OnEnter(VisceralStateMachine CTX)
    {
        _MovementStrategy.SetActiveState(true);
    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        stateMachine.SetGlobalCondition("Moving", false);
        _MovementStrategy.KillAllMovement();
    }


    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        Vector3 TargetDirection = _Target.transform.position - _KCC.Capsule.transform.position;
        Quaternion LookRotation = _KCC.Capsule.transform.rotation;

        _MovementStrategy.UpdateVelocity(TargetDirection);
        _MovementStrategy.UpdateRotation(LookRotation);

    }

    float pulseLifeTime;
    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        
        if(pulseLifeTime < _MinStateLifetime) //stopgap to avoid the state from switching to fast
        {
            pulseLifeTime += Time.unscaledDeltaTime;
            TO = null;
            return false;
        }

        //CALCULO DE SITUACION
        var Distance = Vector3.Distance(_KCC.Capsule.transform.position, _Target.transform.position);
        if (Distance <= _MinDistance)
        {
            print("Move state: melee");
            stateMachine.SetGlobalCondition("Melee", true);
            
        }
        if(Distance > _MaxDistance)
        {
            stateMachine.SetGlobalCondition("Moving", false);
        }


        if (GlobalParams != null)
        {
            if(Mytransitions.Length> 0)
            {
                foreach(var transition in Mytransitions) 
                {
                    if(transition.ShouldTransition(GlobalParams, out BaseState _TO))
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
}
