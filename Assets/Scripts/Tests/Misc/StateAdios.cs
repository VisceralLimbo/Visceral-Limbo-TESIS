 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateAdios : BaseState
{
    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        StartCoroutine(Cooldown()); 
        Mytransitions[0].ShouldTransition(GlobalParams, out BaseState _TO);
        if (_TO != null)
        {
            TO = _TO;
            return true;
        }
        else
        {
            TO = null;
            return false;
        }
    }

    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(1);
        stateMachine.SetGlobalCondition("HOLA", true);
    }

    public override void OnEnter(VisceralStateMachine CTX)
    {
        print("ENTRANDO A ADIOS!");
        stateMachine.SetGlobalCondition("HOLA", false);
    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        print("SALIENDO DE ADIOS!");
    }

    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        print("TICKEANDO ADIOS!");
    }
}
