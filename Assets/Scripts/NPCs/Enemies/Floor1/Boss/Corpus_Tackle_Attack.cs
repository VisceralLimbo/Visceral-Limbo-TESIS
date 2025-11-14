using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpus_Tackle_Attack : BaseState, IStateEnergyCost
{
    [Header("Transition Setup")]
    [SerializeField] string TransitionKey;
    [SerializeField] Corpus_Thinking_Main_State _Main_State;
    [SerializeField] int EnergyCost;
    [SerializeField] float TimingDuration;
    [SerializeField] bool CanExit = false;
    [Space]

    [Header("References")]
    [SerializeField] IMovementStrategy _movementStrategy;
    [SerializeField] Corpus_Thinking_Main_State _MainState;
    [SerializeField] Transform _Target;
    [SerializeField] Collider _Col;

    [SerializeField] PlayerContext _PlayerContext;
    [Space]

    [Header("Variables")]
    [SerializeField] bool _LockAngle;
    [SerializeField] float _TackleSpeed;

    [SerializeField] float damage;
    [SerializeField] float _knockback;
    Vector3 _Direction;


    [SerializeField]float pulse = 0;

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
        base.OnEnter(CTX);

        if(_LockAngle == false)
        {
            if(_Target != null)
            {
                _Direction = _Target.position - _Main_State.playerContext.PlayerGameObject.transform.position;
                _Direction.Normalize();
                _LockAngle = true;
            }

        }

        CanExit = false;
        pulse = 0;
        _movementStrategy.SetActiveState(true);
        _movementStrategy.SetMovementSpeed(_TackleSpeed);
        _Col.enabled = true;

        _Main_State.DeactivateEnergy(true);
        _movementStrategy.KillAllMovement();        
    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        _LockAngle = false;


        _MainState.DeactivateEnergy(false);

        _movementStrategy.ResetMovementSpeed();
        _movementStrategy.UpdateVelocity(Vector3.zero);

        CTX.SetGlobalCondition(TransitionKey,false);
        _Col.enabled = false;
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
            _MainState = this.transform.parent.GetComponent<Corpus_Thinking_Main_State>();
        }

        if(_Target == null)
        {
            _Target = FindObjectOfType<Player_Base>().GetComponentInChildren<Player_Movement>().transform;
        }

        if(_Col == null)
        {
            _Col = GetComponent<Collider>();
        }

        if(_PlayerContext == null)
        {
            _PlayerContext =_MainState.playerContext;
        }
    }

    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        if (pulse < TimingDuration)
        {
            pulse = pulse + Time.deltaTime * TimeDilationManager.GlobalTimeScale;

            _movementStrategy.UpdateVelocity(_Direction);
        }
        else
        {
            CanExit = true;
        }
    }

    public void SetCost(float NewCost)
    {
        EnergyCost = (int)NewCost;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent == this.transform.parent) return;
        if (other.gameObject == this.gameObject) return;

        if(other.TryGetComponent(out Health_Component HPComp))
        {
            if(HPComp.Context != null && HPComp.Context != _PlayerContext)
            {
                DamageScore dmscore = new DamageScore();
                dmscore.Attacker = _PlayerContext;
                dmscore.ElementalDamage = ElementType.Physical;
                dmscore.DamageAmount = damage;
                dmscore.FactionID = _PlayerContext.faction;


                Vector3 Dir = HPComp.transform.position - _PlayerContext.PlayerTransform.position;
                Dir.Normalize();

                HPComp.TakeDamageWithKnockback(Dir, _knockback, dmscore);
            }

        }


    }
}
