using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;



[System.Serializable]
public class Transition
{

    [Header("Condiciones")]
    /// <summary>
    /// El nombre de las condiciones que queremos ver evaluar
    /// </summary>
    [SerializeField] Condition[] conditions;


    [Space]

    [Header("Transiciones")]

    [Tooltip("De donde transicionamos, ignorado en caso de ser un AnyState")]
    [SerializeField] GameObject _FromGO; // el estado del que vinimos

    [Tooltip("Hacia donde transicionamos")]
    [SerializeField] GameObject _ToGO; // el estado al que vamos

    // cache de variables
    private BaseState _cachedTO;
    private bool _isToCached = false;
    public BaseState TargetState
    {
        get
        {
            if (!_isToCached && _ToGO != null)
            {
                _cachedTO = _ToGO.GetComponent<BaseState>();
                _isToCached = true;
            }
            return _cachedTO;
        }
    }

    [Space]

    [Header("Logica de transiciones")]
    /// <summary>
    /// Si esta transicion deberia de ignorar condiciones, 
    /// USAR SOLO SI SE DESEA QUE LA TRANSICION SEA GARANTIZADA
    /// </summary>

    [Tooltip("Si esta transicion deberia de ignorar condiciones, USAR SOLO SI SE DESEA QUE LA TRANSICION SEA GARANTIZADA")]
    [SerializeField] bool _ShouldTransitionWithNoConditions;

    /// <summary>
    /// Si deberíamos trancisionar a otro estado
    /// </summary>
    /// <param name="Conditions">El diccionario de todos los estados del FSM </param>
    /// <param name="TO"> El nuevo estado a transicionar ADVERTENCIA: CHEQUEAR NULL!</param>
    /// <returns></returns>
    public bool ShouldTransition(Dictionary<string,bool> GlobalParams, out BaseState TO)
    {
        if(TargetState == null)
        {
            Debug.LogError("Transition points to Null State: Gameobject is" +
                " missing the required State data ");
            TO = null; 
            return false;
        
        }

        if(_ShouldTransitionWithNoConditions)
        {
            TO = TargetState;
            return true;
        }

        foreach(var Condition in conditions)
        {
            if(GlobalParams.TryGetValue(Condition.ConditionName,out bool Value)) 
                // revisamos si tenemos un valor
            {
                if(Value != Condition.Value)
                {
                    //salimos porque una o mas condiciones fracasaron
                    TO = null;
                    return false;
                }
            }
        }

        TO = TargetState;
        return true;

    }

}
