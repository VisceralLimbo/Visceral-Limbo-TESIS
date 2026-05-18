using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Corpus_Controller : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Referencia a la máquina de estados que este cerebro va a controlar")]
    [SerializeField] private VisceralStateMachine _stateMachine;
    [SerializeField] private Transform _target;
    public Transform Target { get { return _target; } }


    [SerializeField] private IMovementStrategy _movementStrategy;
    public IMovementStrategy MovementStrategy
    {
        get
        {
            if (_movementStrategy == null)
            {
                // Unity busca directamente cualquier componente en los hijos que implemente la interfaz
                _movementStrategy = GetComponentInChildren<IMovementStrategy>();

                if (_movementStrategy == null)
                {
                    Debug.LogError("<color=red>[Corpus_Controller Error]</color> No se encontró ningún componente que implemente IMovementStrategy en este GameObject ni en sus hijos.");
                }
            }
            return _movementStrategy;
        }
    }

    [SerializeField] private AnimatorHandler _anim;
    [SerializeField] private PlayerContext _playerContext;
    [SerializeField] private Boss_HealthComp _HealthComp;
    public PlayerContext playerContext { get { return _playerContext; } }

    [Header("Variables de Combate")]
    [Tooltip("Distancia minima para realizar un ataque (cubre melee y rango)")]
    [SerializeField] private float _minimumAttackRange;

    [Header("Variables de Energia")]
    [SerializeField] private float _energy;
    [SerializeField] private int _maxEnergy;
    [SerializeField] private float _energyRefillRate;
    [SerializeField] private float _ExtraEnergyRefillPerHit;
    [SerializeField] private float _SecondPhaseExtraEnergtRefillPerHit;
    private Coroutine _energyCoroutine;
    [SerializeField] bool _SecondPhase;
    [SerializeField] private float _SecondPhaseRefillRate;

    [Tooltip("Bloquea la toma de decisiones y la regeneracion de energia mientras ataca")]
    [SerializeField]private bool _isAttacking;

    [Header("Configuracion IA")]
    [SerializeField] private float _aiResponsiveness;
    private float _aiThoughtPulse;
    [SerializeField] private bool _canMakeDecision;

    [Header("Datos de Ataques")]
    [SerializeField] private BaseState[] _meleeAttacks;
    [SerializeField] private BaseState[] _rangeAttacks;
    [SerializeField] private BaseState _ChosenState;

    private List<IStateEnergyCost> _meleeAttackCosts = new List<IStateEnergyCost>();
    private List<IStateEnergyCost> _rangeAttackCosts = new List<IStateEnergyCost>();

    // Variables de memoria de la IA
    private IStateEnergyCost _lastAttack = null;
    private IStateEnergyCost _pendingAttack = null;

    private void Awake()
    {
        CacheAttackCosts();
        _movementStrategy = GetComponentInChildren<IMovementStrategy>();
    }

    private void Start()
    {
        if (_target == null)
        {
            var player = FindObjectOfType<Player_Base>().GetComponent<PlayerContext>();
            if (player != null) _target = player.PlayerTransform;
        }

        if(_HealthComp == null)
        {
            _HealthComp = GetComponentInChildren<Boss_HealthComp>();
        }
        _HealthComp.OnDamaged += ExtraEnergy;
        _HealthComp.OnSecondPhase += EnteredSecondPhase;


        StartEnergyRegen();
    }

    private void CacheAttackCosts()
    {
        // Extraemos los costos de los estados asignados en el inspector
        foreach (var melee in _meleeAttacks)
        {
            if (melee.TryGetComponent(out IStateEnergyCost cost))
                _meleeAttackCosts.Add(cost);
        }

        foreach (var range in _rangeAttacks)
        {
            if (range.TryGetComponent(out IStateEnergyCost cost))
                _rangeAttackCosts.Add(cost);
        }
    }

    private void Update()
    {
        // Si el target esta muerto o atacando, no procesa logica de decision
        if (_target == null || _isAttacking) return;

        // 1. Tick interno de pensamiento (Responsiveness)
        if (!_canMakeDecision)
        {
            TickResponsiveness();
            return;
        }

        // 2. Tomar Decision
        MakeDecision();
    }

    private void TickResponsiveness()
    {
        if (_aiThoughtPulse <= _aiResponsiveness)
        {
            _aiThoughtPulse += Time.deltaTime * TimeDilationManager.GlobalTimeScale;
        }
        else
        {
            _canMakeDecision = true;
        }
    }

    private void MakeDecision()
    {
        _aiThoughtPulse = 0f; // Reseteamos el timer para la proxima decisión

        float distanceToPlayer = Vector3.Distance(_target.position, _playerContext.PlayerTransform.position);
        bool inRange = distanceToPlayer <= _minimumAttackRange;

        // Avisamos a la FSM en qué estado de distancia nos encontramos
        _stateMachine.SetGlobalCondition("InRange", inRange);

        // 1. Si no tenemos un ataque en mente (o el jugador salio/entro de rango haciendo invalido el ataque actual), elegimos uno nuevo.
        if (_pendingAttack == null || !IsPendingAttackValidForRange(inRange))
        {
            _pendingAttack = ChooseNextAttack(inRange);
            _ChosenState = _pendingAttack.GetState();
        }

        // 2. Si tenemos un ataque en mente, evaluamos si podemos pagarlo
        if (_pendingAttack != null)
        {
            if (_energy >= _pendingAttack.GetCost())
            {
                // TIENE ENERGÍA => COMPRA Y EJECUTA
                _energy -= _pendingAttack.GetCost();
                _lastAttack = _pendingAttack;

                var attackToExecute = _pendingAttack;
                _pendingAttack = null; // Limpiamos la intencion porque ya lo vamos a ejecutar

                ExecuteAttack(attackToExecute);
            }
            else
            {
                // NO TIENE ENERGÍA => QUEDA EN MOVIMIENTO HASTA RECARGAR
                HandleMovement(inRange);
            }
        }
        else
        {
            // Failsafe en caso de que no haya ataques configurados o todos filtren nulo
            HandleMovement(inRange);
        }
    }

    private void ExecuteAttack(IStateEnergyCost attackCost)
    {
        _isAttacking = true;
        _canMakeDecision = false;

        PauseEnergyRegen();

        // Limpiar condiciones de movimiento para que la FSM no se confunda al transicionar
        _anim.SetParameter("Corpus_Anim", "SideWalk", AnimatorControllerParameterType.Bool, false);
        _anim.SetParameter("Corpus_Anim", "Walk", AnimatorControllerParameterType.Bool, false);
        _stateMachine.SetGlobalCondition("ShouldMove", false);
        _stateMachine.SetGlobalCondition("ShouldMoveAround", false);

        // settear la condicion global a true
        _stateMachine.SetGlobalCondition(attackCost.GetTransitionKey(), true);
    }

    private void HandleMovement(bool inRange)
    {
        _canMakeDecision = false; // Bloqueamos para que espere al proximo pulso de pensamiento

        if (inRange)
        {
            // Esta muy cerca pero no tiene energia => Rotar / Rodear al jugador
            _stateMachine.SetGlobalCondition("ShouldMove", false);
            _stateMachine.SetGlobalCondition("ShouldMoveAround", true);
        }
        else
        {
            // Esta lejos y no tiene energia => Perseguir al jugador
            _stateMachine.SetGlobalCondition("ShouldMove", true);
            _stateMachine.SetGlobalCondition("ShouldMoveAround", false);
        }
    }

    private IStateEnergyCost ChooseNextAttack(bool inRange)
    {
        // si estamos en rango => elegimos melee attacks /else/ elegimos ranged
        var possibleAttacks = inRange ? _meleeAttackCosts : _rangeAttackCosts;

        // SOLO filtramos para no repetir el ultimo ataque.
        var viableAttacks = possibleAttacks
            .Where(x => x != _lastAttack)
            .ToArray();

        // Fallback de seguridad: si solo hay 1 ataque configurado, permitimos que se repita
        if (viableAttacks.Length == 0 && possibleAttacks.Count > 0)
        {
            viableAttacks = possibleAttacks.ToArray();
        }
        else if (viableAttacks.Length == 0)
        {
            return null; // No hay ataques disponibles en absoluto
        }

        int randomIndex = Random.Range(0, viableAttacks.Length);
        return viableAttacks[randomIndex];
    }

    /// <summary>
    /// Verificamos que todavia sea valido el ataque elegido. por que a lo mejor el jugador abandono la distancia de ataque maximo
    /// </summary>
    private bool IsPendingAttackValidForRange(bool inRange)
    {
        if (_pendingAttack == null) return false;

        if (inRange)
        {
            return _meleeAttackCosts.Contains(_pendingAttack);
        }
        else
        {
            return _rangeAttackCosts.Contains(_pendingAttack);
        }
    }

    //  MANEJO DE ENERGIA 

    private void StartEnergyRegen()
    {
        if (_energyCoroutine == null && !_isAttacking)
        {
            _energyCoroutine = StartCoroutine(EnergyCoroutine());
        }
    }

    private void PauseEnergyRegen()
    {
        if (_energyCoroutine != null)
        {
            StopCoroutine(_energyCoroutine);
            _energyCoroutine = null;
        }
    }

    private IEnumerator EnergyCoroutine()
    {
        float energyPulse = 0f;
        while (true)
        {
            if (_energy < _maxEnergy && !_isAttacking)
            {
                if (energyPulse < _energyRefillRate)
                {
                    energyPulse += Time.deltaTime * TimeDilationManager.GlobalTimeScale;
                }
                else
                {
                    energyPulse = 0f;
                    _energy++;
                }
            }
            yield return null;
        }
    }

    // COMUNICACION DESDE LA FSM HACIA EL CEREBRO 

    /// <summary>
    /// Funcion para avisar al Controller que se finalizo el attack.
    /// </summary>
    public void NotifyAttackFinished()
    {
        _isAttacking = false;
        StartEnergyRegen();
    }

    public string CheckDistanceForTransitions()
    {
        float distanceToPlayer = Vector3.Distance(_target.position, _playerContext.PlayerTransform.position);
        bool inRange = distanceToPlayer <= _minimumAttackRange;

        if (!inRange)
        {
            return "ShouldMove";
        }
        else
        {
            return "ShouldMoveAround";
        }
    }

    public void KillMovement()
    {
        if (_movementStrategy != null)
        {
            _movementStrategy.KillAllMovement();
            _movementStrategy.SetActiveState(false);
        }
    }

    private void EnteredSecondPhase()
    {
        _SecondPhase = true;
        _energyRefillRate = _SecondPhaseRefillRate;
    }

    private void ExtraEnergy()
    {
        if((_energy + _SecondPhaseExtraEnergtRefillPerHit) >= _maxEnergy)
        {
            return;
        }

        if(_SecondPhase)
        {
            _energy += _SecondPhaseExtraEnergtRefillPerHit;
        }
        else
        {
            _energy += _ExtraEnergyRefillPerHit;
        }

    }
}

