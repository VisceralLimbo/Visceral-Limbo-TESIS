using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using UnityEngine.Events;


public class State_WretchAttack : BaseState
{
    [Header("References")]
    [SerializeField] KinematicCharacterMotor _KCC;
    [SerializeField] AnimatorHandler _AnimatorHandler;
    [SerializeField] Animator _Anim; // cache de animador
    [SerializeField] Transform _Target;
    IMovementStrategy _MovementStrategy;
    [Space]

    [Header("Variables")]
    [SerializeField] bool _FinishedAttack;
    [SerializeField] float _AttackMovementStrenght;
    [Space]

    [Header("Events")]
    public UnityEvent OnChargeAttackStart,OnChargeAttackEnd;


    float pulse = 0;
    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        //enforce minimum duration 
        if(pulse <= _MinStateLifetime)
        {
            pulse += Time.deltaTime;
            TO = null;
            return false;
        }


        if (_FinishedAttack)
        {
            return base.EvaluateTransitions(GlobalParams, out TO);
        }
        else
        {
            TO = null;
            return false;
        }
    }

    public override void OnEnter(VisceralStateMachine CTX)
    {
        _FinishedAttack = false;

        _AnimatorHandler.SetParameter("Wretched", "IsCharging", AnimatorControllerParameterType.Trigger);

        //create an Inpulse for the Enemy

        Vector3 Dir = _Target.position - _KCC.Capsule.transform.position;
        Dir.Normalize();
        _MovementStrategy.KillAllMovement();
        _MovementStrategy.ApplyExternalForce(Dir, _AttackMovementStrenght);
        OnChargeAttackStart?.Invoke();
    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        _FinishedAttack = false;
        pulse = 0;
        _MovementStrategy.KillAllMovement();
        OnChargeAttackEnd?.Invoke();
    }

    public override void OnInitialize(VisceralStateMachine CTX)
    {
        _AnimatorHandler = CTX.gameObject.GetComponent<AnimatorHandler>();
        if(_AnimatorHandler == null)
        {
            _AnimatorHandler = CTX.gameObject.GetComponentInChildren<AnimatorHandler>();
        }

        if(_MovementStrategy == null)
        {
            if (CTX.gameObject.TryGetComponent(out IMovementStrategy movement))
            {
                _MovementStrategy = movement;
                
            }
            else
            {
                _MovementStrategy = CTX.GetComponentInChildren<IMovementStrategy>();
            }
        }

        if(_KCC == null)
        {
            if(CTX.gameObject.TryGetComponent(out KinematicCharacterMotor KKC))
            {
                _KCC = KKC;
            }
            else
            {
                KKC =  CTX.gameObject.GetComponentInChildren<KinematicCharacterMotor>();
               _KCC = KKC;
            }
        }

 
        _AnimatorHandler.TryGetAnimator("Wretched", out Animator Anim);
        _Anim = Anim;

        _Target = FindObjectOfType<Player_Movement>().transform;
    }

    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        if(_Anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
        {
            _FinishedAttack = true;
        }
    }


}
