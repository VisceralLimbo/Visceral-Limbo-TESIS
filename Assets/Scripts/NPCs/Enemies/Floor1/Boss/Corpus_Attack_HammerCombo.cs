using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpus_Attack_HammerCombo : BaseState, IStateEnergyCost
{

    [Header("References")]
    [SerializeField] AnimatorHandler _AnimHandler;
    [SerializeField] Corpus_Controller _Main_State;
    [SerializeField] IMovementStrategy _MoveStrat;
    [SerializeField] Transform _Target;
    [Space]

    [Header("Variables")]

    [SerializeField] string TransitionKey;
    [SerializeField] int EnergyCost;
    [SerializeField] float TimingDuration;
    [SerializeField] bool CanExit = false;
    [SerializeField] float _KnockbackValue;
    [SerializeField] float attackradius;
    [SerializeField] float AttackDamage;


    [Header("For Testing purposes")]
  
    [SerializeField] bool DrawWireframe;
    [SerializeField] Transform _Model;


    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        if (CanExit == false) 
        {
            TO = this;
            return false;
        }


        return base.EvaluateTransitions(GlobalParams, out TO);
    }

    public override void OnDeInitialize(VisceralStateMachine CTX)
    {
        base.OnDeInitialize(CTX);
    }

    Collider[] hits;
    public override void OnEnter(VisceralStateMachine CTX)
    {

        CTX.SetGlobalCondition(TransitionKey, false);
        print("Performing hammer swings, woosh!" + this.name); // el print estaba en cada tick lo pase aca
        base.OnEnter(CTX);
        CanExit = false;
        _Main_State.KillMovement();
        pulse = 0;
        DrawWireframe = true;
        _Target = _Main_State.Target;

        _AnimHandler.SetParameter("Corpus_Anim", "HammerCombo", AnimatorControllerParameterType.Trigger);


    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        _Main_State.NotifyAttackFinished();

        base.OnExit(CTX);
    }

    public override void OnInitialize(VisceralStateMachine CTX)
    {
        base.OnInitialize(CTX);
        _Main_State = GetComponentInParent<Corpus_Controller>();

        if (_MoveStrat == null) _MoveStrat = CTX.GetComponentInChildren<IMovementStrategy>(); 
    }

    float pulse = 0;

  
    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
       
        if(pulse < TimingDuration) 
        {
            pulse += Time.deltaTime;

            if(pulse > 0.5)
            {
                DrawWireframe = false;
            }

            //rotar hacia player
            if (_Target != null)
            {
                Vector3 Dir =  _Target.transform.position - _MoveStrat.GetKCC().Capsule.transform.position;
                _MoveStrat.UpdateRotation(Dir);
            }
        }
        else
        {
            hits = Physics.OverlapSphere(_Model.transform.position, attackradius);

            if (hits.Length > 0)
            {
                foreach (Collider collider in hits)
                {
                    // busca solo player healthcomp
                    if (collider.TryGetComponent(out Player_HealthComp playerHp))
                    {
                        if (playerHp.Context == _Main_State.playerContext)
                        {
                            continue;
                        }

                        var DamageScore = new DamageScore();
                        DamageScore.Attacker = _Main_State.playerContext;
                        DamageScore.Victim = playerHp.Context;
                        DamageScore.DamageAmount = AttackDamage;
                        DamageScore.ElementalDamage = ElementType.Physical;
                        DamageScore.FactionID = _Main_State.playerContext.faction;

                        Vector3 Dir = playerHp.Context.PlayerTransform.position - _Main_State.playerContext.PlayerTransform.position;
                        Dir.Normalize();

                        playerHp.TakeDamageWithKnockback(Dir,_KnockbackValue,DamageScore);
                    }
                }
            }

            pulse = 0;
            CanExit = true;
        }
      
    }

    public void SetCost(float newCost)
    {
        EnergyCost = (int)newCost;
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


    public void OnDrawGizmos()
    {
        if(DrawWireframe)
        {

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_Model.transform.position, attackradius);
        }


    }
}
