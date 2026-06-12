using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using UnityEngine.Events;
using UnityEditor;


public class State_WretchAttack : BaseState
{
    [Header("References")]
    [SerializeField] StatsManager _Statman;
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
    [SerializeField] float _HitBoxHeight;
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

    [Header("Stats")]
    [SerializeField] StatIdentifier _DamageStat;
    [SerializeField] StatIdentifier _knockbackStat;

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

        if(_Statman == null)
        {
            if(_Context != null)
            {
                if (_Context.Stats)
                {
                    _Statman = _Context.Stats;
                }
            }

            if(_Statman == null)
            {
                _Statman = CTX.GetComponentInChildren<StatsManager>();
            }
        }

 
        _AnimatorHandler.TryGetAnimator("Wretched", out Animator Anim);
        _Anim = Anim;

        _Target = FindObjectOfType<Player_Movement>().transform;

        _Statman.OnStatChanged += UpdateStats;
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
        // calculate capsule center = capsuleposition + rotation * offset
        Vector3 CapsuleCenter = _KCC.Capsule.transform.position + (_KCC.Capsule.transform.rotation * _HitboxOffset);


        // calculate the distance from the center to the extremes 
        float pointOffset = (_HitBoxHeight / 2f) - _HitBoxRadius;

        pointOffset = Mathf.Max(0,pointOffset);

        // we get the centers of each circle that makes the capsule.
        // this is the center of the capsule +- the offset
        Vector3 Point0 = CapsuleCenter - (_KCC.CharacterUp * pointOffset);
        Vector3 Point1 = CapsuleCenter + (_KCC.CharacterUp * pointOffset);

        int hits = Physics.OverlapCapsuleNonAlloc(Point0,Point1, _HitBoxRadius,_HitResults,_Mask);


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

    private void UpdateStats(StatIdentifier ID, float value)
    {
        if(ID == _DamageStat)
        {
            _AttackMovementStrenght = value;
        }
        else if (ID == _knockbackStat) _knockbackForce = value;
    }



    private void OnDrawGizmosSelected()
    {
        // null check
        if (_KCC == null || _KCC.Capsule == null) return;

        // direction ray
        Gizmos.color = Color.blue;
        if (_FinalChargeDirection != Vector3.zero)
        {
            Gizmos.DrawRay(_KCC.Capsule.transform.position, _FinalChargeDirection);
        }

        // step 3) we calculate the center of the capsule
        Vector3 CapsuleCenter = _KCC.Capsule.transform.position + (_KCC.Capsule.transform.rotation * _HitboxOffset);

        // step 4) we calculate the offset
        float pointOffset = (_HitBoxHeight / 2f) - _HitBoxRadius;
        pointOffset = Mathf.Max(0, pointOffset);

        // step 5) we get the sphere positions
        Vector3 Point0 = CapsuleCenter - (_KCC.Capsule.transform.up * pointOffset);
        Vector3 Point1 = CapsuleCenter + (_KCC.Capsule.transform.up * pointOffset);

        // step 6) we draw the spheres
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Point0, _HitBoxRadius);
        Gizmos.DrawWireSphere(Point1, _HitBoxRadius);

        // Step 7) we draw the lines
        Gizmos.color = new Color(0, 0, 1, 0.5f); // transparent blue
        Gizmos.DrawLine(Point0 + _KCC.Capsule.transform.right * _HitBoxRadius, Point1 + _KCC.Capsule.transform.right * _HitBoxRadius);
        Gizmos.DrawLine(Point0 - _KCC.Capsule.transform.right * _HitBoxRadius, Point1 - _KCC.Capsule.transform.right * _HitBoxRadius);
        Gizmos.DrawLine(Point0 + _KCC.Capsule.transform.forward * _HitBoxRadius, Point1 + _KCC.Capsule.transform.forward * _HitBoxRadius);
        Gizmos.DrawLine(Point0 - _KCC.Capsule.transform.forward * _HitBoxRadius, Point1 - _KCC.Capsule.transform.forward * _HitBoxRadius);
    }

}
