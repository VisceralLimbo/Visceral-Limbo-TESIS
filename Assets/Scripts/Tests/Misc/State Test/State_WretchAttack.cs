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

    [Range(0,100)]
    [SerializeField] int _ChanceForPredictiveAttack;
    [SerializeField] float _PredictiveOffset; // que tanto tenemos que exagerar la prediccion 

    [Space]

    [Header("Events")]
    public UnityEvent OnChargeAttackStart,OnChargeAttackEnd;


    float pulse = 0;

    Transform _LastTarget;
    PlayerContext _LastContext;
    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        //enforce minimum duration 
        if(pulse <= _MinStateLifetime)
        {
          
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

    Vector3 _FinalChargeDirection;
    public override void OnEnter(VisceralStateMachine CTX)
    {
        _FinishedAttack = false;

        //_AnimatorHandler.SetParameter("Wretched", "IsCharging", AnimatorControllerParameterType.Trigger);
        _AnimatorHandler.SetParameter("Wretched", "IsAttacking", AnimatorControllerParameterType.Bool, true);


        int RandomSeed = Random.Range(0, 101);
        if(RandomSeed <= _ChanceForPredictiveAttack)
        {
            PredictiveAttack();
        }
        else
        {
            RegularAttack();
        }


    }


    private void RegularAttack()
    {
        //create an Inpulse for the Enemy

        Vector3 Dir = _Target.position - _KCC.Capsule.transform.position;

        _FinalChargeDirection = Dir;
        OnChargeAttackStart?.Invoke();
    }

    private void PredictiveAttack()
    {

        if(_LastTarget != _Target || _LastTarget == null)
        {

            _LastTarget = _Target;
            _LastContext = _Target.GetComponentInParent<PlayerContext>();
        }

        Vector3 _TargetVelocity = Vector3.zero;

        if (_LastContext != null)
        {
            _TargetVelocity = _LastContext.KCCMotor.BaseVelocity;
        }

    

        Vector3 OvershootTarget = _LastContext.KCCMotor.Capsule.transform.position + (_TargetVelocity * _PredictiveOffset);  

        Vector3 Dir = OvershootTarget - _KCC.Capsule.transform.position;
        _FinalChargeDirection = Dir;

        OnChargeAttackStart?.Invoke();

    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        _FinishedAttack = false;
        pulse = 0;
        _MovementStrategy.KillAllMovement();
        _MovementStrategy.ResetMovementSpeed();
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
        pulse += Time.deltaTime * TimeDilationManager.GlobalTimeScale;

        _MovementStrategy.UpdateRotation(_FinalChargeDirection.normalized);


           // animation state = windup del ataque 
        if(_Anim.GetCurrentAnimatorStateInfo(0).IsTag("WindupAttack"))
        {
            // cero movimiento
            _MovementStrategy.UpdateVelocity(Vector3.zero);
            _MovementStrategy.KillAllMovement();
            return;
        }
           //_MovementStrategy.ApplyExternalForce(FinalDir, _AttackMovementStrenght);

        if (_Anim.GetCurrentAnimatorStateInfo(0).IsTag("StartAttack"))
        {
            Vector3 MoveDir = _FinalChargeDirection.normalized;

            _MovementStrategy.SetMovementSpeed(_AttackMovementStrenght);

            _MovementStrategy.UpdateVelocity(MoveDir);
            return;
        }


            // animation state = en el aire
        if (_Anim.GetCurrentAnimatorStateInfo(0).IsTag("IdleAttack"))     
        {
            Vector3 MoveDir = _FinalChargeDirection.normalized;

            _MovementStrategy.SetMovementSpeed(_AttackMovementStrenght);

            _MovementStrategy.UpdateVelocity(MoveDir);
        

            // estoy finalizando la animacion de ataque
            if(_Anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9)
            {
                _AnimatorHandler.SetParameter("Wretched", "IsAttacking", AnimatorControllerParameterType.Bool, false);
            }
            return;
        }

        // animation state = finalizar ataque
        if (_Anim.GetCurrentAnimatorStateInfo(0).IsTag("EndAttack")
            && _Anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1)
        {

            float SlowdownSpeed = Mathf.Lerp(_AttackMovementStrenght, 0f,_Anim.GetCurrentAnimatorStateInfo(0).normalizedTime);

            _MovementStrategy.SetMovementSpeed(SlowdownSpeed);
            _MovementStrategy.UpdateVelocity(_FinalChargeDirection.normalized);


            if (_Anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
            {
                _FinishedAttack = true;
            }
            return;

        }
     


    }

    private void OnDrawGizmosSelected()
    {

        Gizmos.DrawLine(_KCC.CharacterUp, _FinalChargeDirection);


    }

}
