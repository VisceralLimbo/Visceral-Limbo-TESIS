 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VisceralStateMachine : MonoBehaviour
{
    /// <summary>
    /// Array con todos los States posibles
    /// </summary>
    /// 

    [Header("Setup")]
    [Tooltip("Todos los estados posibles")]
    [SerializeField] private BaseState[] _States;

    /// <summary>
    /// El State inicial
    /// </summary>
    [Tooltip("Estado inicial")]
    [SerializeField] private BaseState _StartingState;

    /// <summary>
    /// El State actual
    /// </summary>

    [Tooltip("Estado Actual siendo ejecutado")]
    [SerializeField] private BaseState _CurrentState;
    public BaseState CurrentState
    {
        get { return _CurrentState; }
    }

    [Tooltip("Ultimo Estado ejecutado")]
    [SerializeField] private BaseState _LastState;
    public BaseState LastState
    {
        get { return _LastState; }
    }
    [Space]

    [Header("Transition Data - BlackBoard")]




    /// <summary>
    /// Las condiciones / parametros del state machine (basicamente mi blackboard)
    /// </summary>
    [Tooltip("Condiciones de transicion posibles")]
    [SerializeField] List<Condition> Conditions = new List<Condition>();

    [Space]
    [Header("Transition Data - Any State")]
    [SerializeField] private Transition[] _AnyTransitionState;

    [Tooltip("si esta en TRUE, se priorizaran las transiciones del AnyState antes de las transiciones del estado actual")]
    [SerializeField] bool _PrioritizeAnyTransitions;
    /// <summary>
    /// diccionario de condiciones 
    /// </summary>
    Dictionary<string,bool> _GlobalConditions = new Dictionary<string,bool>();

    [Space]
    [Header("References")]
    [SerializeField]Health_Component _HPComp;




    private void Awake()
    {
        foreach(var condition in Conditions)
        {
            _GlobalConditions.TryAdd(condition.ConditionName,condition.Value);
        }
    }

    private void Start()
    {
        //inicializacion de estados
        foreach(var state in _States)
        {
            state.OnInitialize(this);
        }


        //logica de entrado al estado inicial
        if(_StartingState != null)
        {
            _StartingState.OnEnter(this);
            _CurrentState = _StartingState;
        }
        else
        {
            _StartingState = _States.FirstOrDefault();

            if(_StartingState!= null) 
            {
                _StartingState.OnEnter(this);
                _CurrentState = _StartingState;
            }
            else
            {
                Debug.LogError("<Color=blue> [Visceral Error] State Machine Start:" +
              "Missing Starting coroutine, please check in inspector");
            }
        }

        if(_HPComp == null)
        {
            if(gameObject.TryGetComponent(out Health_Component HPC))
            {
                _HPComp = HPC;
                _HPComp.OnDeath += OnDisable;
            }
        }
        else
        {

            _HPComp.OnDeath += OnDisable;
        }
    }

    public void Update()
    {
        if (!isActiveAndEnabled|| !gameObject.activeInHierarchy)
        {
            return;
        }

        if (_CurrentState == null)
        {
            Debug.LogError("<Color=blue>[Visceral Error] StateMachine: no hay estado actual");
            return;
        }

        _CurrentState.OnTick(this, Time.deltaTime);


        if (_PrioritizeAnyTransitions)
        {
            // Prioridad 1: Globales
            if (TryEvaluateAnyTransitions(out BaseState To)) { SwitchToNewState(_CurrentState, To); return; }
            // Prioridad 2: Locales
            if (_CurrentState.EvaluateTransitions(_GlobalConditions, out BaseState localTO)) { SwitchToNewState(_CurrentState, localTO); return; }
        }
        else
        {
            // Prioridad 1: Locales
            if (_CurrentState.EvaluateTransitions(_GlobalConditions, out BaseState localTO)) { SwitchToNewState(_CurrentState, localTO); return; }
            // Prioridad 2: Globales
            if (TryEvaluateAnyTransitions(out BaseState To)) { SwitchToNewState(_CurrentState, To); return; }
        }
    }

    public void SetGlobalCondition(string conditionName,bool value)
    {
        if(_GlobalConditions.ContainsKey(conditionName))
        {
            _GlobalConditions[conditionName] = value;

#if UNITY_EDITOR
            //delete in unity build

            var changed = Conditions.Find((X)=> X.ConditionName == conditionName);
            changed.Value = value;
#endif


        }
        else
        {
            Debug.LogError("<Color = blue> [Visceral Error]" +
                " State machine globalParameter NOT FOUND: " + conditionName);
        }
    }

    public bool GetGlobalCondition(string conditionName) 
    {
        if (_GlobalConditions.ContainsKey(conditionName))
        {
            return _GlobalConditions[conditionName];
        }
        else
        {
            Debug.LogError("<Color = blue> [Visceral Error]" +
            " State machine Getter GlobalParameter NOT FOUND: " + conditionName);
            return false;
        }
    }


    private void SwitchToNewState(BaseState FROM, BaseState TO)
    {
        //reset de condisiones reseteables
        foreach (var condition in Conditions)
        {
            if (condition.IsResetAfterSwitch) // la condicion analizada es reseteable
            {
                if (_GlobalConditions.ContainsKey(condition.ConditionName))
                {
                    _GlobalConditions[condition.ConditionName] = condition.Value;

#if UNITY_EDITOR
                    //delete in unity build

                    // Resetea el valor del INSPECTOR
                    var changed = Conditions.Find((X) => X.ConditionName == condition.ConditionName);
                    if (changed != null) // Pequeña seguridad extra
                    {
                        changed.Value = condition.Value;
                    }
#endif
                }


            } 
        }

        FROM.OnExit(this);
        _LastState = FROM;
        _CurrentState = TO;
        _CurrentState.OnEnter(this);

    }

    public void DeactivateMachine(bool ResetMachine)
    {
        this.enabled = false;

        if (ResetMachine)
        {
            foreach(var Condition in Conditions)
            {
                if (_GlobalConditions.ContainsKey(Condition.ConditionName))
                {
                    _GlobalConditions[Condition.ConditionName] = Condition.Value;
                }
                else
                {
                    _GlobalConditions[Condition.ConditionName] = false;
                }
            }
            _CurrentState = null;
        }
    }

    private bool TryEvaluateAnyTransitions(out BaseState AnyStateTO)
    {

        if(_AnyTransitionState == null || _AnyTransitionState.Length <= 0)
        {
            AnyStateTO = null;
            return false;
        }

        foreach(Transition AnyTransition in _AnyTransitionState)
        {
            if(AnyTransition.ShouldTransition(_GlobalConditions,out BaseState Candidate))
            {
                // ignoramos la transicion al estado actual
                if(Candidate != null && Candidate != _CurrentState)
                {
                    AnyStateTO = Candidate;
                    return true;
                }

            }
        }


        AnyStateTO = null;
        return false;
    }




    private void OnDisable()
    {
        DeactivateMachine(true);
    }
}



[System.Serializable]
public class Condition
{
    [SerializeField] public string ConditionName;
    [SerializeField] public bool Value;
    [Tooltip("Este booleano indica si la condicion tiene que ser reseteada al valor original indicado en inspector, EL VALOR NO NECESARIAMENTE SEA FALSE!")]
    [SerializeField] public bool IsResetAfterSwitch;
}