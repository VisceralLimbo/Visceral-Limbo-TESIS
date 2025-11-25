using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Corpus_Thinking_Main_State : BaseState
{
    [Header("References")]
    [SerializeField] Transform Target;
    [SerializeField] GameObject Model;
    [SerializeField] IMovementStrategy _MovementStrategy;
    [SerializeField] AnimatorHandler _Anim;
    Coroutine _EnergyCoroutine;

    [SerializeField] PlayerContext _PlayerContext;
    public PlayerContext playerContext { get { return _PlayerContext; } }

    [Header("Variables")]
    [Tooltip("Distancia minima para realizar un ataque, recubre tanto melee como rango")]
    [SerializeField] float _MinimumAttackRange;
    public float MinimumAttackRange { get { return _MinimumAttackRange; } }

    [Tooltip("Energia del Corpus, determina cuantas acciones puede hacer")]
    [SerializeField] int Energy;

    [Tooltip("Energia maxima del Corpus")]
    [SerializeField] int MaxEnergy;

    [Tooltip("Regeneracion del Corpus en segundos")]
    [SerializeField] float EnergyRefillRate;

    [Tooltip("Timer interno para determinar que tan responsiva la IA es")]
    [SerializeField] float _AIResponsiveness;
    [SerializeField] float _AIThoughPulse;
    [SerializeField] bool _CanMakeDecision;

    private bool _StopRegeneratingEnergy;

    [SerializeField] BaseState[] MeleeAttacks;
    [SerializeField] BaseState[] RangeAttacks;

    private List<IStateEnergyCost> AttacksCosts = new List<IStateEnergyCost>();
    private List<IStateEnergyCost> RangeAttackCosts = new List<IStateEnergyCost>();
    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        return base.EvaluateTransitions(GlobalParams, out TO);
    }

    public override void OnDeInitialize(VisceralStateMachine CTX)
    {
        
    }

    public override void OnEnter(VisceralStateMachine CTX)
    {

        if(Target == null)
        {
            Target = FindObjectOfType<Player_Movement>().transform;
        }

        if(Energy < MaxEnergy && !_StopRegeneratingEnergy)
        {
            if(_EnergyCoroutine == null)
            {
                print("activando regeneracion");
                _EnergyCoroutine = StartCoroutine(EnergyCoroutine());
            }

        }
        else if (_StopRegeneratingEnergy && _EnergyCoroutine != null)
        {
            StopCoroutine(_EnergyCoroutine);
            _EnergyCoroutine = null;
        }

        _AIThoughPulse = 0;
        _CanMakeDecision = false;
    }

    public override void OnExit(VisceralStateMachine CTX)
    {


    }

    public override void OnInitialize(VisceralStateMachine CTX)
    {
        Target = FindObjectOfType<Player_Movement>().transform;
        stateMachine = CTX;

        foreach(BaseState melee in MeleeAttacks)
        {
            if(melee.TryGetComponent(out IStateEnergyCost EnerCost))
            {
                AttacksCosts.Add(EnerCost);
            }
            
        }

        foreach (BaseState Range in RangeAttacks)
        {
            if(Range.TryGetComponent(out IStateEnergyCost Enerc))
            {
                RangeAttackCosts.Add(Enerc);
            }
        }

        if(_PlayerContext== null) 
        {
            _PlayerContext = this.GetComponentInParent<PlayerContext>();
        }
        _MovementStrategy = transform.parent.GetComponentInChildren<IMovementStrategy>();
    }

    float EnergyPulse = 0;
    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        if(Target == null)
        {
            return;
        }

        if (!_CanMakeDecision)
        {
            if (_AIThoughPulse <= _AIResponsiveness)
            {
                _AIThoughPulse += Time.deltaTime * TimeDilationManager.GlobalTimeScale;
            }
            else
            {
                print("puedo hacer algo");
                _CanMakeDecision = true;
                DeactivateEnergy(false);
            }
        }

        // meti adentro todo dentro del if
        if (_CanMakeDecision) //  se dice si esta
        {
            _AIThoughPulse = 0; // reset al pulse y hago lo q ya estaba

            // PASO 1: Calcular la distancia al jugador del Corpus
            float DistanceToPlayer = Vector3.Distance(Target.transform.position,Model.transform.position);


            if (DistanceToPlayer <= MinimumAttackRange)
            {
                stateMachine.SetGlobalCondition("InRange", true);
            }
            else
            {
                stateMachine.SetGlobalCondition("InRange", false);
            }


            // PASO 2: Determinar si puedo hacer un ataque
            if (Energy > 0)
            {
                var NextAttack = ChooseNextAttack(out IStateEnergyCost EnerCost);

                if (NextAttack != null)
                {
                    _Anim.SetParameter("Corpus_Anim", "SideWalk", AnimatorControllerParameterType.Bool, false);
                    _Anim.SetParameter("Corpus_Anim", "Walk", AnimatorControllerParameterType.Bool, false);
                    stateMachine.SetGlobalCondition("ShouldMove", false);
                    stateMachine.SetGlobalCondition("ShouldMoveAround", false);

                    stateMachine.SetGlobalCondition(EnerCost.GetTransitionKey(), true);


                    _CanMakeDecision = false; // Bloqueamos la decisión mientras se ejecuta el ataque

                    print("Next attack is " + NextAttack.name);
                    return;
                }
            }

            // PASO 2: MOVERNOS
            else if (DistanceToPlayer < MinimumAttackRange)
            {
                // MUY CERCA DEL PLAYER, ERGO ROTAMOS 
                stateMachine.SetGlobalCondition("ShouldMove", false);

                stateMachine.SetGlobalCondition("ShouldMoveAround", true);
                _CanMakeDecision = false; // Bloqueamos la decisión para esperar el cooldown/pensamiento
                                          // vamos a idle
                return;
            }
            else
            {
                // THAT MADAFAKA GOT THEM FAKE Js!!!
                stateMachine.SetGlobalCondition("ShouldMove", true);

                stateMachine.SetGlobalCondition("ShouldMoveAround", false);
                _CanMakeDecision = false; // Bloqueamos la decisión para esperar el cooldown/pensamiento
                                          // vamos a idle
                return;
            }

            // PASO 3: Determinar si vamos a Idle
            if (Target != null)
            {

                stateMachine.SetGlobalCondition("ShouldMove", false);
                stateMachine.SetGlobalCondition("ShouldMoveAround", false);
                _CanMakeDecision = false; // Bloqueamos la decisión para esperar el cooldown/pensamiento
                                          // idle
            } 
        }
    }


    private IEnumerator EnergyCoroutine()
    {
        while(Energy < MaxEnergy)
        {
            print("regenerando energia");
            if (EnergyPulse < EnergyRefillRate)
            {
                EnergyPulse += Time.deltaTime * TimeDilationManager.GlobalTimeScale;

            }
            else
            {
                EnergyPulse = 0;
                Energy++;
            }

            yield return null;
        }

        _EnergyCoroutine = null;
    }


    private BaseState ChooseNextAttack(out IStateEnergyCost IEnegyCost)
    {
        BaseState ChosenAttack = null;

        IStateEnergyCost[] PossibleAttacks = null;
        
        // paso 1) determinar que ataques posibles puedo hacer
        if(stateMachine.GetGlobalCondition("InRange") == true)
        {
            PossibleAttacks = AttacksCosts.ToArray();
        }
        else
        {
            PossibleAttacks = RangeAttackCosts.ToArray();
        }

        //paso 2) Determinar que ataques puedo comprar con mi energia actual

        IStateEnergyCost[] ViableAttacks = PossibleAttacks.Where(x => Energy - x.GetCost() >= 0).ToArray();

        // CATCH! no hay ataques viables
        if(ViableAttacks.Length <= 0)
        {
            IEnegyCost = null;
            return null;
        }

        // paso 3) elegimos un ataque random

        int RandomAttack = Random.Range(0,ViableAttacks.Length);

        
        ChosenAttack = ViableAttacks[RandomAttack].GetState();

        if(ChosenAttack != null)
        {
            Energy -= ViableAttacks[RandomAttack].GetCost();
            IEnegyCost = ViableAttacks[RandomAttack];
            return ChosenAttack;
        }
        else
        {
            IEnegyCost = null;
            return null;
        }
    } 


    public Transform GetTargetTransform { get { return Target.transform; } }

    public float GetCurrentEnergy { get { return Energy; } }

    public GameObject GetModel { get { return Model; } }

    public void KillMovement()
    {
        _MovementStrategy.KillAllMovement();
        _MovementStrategy.SetActiveState(false);
    }

    public void DeactivateEnergy(bool value)
    {
        _StopRegeneratingEnergy = value;

        if(value == true)
        {
            if(_EnergyCoroutine != null)
            {
                StopCoroutine(_EnergyCoroutine);
                _EnergyCoroutine = null;
            }
        }
        else
        {
            if(_EnergyCoroutine == null)
            {
               _EnergyCoroutine = StartCoroutine(EnergyCoroutine());
            }
           
        }
    }

}
