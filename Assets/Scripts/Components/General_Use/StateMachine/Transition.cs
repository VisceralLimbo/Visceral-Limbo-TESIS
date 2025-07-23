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
    [SerializeField] string[] ConditionNames;
    [Space]

    [Header("Transiciones")]
    [SerializeField] GameObject _FromGO; // el estado del que vinimos
    [SerializeField] GameObject _ToGO; // el estado al que vamos
    BaseState _TO => _ToGO.GetComponent<BaseState>();
    BaseState _FROM => _FromGO.GetComponent<BaseState>();

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
        if(_TO == null)
        {
            Debug.LogError("Transition points to Null State: Gameobject is" +
                " missing the required State data: " + _ToGO.name );
            TO = null; 
            return false;
        
        }

        if(_ShouldTransitionWithNoConditions)
        {
            TO = _TO;
            return true;
        }

        foreach(var Condition in ConditionNames)
        {
            if(GlobalParams.TryGetValue(Condition,out bool Value)) // revisamos si tenemos un valor
            {
                if(!Value)
                {
                    TO = null;
                    return false;
                }
            }
        }

        TO = _TO;
        return true;

    }
}

