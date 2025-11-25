using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpus_Attack_HammerCombo : BaseState, IStateEnergyCost
{

    [Header("References")]
    [SerializeField] AnimatorHandler _AnimHandler;
    [SerializeField] Corpus_Thinking_Main_State _Main_State;
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
        _Main_State.DeactivateEnergy(true);

        _AnimHandler.SetParameter("Corpus_Anim", "HammerCombo", AnimatorControllerParameterType.Trigger);

    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        print("Oh no, i finished swinging my hammer, clang!");
        base.OnExit(CTX);
    }

    public override void OnInitialize(VisceralStateMachine CTX)
    {
        base.OnInitialize(CTX);
        _Main_State = GetComponentInParent<Corpus_Thinking_Main_State>();
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

        }
        else
        {

            // refe del boss
            Boss_HealthComp attackerHP = CTX.transform.root.GetComponentInChildren<Boss_HealthComp>();

            if (attackerHP == null)
            {
                return; // al boss no
            }

            // debug purposes
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
                            return;
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
