using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;


public class Corpus_ButtSlam : BaseState, IStateEnergyCost
{
    [Header("Energy Setup")]
    [SerializeField] int EnergyCost;
    [SerializeField] string TransitionKey;
    [Space]

    [Header("References")]
    [SerializeField] Transform _Target;
    [SerializeField] Corpus_Controller _Main_State;
    IMovementStrategy _MoveStrategy;
    [SerializeField] KinematicCharacterMotor _KCC;
    [SerializeField] AnimatorHandler _AnimatorHandler;
    [SerializeField] DamageCollisionTrigger _KnockbackTrigger;
    [SerializeField] VFXPlayer _ButtSlamParticles;

    [Space]

    [Header("Variables")]
    [SerializeField] float _JumpStrenght;
    [SerializeField] float _JumpDuration; // Nota: Esto no se usa actualmente en la lógica, solo el físico
    [SerializeField] bool _FinishedAttack;

    float _AirTimer;
    [SerializeField] float _MinAirTime = 0.2f;
    [Space]


    [Header("For Testing purposes")]
    [SerializeField] float attackradius;
    [SerializeField] float AttackDamage;
    [SerializeField] float AttackKnockback;
    [SerializeField] bool DrawWireframe;
    [SerializeField] Transform _Model;

    [SerializeField] Collider[] hits;

    [SerializeField] float _SlamToIdleTimer;
    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        // Solo intentamos transicionar si el ataque terminó Y estamos en el suelo
        if (_FinishedAttack && _KCC.GroundingStatus.IsStableOnGround)
        {
            if(_MinStateLifetime>= _SlamToIdleTimer)
            {
                _Main_State.NotifyAttackFinished();
                return base.EvaluateTransitions(GlobalParams, out TO);
            }
            else
            {
                _MinStateLifetime += Time.deltaTime * TimeDilationManager.GlobalTimeScale;
                TO = null;
                return false;
            }
        }
        else
        {
            TO = null;
            return false;
        }
    }


    public override void OnEnter(VisceralStateMachine CTX)
    {
        base.OnEnter(CTX);

        _AnimatorHandler.SetParameter("Corpus_Anim", "ButtSlam", AnimatorControllerParameterType.Trigger);
        _MoveStrategy.KillAllMovement();

        // Reset de variables
        _ExecutingAttack = false;
        _FinishedAttack = false;
        _WindupPulse = 0;
        _AirTimer = 0;

        _MinStateLifetime = 0;
    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        base.OnExit(CTX);
        _Main_State.NotifyAttackFinished();
        _ExecutingAttack = false;
        _FinishedAttack = false;
        CTX.SetGlobalCondition(TransitionKey, false);
    }

    public override void OnInitialize(VisceralStateMachine CTX)
    {
        base.OnInitialize(CTX);

        if (_Main_State == null) _Main_State = GetComponentInParent<Corpus_Controller>();
        if (_MoveStrategy == null) _MoveStrategy = CTX.GetComponentInChildren<IMovementStrategy>();
        if (_Target == null) _Target = _Main_State.Target;
        if (_KCC == null) _KCC = CTX.GetComponentInChildren<KinematicCharacterMotor>();
    }

    [SerializeField] float _Windup;
    [SerializeField] float _WindupPulse;
    [SerializeField] bool _ExecutingAttack;

    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
       
        if (_FinishedAttack) return;

        // PASO 1: WINDUP
        if (!_ExecutingAttack)
        {
            _WindupPulse += (Time.deltaTime * TimeDilationManager.GlobalTimeScale);

            if (_Windup <= _WindupPulse)
            {
                // INICIAMOS EL SALTO
                _ExecutingAttack = true;
                _AirTimer = 0;

                _AnimatorHandler.SetParameter("Corpus_Anim", "ButtSlamAir", AnimatorControllerParameterType.Trigger);
                _MoveStrategy.ForceUngroundSelf(0.1f);
                _MoveStrategy.ApplyExternalForce(Vector3.up, _JumpStrenght); 
                _KnockbackTrigger.Activate(true);
            }
            else
            {
                
                // matamos movimiento
                _MoveStrategy.UpdateVelocity(Vector3.zero);
                _MoveStrategy.KillAllMovement();
            }
        }
        // PASO 2: Estamos en el aire, así que a mantenernos
        else
        {
            _AirTimer += Time.deltaTime * TimeDilationManager.GlobalTimeScale;

            if(_AirTimer < _MinAirTime && _KCC.GroundingStatus.IsStableOnGround)
            {
                _MoveStrategy.ForceUngroundSelf(0.1f);
                _MoveStrategy.ApplyExternalForce(Vector3.up, _JumpStrenght);
            }


            // Solo chequeamos si aterrizó si YA pasamos el tiempo mínimo de aire (_MinAirTime).
            // Esto evita que detecte el suelo en el mismo frame que saltó.
            if (_AirTimer > _MinAirTime && _KCC.GroundingStatus.IsStableOnGround)
            {
                LandingLogic();
            }
        }
    }

    void LandingLogic()
    {
        _KnockbackTrigger.Activate(false);
        hits = Physics.OverlapSphere(_Model.transform.position, attackradius);

        if (hits.Length > 0)
        {
            // Lista para evitar doble daño a la misma entidad con multiples colliders
            List<Health_Component> damagedTargets = new List<Health_Component>();

            foreach (Collider collider in hits)
            {
                if (collider.TryGetComponent(out Health_Component playerHp))
                {
                    if (playerHp == null || playerHp.Context == null) continue;

                    // Ignorar self y duplicados
                    if (playerHp.Context == _Main_State.playerContext) continue;
                    if (damagedTargets.Contains(playerHp)) continue;

                    DamageScore DMScore = new DamageScore();
                    DMScore.Attacker = _Main_State.playerContext;
                    DMScore.FactionID = _Main_State.playerContext.faction;
                    DMScore.DamageAmount = AttackDamage;
                    DMScore.ElementalDamage = ElementType.Physical;

                    Vector3 Dir = playerHp.Context.PlayerTransform.position - _Model.transform.position;
                    Dir.Normalize();
                    Dir.y = 0.7f;

                    playerHp.TakeDamageWithKnockback(Dir, AttackKnockback, DMScore);

                    damagedTargets.Add(playerHp);
                }
            }
        }

        _AnimatorHandler.SetParameter("Corpus_Anim", "ButtSlamToIdle", AnimatorControllerParameterType.Trigger);
        if (_ButtSlamParticles != null) _ButtSlamParticles.PlayAllParticles();
        _FinishedAttack = true;

        stateMachine.SetGlobalCondition("Attack_ButtSlam", false);
        stateMachine.SetGlobalCondition(_Main_State.CheckDistanceForTransitions(), false);
    }


    private void OnDrawGizmos()
    {
        if (DrawWireframe) 
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_Model.transform.position, attackradius);
        }
    }


    #region EnergyInterface
    public int GetCost()
    {
        return EnergyCost;
    }

    public BaseState GetState()
    {
        return this;
    }

    public string GetTransitionKey()
    {
        return TransitionKey;
    }


    public void SetCost(float NewCost)
    {
        EnergyCost = (int)NewCost;
    }
    #endregion

}
