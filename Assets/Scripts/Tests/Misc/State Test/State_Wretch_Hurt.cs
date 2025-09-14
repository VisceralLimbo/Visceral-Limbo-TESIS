using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Wretch_Hurt : BaseState, IKnockback
{
    [Header("References")]
    [SerializeField] AnimatorHandler _AnimHandler;
    [SerializeField] IMovementStrategy movementStrategy; 
    Animator _Anim;
    [SerializeField] RagDollTimer ragDollTimer;

    [Space]
    [Header("Variables")]
    private float ForceOfHit;
    [SerializeField] float KnockBackResistance;
    [SerializeField] bool CanTransition;

    private Vector2 DirectionOfHit;

    public void ApplyKnockBack(Vector3 KnockbackDir, float Force)
    {
        DirectionOfHit = KnockbackDir;
        DirectionOfHit.Normalize();
        ForceOfHit = Force;
        ForceOfHit = ForceOfHit / KnockBackResistance;

        if(_AnimHandler != null)
        {
            _AnimHandler.SetParameter("Wretched", "HurtDirectionX",
                                      AnimatorControllerParameterType.Float, DirectionOfHit.x);
            _AnimHandler.SetParameter("Wretched", "HurtDirectionY",
                              AnimatorControllerParameterType.Float, DirectionOfHit.y);
            _AnimHandler.SetParameter("Wretched", "IsHurt", AnimatorControllerParameterType.Trigger);
            
        }
    }

    float pulse;
    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        if(pulse < _MinStateLifetime)
        {
            pulse += Time.deltaTime;
            TO = null;
            return false;
        }

        _AnimHandler.SetParameter("Wretched", "IsHurtRecovered", AnimatorControllerParameterType.Trigger);

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
        movementStrategy.SetActiveState(false);
        CanTransition = false;
    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        stateMachine.SetGlobalCondition("Hurt", false);
        movementStrategy.SetActiveState(true);
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
            hpcomp.OnDeath += Dead;
        }
        else 
        {
            var hp = CTX.GetComponentInChildren<Health_Component>();
            if(hp != null)
            {
                hp.OnKnockbackTaken += ApplyKnockBack;
                hp.OnDamaged += GotHurt;
                hp.OnDeath += Dead;
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

    private void Dead()
    {
        _Anim.enabled = false;

        this.enabled = false;
        movementStrategy.SetActiveState(false);
        ragDollTimer.enabled = true;
    }

    private void GotHurt()
    {
        stateMachine.SetGlobalCondition("Hurt", true);
    }
    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {

        if (_Anim.GetCurrentAnimatorStateInfo(1).normalizedTime < 0.9f)
        {
            CanTransition = false;
            return;
        }
        else
        {
            CanTransition = true;
        }


    }
}
