using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

public class Corpus_Tackle_Attack : BaseState, IStateEnergyCost
{
    [Header("Transition Setup")]
    [SerializeField] string TransitionKey;
    [SerializeField] int EnergyCost;
    [SerializeField] float TimingDuration;
    [SerializeField] bool CanExit = false;
    [Space]

    [Header("References")]
    [SerializeField] IMovementStrategy _movementStrategy;
    [SerializeField] Corpus_Controller _MainState;
    [SerializeField] Transform _Target;
    [SerializeField] Collider _Col;
    [SerializeField] AnimatorHandler _AnimatorHandler;

    [SerializeField] PlayerContext _PlayerContext;
    [SerializeField] FireDamageZone _FireObjPrefab;
    [SerializeField] ObjectPool<FireDamageZone> _Mypool;

    [Space]
    [Header("Variables")]
    [SerializeField] bool _LockAngle;
    [SerializeField] float _TackleSpeed;
    [Space]
    [SerializeField] bool _SecondPhase;
    [SerializeField] float _DelayBetweenFires;
    [SerializeField] float _FireDuration;
    [SerializeField] LayerMask _FireMask;


    Vector3 _Direction;


    [SerializeField]float pulse = 0;

    [Header("Events")]
    [SerializeField] UnityEvent OnTackleStart;
    [SerializeField] UnityEvent OnTackleEnd;

    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        if(CanExit == false)
        {
            TO = null;
            return false;
        }

        return base.EvaluateTransitions(GlobalParams, out TO);
    }

    public int GetCost()
    {
        return EnergyCost;
    }

    public BaseState GetState()
    {
       return this;
    }

    public string GetTransitionKey()
    {
        return TransitionKey;
    }

    public override void OnEnter(VisceralStateMachine CTX)
    {

        if (_Target == null)
        {
            _Target = _MainState.Target;
        }

        base.OnEnter(CTX);

        if(_LockAngle == false)
        {
            if(_Target != null)
            {
                _Direction = _Target.position - _MainState.playerContext.PlayerGameObject.transform.position;
                _Direction.Normalize();
                _LockAngle = true;
            }

        }

        CanExit = false;
        pulse = 0;
        SecondPhase_Pulse = 0;
        _movementStrategy.SetActiveState(true);
        _movementStrategy.SetMovementSpeed(_TackleSpeed);

        OnTackleStart?.Invoke();

  
        _movementStrategy.KillAllMovement();
        _AnimatorHandler.SetParameter("Corpus_Anim", "Running", AnimatorControllerParameterType.Bool,true);
    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        _LockAngle = false;


        _MainState.NotifyAttackFinished();

        _movementStrategy.ResetMovementSpeed();
        _movementStrategy.UpdateVelocity(Vector3.zero);
        

        CTX.SetGlobalCondition(TransitionKey,false);

        OnTackleEnd?.Invoke();
        _AnimatorHandler.SetParameter("Corpus_Anim", "Running", AnimatorControllerParameterType.Bool, false);

        base.OnExit(CTX);
    }

    public override void OnInitialize(VisceralStateMachine CTX)
    {
        base.OnInitialize(CTX);

        if(_movementStrategy == null)
        {
            _movementStrategy = CTX.GetComponentInChildren<IMovementStrategy>();
        }
        if(_MainState == null)
        {
            _movementStrategy = _MainState.MovementStrategy;

        }

        if(_Col == null)
        {
            _Col = GetComponent<Collider>();
        }

        if(_PlayerContext == null)
        {
            _PlayerContext =_MainState.playerContext;
        }

        // creamos pool
        _Mypool = new ObjectPool<FireDamageZone>
            (
                createFunc: CreateFireZone, // funcion de creacion
                actionOnGet: zone => zone.gameObject.SetActive(true), // que hacemos cuando agarramos
                actionOnRelease: zone => zone.gameObject.SetActive(false), // que hacemos cuando liberamos
                actionOnDestroy: zone => Destroy(zone.gameObject), // que hacemos cuando destruimos
                collectionCheck: false,
                defaultCapacity: 15,
                maxSize:50
            );


        _MainState.HealthComp.OnSecondPhase += SetSecondPhase;
    }


    private FireDamageZone CreateFireZone()
    {
        FireDamageZone NewFireZone = Instantiate(_FireObjPrefab);
        NewFireZone.Initialize(_PlayerContext);

        if(NewFireZone.TryGetComponent(out PoolObjectComponent PoolObj))
        {
            // inicializamos el PoolObj y le pasamos de funcion parametro el Release del pool
            PoolObj.Initialize(() => _Mypool.Release(NewFireZone));
        }

        return NewFireZone;
    }

    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        if (pulse < TimingDuration)
        {
            pulse = pulse + Time.deltaTime * TimeDilationManager.GlobalTimeScale;
            _movementStrategy.UpdateVelocity(_Direction);

            if (_SecondPhase)
            {
                SecondPhase_FireZone();
            }
        }
        else
        {
            CanExit = true;
            stateMachine.SetGlobalCondition(TransitionKey, false);
            stateMachine.SetGlobalCondition(_MainState.CheckDistanceForTransitions(), true);

        }
    }

    public void SetCost(float NewCost)
    {
        EnergyCost = (int)NewCost;
    }


    float SecondPhase_Pulse;

    private void SecondPhase_FireZone()
    {
        SecondPhase_Pulse += Time.deltaTime * TimeDilationManager.GlobalTimeScale;


        if(SecondPhase_Pulse >= _DelayBetweenFires)
        {
            SecondPhase_Pulse = 0;
            FireDamageZone fire = _Mypool.Get();

            Vector3 FinalPos = Vector3.zero;

            if (Physics.Raycast(_PlayerContext.PlayerTransform.position, Vector3.down, out RaycastHit HitInfo, 10, _FireMask))
            {
                FinalPos = HitInfo.point + (Vector3.up * 0.3f);
            }

            fire.transform.position = FinalPos;
        
        }
    }


    private void SetSecondPhase()
    {
        _SecondPhase = true;
    }

}
