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

        TaggedColliders.Clear();
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
                Collider[] HitsCollider = Physics.OverlapSphere(movementStrategy.GetKCC().Capsule.transform.position, _AttackRadius, _AttackMask);

                if (HitsCollider.Length > 0)
                {
                    foreach (var Col in HitsCollider)
                    {
                        ProcessHit(Col);
                    }
                }

            }
        }
      
    }

    HashSet<Collider> TaggedColliders = new HashSet<Collider>();
    HashSet<Health_Component> TaggedHealth = new HashSet<Health_Component>();
    private void ProcessHit(Collider other)
    {
        if (other.gameObject == _PlayerContext.PlayerGameObject) return;
        if (TaggedColliders.Contains(other)) return;


        // priorizamos el Idamageable
        if (other.TryGetComponent(out IDamageable Idamage))
        {
            if (Idamage.GetHealthComponent(out Health_Component IHealth) && !TaggedHealth.Contains(IHealth))
            {
                if (IHealth.Context != _PlayerContext)
                {
                    TaggedColliders.Add(other);
                    TaggedHealth.Add(IHealth);

                    Vector3 Dir = IHealth.Context.PlayerTransform.position - _PlayerContext.PlayerTransform.position;
                    Dir.y = 0;


                    DamageScore DamageDT = new DamageScore
                    {
                        Attacker = _PlayerContext,
                        DamageAmount = _BiteAttack,
                        Victim = IHealth.Context, // Puede ser null si es un prop, lo manejamos abajo
                        ElementalDamage = ElementType.Physical,
                        FactionID = FactionID.LimboMonster1
                    };
                    DamageDT.AddTag(ScoreFlags.Skill1Kill);

                    if (IHealth.Context == null)
                    {
                        IHealth.SimpleDamage(_BiteAttack);
                    }
                    else
                    {
                        IHealth.TakeDamageWithKnockback(Dir.normalized, _AttackKnockback, DamageDT);
                    }
                }



            }


        }
        else if (other.TryGetComponent(out Health_Component HPComp))
        {
            if (HPComp.Context != _PlayerContext)
            {
                TaggedColliders.Add(other);
                TaggedHealth.Add(HPComp);

                Vector3 Dir = HPComp.Context.PlayerTransform.position - _PlayerContext.PlayerTransform.position;
                Dir.y = 0;


                DamageScore DamageDT = new DamageScore
                {
                    Attacker = _PlayerContext,
                    DamageAmount = _BiteAttack,
                    Victim = HPComp.Context, // Puede ser null si es un prop, lo manejamos abajo
                    ElementalDamage = ElementType.Physical,
                    FactionID = FactionID.LimboMonster1
                };
                DamageDT.AddTag(ScoreFlags.Skill1Kill);

                if (HPComp.Context == null)
                {
                    HPComp.SimpleDamage(_BiteAttack);
                }
                else
                {
                    HPComp.TakeDamageWithKnockback(Dir.normalized, _AttackKnockback, DamageDT);
                }
            }



        }
    }

    private void UpdateStats(StatIdentifier Stat, float Value)
    {
        if (Stat == AttackStatID) { _BiteAttack = Value; return; };
        if (Stat == KnockbackStatID) { _AttackKnockback = Value; ; return; }


    }
}
