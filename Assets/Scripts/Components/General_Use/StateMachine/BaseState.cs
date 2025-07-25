using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base State es el estado base de todos los states implementados por el StateMachine
/// </summary>
public class BaseState : MonoBehaviour
{
    [SerializeField] protected VisceralStateMachine stateMachine;

    [SerializeField] public Transition[] Mytransitions;

    [SerializeField] protected float _MinStateLifetime;

    /// <summary>
    /// Funcion de inicializacion, usar para cargar transiciones
    /// </summary>
    /// <param name="CTX"></param>
    public virtual void OnInitialize(VisceralStateMachine CTX) 
    {
        stateMachine = CTX;
    }

    /// <summary>
    /// Funcion de DesInicializacion, usar para descargar transiciones
    /// </summary>
    /// <param name="CTX"></param>
    public virtual void OnDeInitialize(VisceralStateMachine CTX) { }

    /// <summary>
    /// Funcion de entrada al estado
    /// </summary>
    /// <param name="CTX"></param>
    public virtual void OnEnter(VisceralStateMachine CTX) { }

    /// <summary>
    /// Funcion de actualizacion de estado
    /// </summary>
    /// <param name="CTX"></param>
    /// <param name="TickRate"> velocidad de actualizacion de estado, similar al time.deltaTime</param>
    public virtual void OnTick(VisceralStateMachine CTX,float TickRate) { }

    /// <summary>
    /// Funcion para evaluar las transiciones
    /// </summary>
    /// <param name="CTX"></param>
    public virtual bool EvaluateTransitions(Dictionary<string,bool> GlobalParams,out BaseState TO)
    {
        foreach(Transition condition in Mytransitions)
        {
           var result = condition.ShouldTransition(GlobalParams,out BaseState newTrans);

            if (result)
            {
                TO = newTrans;
                return result;
            } 
        }

        TO = null;
        return false;
    }

    /// <summary>
    /// Funcion de salida de estado
    /// </summary>
    /// <param name="CTX"></param>
    public virtual void OnExit(VisceralStateMachine CTX) { }

}
