using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Corpus_FlamePillar_Attack : BaseState, IStateEnergyCost
{
    [Header("Configuración del Pilar")]
    public Transform playerTarget;
    // Tipado fuerte: adiós GameObject y GetComponents posteriores
    public FlamePillarIndicator indicatorPrefab;
    [SerializeField] float _timeBetweenPillars = 0.5f;
    [SerializeField] int _numberOfPillars = 5;
    [SerializeField] float _indicatorDuration = 1.5f;
    [SerializeField] private float maxPillarTargetDistance = 200f;

    [Header("Fase 2")]
    [SerializeField] private Vector2Int _minMaxSecondPhasePillarsCount;
    [SerializeField] private Vector2 _minPillarRandomPosition;
    [SerializeField] private Vector2 _maxPillarRandomPosition;
    [SerializeField] private bool _secondPhase;

    [Header("Raycast")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float raycastDistance = 100f;

    [Header("State Machine Setup")]
    [SerializeField] private string transitionKey;
    [SerializeField] private int energyCost;

    [Header("Referencias")]
    [SerializeField] private Corpus_Controller thinker;
    [SerializeField] private AnimatorHandler animHandler;
    [SerializeField] private Boss_HealthComp bossHP;
    [SerializeField] private PlayerContext _Context;

    private IMovementStrategy _moveStrat;
    private bool _isAttacking;
    private Coroutine _attackRoutine;



    public override void OnInitialize(VisceralStateMachine CTX)
    {
        if (thinker == null) thinker = GetComponentInParent<Corpus_Controller>();
        playerTarget = thinker.Target;


        if(bossHP != null)
        {
            bossHP.OnSecondPhase += ToggleSecondPhase;
        }

        if(_moveStrat == null)
        {
            //_moveStrat = CTX.GetComponentInChildren<IMovementStrategy>();
            _moveStrat = thinker.MovementStrategy;
        }


        print("Finished On Initialize");
        base.OnInitialize(CTX);
    }

    public override void OnEnter(VisceralStateMachine CTX)
    {
        base.OnEnter(CTX);

        if (_moveStrat == null) 
        {
            _moveStrat = thinker.MovementStrategy;
        }
        if(_moveStrat != null)
        {
            _moveStrat.UpdateVelocity(Vector3.zero);
            _moveStrat.KillAllMovement();
        }

        if(playerTarget == null)
        {
            playerTarget = thinker.Target;
        }

        animHandler.SetParameter("Corpus_Anim", "PilarAttack", AnimatorControllerParameterType.Bool, true);

        if (_attackRoutine != null) StopCoroutine(_attackRoutine);
        _attackRoutine = StartCoroutine(PillarAttackCoroutine());
    }

    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {

        _moveStrat.UpdateVelocity(Vector3.zero);

        base.OnTick(CTX, TickRate);
    }

    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        if (_isAttacking)
        {
            TO = null;
            return false;
        }

        stateMachine.SetGlobalCondition("Attack_Pilar", false);
        stateMachine.SetGlobalCondition(thinker.CheckDistanceForTransitions(), true);


        return base.EvaluateTransitions(GlobalParams, out TO);
    }

    public override void OnExit(VisceralStateMachine CTX)
    {

        animHandler.SetParameter("Corpus_Anim", "PilarAttack", AnimatorControllerParameterType.Bool, false);

        if(_attackRoutine != null) StopCoroutine(_attackRoutine);

        thinker.NotifyAttackFinished();
        base.OnExit(CTX);
    }


    private IEnumerator PillarAttackCoroutine()
    {
        _isAttacking = true;
        yield return new WaitForSeconds(0.5f); // HACK! switchear a futuro para mejor comunicacion ataque
                                               // simula el startup de una animacion de ataque.

        if (!CanTargetPlayerWithPillars())
        {
            _isAttacking = false;
            yield break;
        }

        for (int i = 0; i < _numberOfPillars; i++)
        {
            SpawnSinglePillar(playerTarget.position);

            // Si estamos en fase 2, instanciar los pilares extra
            if (_secondPhase)
            {
                int extraPillars = Random.Range(_minMaxSecondPhasePillarsCount.x, _minMaxSecondPhasePillarsCount.y + 1);

                for (int j = 0; j < extraPillars; j++)
                {
                    // Calculamos un offset aleatorio
                    Vector3 randomOffset = new Vector3(
                        Random.Range(_minPillarRandomPosition.x, _maxPillarRandomPosition.x),
                        0,
                        Random.Range(_minPillarRandomPosition.y, _maxPillarRandomPosition.y)
                    );

                    Vector3 OwnPosition = _Context.PlayerTransform.position;

                    // Tomamos la posicion del jugador y le sumamos el offset
                    Vector3 randomPos = OwnPosition + randomOffset;
                    randomPos.y += 1f; // Lo elevamos un poco para asegurarnos de que el Raycast hacia abajo funcione bien

                    SpawnSinglePillar(randomPos);
                }
            }

            yield return new WaitForSeconds(_timeBetweenPillars);
        }

        _isAttacking = false;
    }

    private void SpawnSinglePillar(Vector3 CastOrigin)
    {
        Vector3 finalPos = CastOrigin;

        if(Physics.Raycast(CastOrigin,Vector3.down,out RaycastHit Hit, raycastDistance, groundLayer))
        {
            finalPos = Hit.point + (Vector3.up * 0.01f);
        }

        FlamePillarIndicator Indicator = Instantiate(indicatorPrefab, finalPos, Quaternion.Euler(90f, 0f, 0f));
        Indicator.SetupIndicator(finalPos, _indicatorDuration,thinker.playerContext);

    }


    #region IEnergyStateCost
    public int GetCost() => energyCost;


    public BaseState GetState() => this;
  

    public string GetTransitionKey() => transitionKey;


    public void SetCost(float NewCost) => energyCost = (int)NewCost;
  


    private void ToggleSecondPhase() => _secondPhase = true;
    #endregion

    private bool CanTargetPlayerWithPillars()
    {
        if (playerTarget == null) return false;

        float distance = Vector3.Distance(transform.position, playerTarget.position);
        return distance <= maxPillarTargetDistance;
    }
}
