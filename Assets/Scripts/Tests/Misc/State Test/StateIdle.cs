using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateIdle : BaseState
{
    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        return base.EvaluateTransitions(GlobalParams, out TO);
    }

    public override void OnEnter(VisceralStateMachine CTX)
    {
        base.OnEnter(CTX);
    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        base.OnExit(CTX);
    }

    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        base.OnTick(CTX, TickRate);
    }
}
