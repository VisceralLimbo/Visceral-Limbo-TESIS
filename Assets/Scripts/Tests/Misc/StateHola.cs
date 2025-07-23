using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateHola : BaseState
{
    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {

        StartCoroutine(Cooldown());



        Mytransitions[0].ShouldTransition(GlobalParams,out BaseState _TO);
        if(_TO!=null)
        {
            TO = _TO;
            return true;
        }
        else
        {
            TO= null;
            return false;
        }
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(1);
        stateMachine.SetGlobalCondition("ADIOS", true);
    }

    public override void OnEnter(VisceralStateMachine CTX)
    {
        stateMachine.SetGlobalCondition("ADIOS", false);
        print("ENTRANDO A HOLA!");
    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        print("SALIENDO DE HOLA!");
      
    }

    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        print("TICKEANDO A HOLA!");
    }
}
