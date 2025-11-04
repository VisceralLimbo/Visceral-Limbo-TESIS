using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpus_Attack_HammerCombo : BaseState, IStateEnergyCost
{

    [SerializeField] string TransitionKey;
    [SerializeField] Corpus_Thinking_Main_State _Main_State;
    [SerializeField] int EnergyCost;
    [SerializeField] float TimingDuration;
    [SerializeField] bool CanExit = false;


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

    public override void OnEnter(VisceralStateMachine CTX)
    {
        print("Starting Hammer ATTACK");
        print("Performing hammer swings, woosh!" + this.name); // el print estaba en cada tick lo pase aca
        base.OnEnter(CTX);
        CanExit = false;
        _Main_State.KillMovement();
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
        }
        else
        {
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
}
