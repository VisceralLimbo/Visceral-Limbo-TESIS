using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using UnityEngine.Events;
using UnityEditor;


public class State_WretchAttack : BaseState
{
    [Header("References")]
    [SerializeField] PlayerContext _Context;
    [SerializeField] KinematicCharacterMotor _KCC;
    [SerializeField] AnimatorHandler _AnimatorHandler;
    [SerializeField] Animator _Anim; // cache de animador
    [SerializeField] Transform _Target;
    IMovementStrategy _MovementStrategy;
    [Space]

    [Header("Variables")]
    [SerializeField] bool _FinishedAttack, _CancelAttack;
    [SerializeField] float _Damage;
    [SerializeField] float _AttackMovementStrenght;
    [SerializeField] float _knockbackForce;
    [SerializeField] float _HitBoxRadius;
    [SerializeField] Vector3 _HitboxOffset;
    [SerializeField] LayerMask _Mask;

    Collider[] _HitResults = new Collider[50];
    private HashSet<Health_Component> _hitcache = new HashSet<Health_Component>();

    [Range(0,100)]
    [SerializeField] int _ChanceForPredictiveAttack;
    [SerializeField] float _PredictiveOffset; // que tanto tenemos que exagerar la prediccion 
    [SerializeField] LayerMask _RaycastMask;
    [SerializeField] float _RaycastWallCheckLenght;

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
        _hitcache.Clear();

       
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

        _MovementStrategy.ToggleObstacleAvoidance(true);

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
        _MovementStrategy.ToggleObstacleAvoidance(true);
        CTX.SetGlobalCondition("Melee", true);
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

        if(_Context == null)
        {
            if(CTX.gameObject.TryGetComponent(out PlayerContext Cont))
            {
                _Context = Cont;
            }
            else
            {
                _Context = CTX.gameObject.GetComponentInChildren<PlayerContext>();
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

        //  START ATTACK Y IDLE ATTACK (Fases de Movimiento)
        if (_Anim.GetCurrentAnimatorStateInfo(0).IsTag("StartAttack") || _Anim.GetCurrentAnimatorStateInfo(0).IsTag("IdleAttack"))
        {
            // Chequeamos si se va a estampar contra la pared
            if (IsHittingWall())
            {
                //_MovementStrategy.SetMovementSpeed(_AttackMovementStrenght);
                //_MovementStrategy.UpdateVelocity(_FinalChargeDirection.normalized);
                _MovementStrategy.UpdateVelocity(Vector3.zero);

                if (_MovementStrategy != null)
                {
                    _MovementStrategy.ToggleObstacleAvoidance(false);
                    print("Toggle Off start / Idle");
                }


            }
            else
            {
                // Si el camino esta libre, ataca normal
                _MovementStrategy.SetMovementSpeed(_AttackMovementStrenght);
                _MovementStrategy.UpdateVelocity(_FinalChargeDirection.normalized);
            }

            // procesar danio 
            ProcessHitbox(CTX);


            // Si estamos en la fase de vuelo, chequeamos el final de la animacion
            if (_Anim.GetCurrentAnimatorStateInfo(0).IsTag("IdleAttack") && _Anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
            {
                _AnimatorHandler.SetParameter("Wretched", "IsAttacking", AnimatorControllerParameterType.Bool, false);
            }
            return;
        }

        // animation state = finalizar ataque
        if (_Anim.GetCurrentAnimatorStateInfo(0).IsTag("EndAttack")
            && _Anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1)
        {
            if (IsHittingWall())
            {
                _MovementStrategy.UpdateVelocity(Vector3.zero);

                _MovementStrategy.ToggleObstacleAvoidance(false);
                print("Toggle Off End");

                if (_Anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
                {
                    _FinishedAttack = true;
                }
                return;
            }


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


    private bool IsHittingWall()
    {
        if (_KCC == null) return false;

        Vector3 OriginPoint = _KCC.Capsule.bounds.center;
        Vector3 Direction = _FinalChargeDirection.normalized;

        float radius = _KCC.Capsule.radius * 0.9f;
        float Distance = _KCC.Capsule.radius + _RaycastWallCheckLenght;

        if (Physics.SphereCast(OriginPoint,radius,Direction,out RaycastHit hit,_RaycastWallCheckLenght, _RaycastMask))
        {
            return true;
        }

        return false;

    }

    void ProcessHitbox(VisceralStateMachine CTX)
    {

        Vector3 sphereCenter = _KCC.Capsule.transform.position + (_KCC.Capsule.transform.rotation * _HitboxOffset);

        int hits = Physics.OverlapSphereNonAlloc(sphereCenter, _HitBoxRadius,_HitResults,_Mask);


        for(int i = 0; i < hits ; i++)
        {
            Collider hitcol = _HitResults[i];

            if (hitcol == null) continue;

            DamageScore DMS = new DamageScore()
            {
                Attacker = _Context,
                DamageAmount = _Damage,
                ElementalDamage = ElementType.Physical,
                FactionID = FactionID.LimboMonster1
            };

            Vector3 knockbackDir = (hitcol.transform.position - _KCC.Capsule.transform.position);
            knockbackDir.y = 0;

            DamageDispatcher.ProcessSingleHit
                (
                hitcol,
                ref DMS,
                knockbackDir,
                _knockbackForce,
                _hitcache,
                false               
                );
        }

    }


    private void OnDrawGizmosSelected()
    {

        Gizmos.DrawLine(_KCC.CharacterUp, _FinalChargeDirection);


    }

}
