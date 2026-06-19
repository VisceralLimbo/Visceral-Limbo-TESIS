using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wretch_StateBite : BaseState
{
    [Header("References")]
    [SerializeField] IMovementStrategy movementStrategy;
    [SerializeField] AnimatorHandler _AnimHandler;
    Animator _Anim;
    [SerializeField] Transform _Target;
    [SerializeField] PlayerContext _PlayerContext;
    [SerializeField] StatsManager _StatsManager;

    [Space]
    [Header("Variables")]
    [SerializeField] float _AttackRadius;
    [SerializeField] float _BiteAttack;
    [SerializeField] float _AttackKnockback;
    [SerializeField] LayerMask _AttackMask;

    [SerializeField]bool _CanTransition;

    [Space]
    [Header("Animation Variables")]
    [SerializeField] float _TotalAnimationLenght;
    [SerializeField] float _StartDealingDamage;
    [SerializeField] float _StopDealingDamage;
    float _AnimationPulse = 0;

    [Space]
    [Header("Stats")]
    [SerializeField] StatIdentifier AttackStatID;
    [SerializeField] StatIdentifier KnockbackStatID;

    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        if(_AnimationPulse >= _TotalAnimationLenght)
        {
            if (_Target != null)
            {
                    stateMachine.SetGlobalCondition("BiteMelee", false);

                    return base.EvaluateTransitions(GlobalParams, out TO);
            }
        }
            TO = null;
            return false;
    }

    public override void OnDeInitialize(VisceralStateMachine CTX)
    {

    }

    public override void OnEnter(VisceralStateMachine CTX)
    {
        if(_AnimHandler != null)
        {
            _AnimHandler.SetParameter("Wretched", "IsCharging", AnimatorControllerParameterType.Trigger);
        }

        _AnimationPulse = 0;
        FinishedAttack = false;
    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        movementStrategy.ResetMovementSpeed();

        TaggedHealth.Clear();
    }

    public override void OnInitialize(VisceralStateMachine CTX)
    {
        if(movementStrategy == null)
        {
            movementStrategy = CTX.GetComponentInChildren<IMovementStrategy>();
        }
        if(_AnimHandler == null)
        {
            _AnimHandler = GetComponentInParent<AnimatorHandler>();
        }
        if(_PlayerContext == null)
        {
            _PlayerContext = CTX.GetComponent<PlayerContext>();
        }
        if(_StatsManager == null)
        {
            _StatsManager = CTX.GetComponent<StatsManager>();
        }

        _StatsManager.OnStatChanged += UpdateStats;

        _AnimHandler.TryGetAnimator("Wretched", out Animator Anim);
        _Anim = Anim;

      

        _Target = FindObjectOfType<Player_Movement>().transform;

        stateMachine = CTX;

        _BiteAttack =_StatsManager.GetFloatStatValue(AttackStatID);
        _AttackKnockback = _StatsManager.GetFloatStatValue(KnockbackStatID);

    }


    bool FinishedAttack;
    Collider[] TaggedCol = new Collider[50];

    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        if (_Target == null) return;

        if(movementStrategy != null)
        {
            movementStrategy.UpdateVelocity(Vector3.zero);
            movementStrategy.KillAllMovement();
        }

        _AnimationPulse += Time.deltaTime * TimeDilationManager.GlobalTimeScale;


        if (!FinishedAttack)
        {
            if (_AnimationPulse > _StopDealingDamage)
            {
                FinishedAttack = true;
            }
            else if (_AnimationPulse > _StartDealingDamage)
            {
                // calculamos los hits
                int hits = Physics.OverlapSphereNonAlloc(
                    movementStrategy.GetKCC().Capsule.transform.position
                    , _AttackRadius
                    , TaggedCol,_AttackMask);

                if(hits > 0)
                {
                    for(int i = 0; i < hits; i++)
                    {
                        ProcessHit(TaggedCol[i]);

                    }
                }
               

            }
        }
      
    }


    HashSet<Health_Component> TaggedHealth = new HashSet<Health_Component>();
    private void ProcessHit(Collider other)
    {
        if (other.gameObject == _PlayerContext.PlayerGameObject) return;

        // Calculamos la direccion del knockback
        Vector3 dir = other.transform.position - _PlayerContext.PlayerTransform.position;
        dir.y = 0;

        // Damage score
        DamageScore damageDT = new DamageScore
        {
            Attacker = _PlayerContext,
            DamageAmount = _BiteAttack,
            ElementalDamage = ElementType.Physical,
            FactionID = FactionID.LimboMonster1
        };
        damageDT.AddTag(ScoreFlags.Skill1Kill);

        // llamamos al damage dispatcher
        DamageDispatcher.ProcessSingleHit(
            HitCollider: other,
            DMScore: ref damageDT,
            KnockbackDir: dir.normalized,
            KnockbackForce: _AttackKnockback,
            HitCache: TaggedHealth,
            ProcOnHit: false
        );


    }

    private void UpdateStats(StatIdentifier Stat, float Value)
    {
        if (Stat == AttackStatID) { _BiteAttack = Value; return; };
        if (Stat == KnockbackStatID) {}


    }
}
