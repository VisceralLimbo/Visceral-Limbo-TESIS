using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StateIdle : BaseState
{

    [SerializeField] float _DistanceToChase;
    [SerializeField] Transform _target;
    [SerializeField] Transform _User;
    [SerializeField] IMovementStrategy movementStrategy;

    public override void OnInitialize(VisceralStateMachine CTX)
    {
        base.OnInitialize(CTX);
        movementStrategy = CTX.gameObject.GetComponentInChildren<IMovementStrategy>();
        _target = FindObjectOfType<Player_Movement>().gameObject.transform;
        _User = CTX.transform;
    }


    float pulseLife;
    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {

        if(pulseLife < _MinStateLifetime)
        {
            pulseLife += Time.unscaledDeltaTime;
            TO = null;
            return false;
        }

        //CHEQUEAMOS LA TRANSICIONES PRIMERO
        float _DistanceToTarget = Vector2.Distance
                                  (_target.transform.position, _User.transform.position);
        if (_DistanceToTarget < _DistanceToChase)
        {
            stateMachine.SetGlobalCondition("Moving", true);
          
        }
        else
        {
            stateMachine.SetGlobalCondition("Moving", false);

        }
        // checkeo si podemos transicionar
        if (GlobalParams != null)
        {
            if (Mytransitions.Length > 0)
            {
                foreach (var transition in Mytransitions)
                {
                    if(transition.ShouldTransition(GlobalParams, out BaseState _TO))
                    {
                        print("Should transition to " + _TO.name);
                        TO = _TO;
                        pulseLife = 0;
                        return true;
                    }
                }
            }
        }

        //NO SE PUDO TRANSICIONAR
        TO = null;
        return false;

    }

    public override void OnEnter(VisceralStateMachine CTX)
    {
        base.OnEnter(CTX);
        movementStrategy.KillAllMovement();
    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        base.OnExit(CTX);
        movementStrategy.SetActiveState(true);
    }

    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        base.OnTick(CTX, TickRate);


    }
}
