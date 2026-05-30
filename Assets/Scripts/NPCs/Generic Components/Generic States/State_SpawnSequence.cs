using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_SpawnSequence : BaseState
{
    [System.Serializable]
    private struct TransitionKeyConfiguration
    {
        [Header("Setup de transicion")]
        [SerializeField] public string TransitionKey;
        [SerializeField] public bool TransitionValue;
    }



    [Header("Variables")]
    [SerializeField] private float _Timer;
    [SerializeField] private float _Pulse;
    [SerializeField] private TransitionKeyConfiguration[] _TransitionKeys;

    [SerializeField] private bool _CanTransition = false;

    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        if (_CanTransition)
        {
            TO = null;
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
        base.OnEnter(CTX);
    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        base.OnExit(CTX);
    }

    public override void OnInitialize(VisceralStateMachine CTX)
    {
        base.OnInitialize(CTX);
    }

    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        if(_Pulse < _Timer)
        {
            _Pulse += Time.deltaTime * TimeDilationManager.GlobalTimeScale;
            return;
        }
        else
        {
            _CanTransition = true;

            foreach(var key in _TransitionKeys)
            {
                CTX.SetGlobalCondition(key.TransitionKey, key.TransitionValue);
            }
        }
        base.OnTick(CTX, TickRate);
    }


}

