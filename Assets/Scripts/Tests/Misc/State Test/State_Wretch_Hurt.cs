using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Wretch_Hurt : BaseState, IKnockback
{
    [Header("References")]
    [SerializeField] AnimatorHandler _AnimHandler;
    [SerializeField] IMovementStrategy movementStrategy; 
    Animator _Anim;

    [Space]
    [Header("Variables")]
    private float ForceOfHit;
    [SerializeField] float KnockBackResistance;

    private Vector2 DirectionOfHit;

    public void ApplyKnockBack(Vector3 KnockbackDir, float Force)
    {
        DirectionOfHit = KnockbackDir;
        DirectionOfHit.Normalize();
        ForceOfHit = Force;
        ForceOfHit = ForceOfHit / KnockBackResistance;

        if(_AnimHandler != null)
        {
            _AnimHandler.SetParameter("Wretched", "IsHurt", AnimatorControllerParameterType.Trigger);
            _AnimHandler.SetParameter("Wretched", "HurtDirectionX",
                                      AnimatorControllerParameterType.Float, DirectionOfHit.x);
            _AnimHandler.SetParameter("Wretched", "HurtDirectionY",
                              AnimatorControllerParameterType.Float, DirectionOfHit.y);
        }
    }

    float pulse;
    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        if(pulse < _MinStateLifetime)
        {
            print("Still hurting");
            pulse += Time.deltaTime;
            TO = null;
            return false;
        }

        if(_Anim.GetCurrentAnimatorStateInfo(1).normalizedTime < 1)
        {
            print("havent finished my hurt animation");
            TO = null;
            return false;
        }
        print("no longer hurting");

        // si podemos volver al estado anterior, regresamos al estado anterior
        if (stateMachine.LastState != null && stateMachine.LastState != this)
        {
            TO = stateMachine.LastState;
            return true;
        }
        else
        {
            // de lo contrario, fallback a los parametros de transicion
            return base.EvaluateTransitions(GlobalParams, out TO);

        }
    }

    public override void OnEnter(VisceralStateMachine CTX)
    {
        pulse = 0;
        movementStrategy.KillAllMovement();
    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        stateMachine.SetGlobalCondition("Hurt", false);
        base.OnExit(CTX);
    }

    public override void OnInitialize(VisceralStateMachine CTX)
    {
        base.OnInitialize(CTX);

        if(_AnimHandler == null)
        {
            _AnimHandler = CTX.gameObject.GetComponent<AnimatorHandler>();
            if(_AnimHandler == null)
            {
                _AnimHandler = CTX.gameObject.GetComponentInChildren<AnimatorHandler>();
            }
        }

        if (_AnimHandler.TryGetAnimator("Wretched",out Animator value))
        {
            _Anim = value;
        }

        if(CTX.TryGetComponent(out Health_Component hpcomp))
        {
            hpcomp.OnKnockbackTaken += ApplyKnockBack;
            hpcomp.OnDamaged += GotHurt;
            hpcomp.OnDeath += Desubscribe;
        }
        else 
        {
            var hp = CTX.GetComponentInChildren<Health_Component>();
            if(hp != null)
            {
                hp.OnKnockbackTaken += ApplyKnockBack;
                hp.OnDamaged += GotHurt;
                hp.OnDeath += Desubscribe;
            }
        }


        if (movementStrategy == null)
        {
            movementStrategy = CTX.gameObject.GetComponent<IMovementStrategy>();
            if (movementStrategy == null)
            {
                movementStrategy = CTX.gameObject.GetComponentInChildren<IMovementStrategy>();
            }
        }

    }

    private void GotHurt()
    {
        stateMachine.SetGlobalCondition("Hurt", true);
    }
    private void Desubscribe()
    {
        this.enabled= false;
    }


    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        base.OnTick(CTX, TickRate);
    }
}
