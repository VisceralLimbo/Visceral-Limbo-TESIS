using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class Corpus_ButtSlam : BaseState, IStateEnergyCost
{
    [Header("Energy Setup")]
    [SerializeField] int EnergyCost;
    [SerializeField] string TransitionKey;
    [Space]

    [Header("References")]
    [SerializeField] Transform _Target;
    [SerializeField] Corpus_Thinking_Main_State _Main_State;
    IMovementStrategy _MoveStrategy;
    [SerializeField] KinematicCharacterMotor _KCC;
    
    [Space]

    [Header("Variables")]
    [SerializeField] float _JumpStrenght;
    [SerializeField] float _JumpDuration;
    [SerializeField] bool _FinishedAttack;

    [Space]


    [Header("For Testing purposes")]
    [SerializeField] float attackradius;
    [SerializeField] float AttackDamage;
    [SerializeField] float AttackKnockback;
    [SerializeField] bool DrawWireframe;
    [SerializeField] Transform _Model;

    float timer;
    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        print("ButtSlam finished " + _FinishedAttack);
        if(_KCC.GroundingStatus.IsStableOnGround && _FinishedAttack)
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
        base.OnEnter(CTX);

        _Main_State.DeactivateEnergy(true);

        _MoveStrategy.KillAllMovement();
        _ExecutingAttack = false;
        _FinishedAttack = false;
    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        base.OnExit(CTX);
        _Main_State.DeactivateEnergy(false);
        _ExecutingAttack = false;
        _FinishedAttack = false;


        CTX.SetGlobalCondition(TransitionKey, false);
    }

    public override void OnInitialize(VisceralStateMachine CTX)
    {
        base.OnInitialize(CTX);

        if(_Target == null)
        {

            _Target = FindObjectOfType<Player_Movement>().transform;

        }

        if(_Main_State == null)
        {
            _Main_State = GetComponentInParent<Corpus_Thinking_Main_State>();
        }

        if(_MoveStrategy == null)
        {
            _MoveStrategy = CTX.GetComponentInChildren<IMovementStrategy>();
        }

        if(_KCC == null)
        {
            _KCC = CTX.GetComponentInChildren<KinematicCharacterMotor>();
        }
    }

    [SerializeField] float _Windup;
    float _WindupPulse;
    bool _ExecutingAttack;
    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        base.OnTick(CTX, TickRate);

        if (!_ExecutingAttack)
        {
            if (_Windup > _WindupPulse)
            {
                _ExecutingAttack = true;
                _MoveStrategy.ForceUngroundSelf(0.1f);

                _MoveStrategy.ApplyExternalForce(Vector3.up, _JumpStrenght);

            }
            else
            {
                _MoveStrategy.UpdateVelocity(Vector3.zero);
                _MoveStrategy.KillAllMovement();

                _WindupPulse = _Windup + (Time.deltaTime * TimeDilationManager.GlobalTimeScale);
            }
        }
        else
        {
         
            if(_ExecutingAttack == true && _FinishedAttack == false && _KCC.GroundingStatus.IsStableOnGround)
            {
             
                hits = Physics.OverlapSphere(_Model.transform.position, attackradius);

                if (hits.Length > 0)
                {
                    foreach (Collider collider in hits)
                    {
                        // busca solo player healthcomp
                        if (collider.TryGetComponent(out Health_Component playerHp))
                        {
                            if (playerHp == null || playerHp.Context == null)
                            {
                                continue;
                            }

                            if (playerHp.Context == _Main_State.playerContext)
                            {
                                continue;
                            }

                            DamageScore DMScore = new DamageScore();

                            DMScore.Attacker = _Main_State.playerContext;
                            DMScore.FactionID = _Main_State.playerContext.faction;
                            DMScore.DamageAmount = AttackDamage;
                            DMScore.ElementalDamage = ElementType.Physical;



                            Vector3 Dir = playerHp.Context.PlayerTransform.position - _Model.transform.position;
                            Dir.Normalize();

                            playerHp.TakeDamageWithKnockback(Dir, AttackKnockback, DMScore);


                        }
                    }
                }

                _FinishedAttack = true;
            }

            return;
        }
    }

    [SerializeField] Collider[] hits;


    private void OnDrawGizmos()
    {
        if (DrawWireframe && _FinishedAttack)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_Model.transform.position, attackradius);
        }
    }


    #region EnergyInterface
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

   
    public void SetCost(float NewCost)
    {
        EnergyCost = (int)NewCost;
    }
    #endregion
}
