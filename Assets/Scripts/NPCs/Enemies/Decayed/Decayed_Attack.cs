using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class Decayed_Attack : BaseState
{
    [Header("References")]
    [SerializeField] IMovementStrategy _MovementStrategy;
    [SerializeField] AnimatorHandler _AnimHandler;
    [SerializeField] GameObject _BulletPrefab;
    [SerializeField] Transform _BulletSpawnPoint;
    [SerializeField] Transform _Target;
    [SerializeField] KinematicCharacterMotor _KCC;

    [Header("Variables")]
    [SerializeField] float _AttackSpeed;
    [SerializeField] float _SafeSpace,_FarAway;
    Vector3 TargetDirection;
    [SerializeField] bool _TargetIsTooClose,_TargetIsTooFar;

    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        if (_TargetIsTooClose || _TargetIsTooFar)
        {
            stateMachine.SetGlobalCondition("Attack", false);
            stateMachine.SetGlobalCondition("Moving", true);
        }
        else
        {
            stateMachine.SetGlobalCondition("Attack", true);
            stateMachine.SetGlobalCondition("Moving", false);
        }

        return base.EvaluateTransitions(GlobalParams, out TO);
    }

    public override void OnEnter(VisceralStateMachine CTX)
    {
        _TargetIsTooClose = false;
        _TargetIsTooFar = false;
        stateMachine.SetGlobalCondition("Moving", false);
        _MovementStrategy.KillAllMovement();

    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        stateMachine.SetGlobalCondition("Attack", false);
    }

    public override void OnInitialize(VisceralStateMachine CTX)
    {
        stateMachine = CTX;
        _Target = FindObjectOfType<Player_Movement>().transform;
        _KCC = CTX.GetComponentInChildren<KinematicCharacterMotor>();
        _AnimHandler = CTX.GetComponentInChildren<AnimatorHandler>();
        _MovementStrategy = stateMachine.GetComponentInChildren<IMovementStrategy>();
    }

    float pulse;
    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        // matar movimiento del personaje restante (entre updates)
        _MovementStrategy.KillAllMovement();

        // direccion del ataque
        TargetDirection = _Target.position - _KCC.Capsule.transform.position;

        float targetDistance = Vector3.Distance(_Target.position, _KCC.Capsule.transform.position);

        _MovementStrategy.UpdateRotation(TargetDirection);

        //check! el enemigo esta muy cerca
        if(_SafeSpace > targetDistance)
        {
            // salir del estado
            _TargetIsTooClose = true;
            return;
            
        }
        // segundo check, el enemigo esta muy lejos
        else if(_FarAway < targetDistance)
        {
            _TargetIsTooFar = true;
            return;
        }
        // esta en goldilocks zone
        else
        {
            _TargetIsTooFar = false;
            _TargetIsTooClose = false;
        }

        //periodo de cooldown entre ataque
        if(pulse < _AttackSpeed)
        {
            pulse += TickRate;
            return;
        }

        //realizar ataque
        _AnimHandler.SetParameter("Decayed", "Attack", AnimatorControllerParameterType.Trigger);

        //obtener el animador que usamos
        if(_AnimHandler.TryGetAnimator("Decayed",out Animator Anim))
        {
            // chequeo si termino la animacion actual
            if(Anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f)
            {
                //apuntado al player
                _BulletSpawnPoint.LookAt(_Target, _KCC.CharacterUp);

                // instanciado de bala
                var bullet =Instantiate(_BulletPrefab, _BulletSpawnPoint.position, _BulletSpawnPoint.rotation);
                bullet.GetComponent<BulletDumb>().SetOwner(CTX.gameObject, CTX.GetComponent<PlayerContext>());
                pulse = 0;
            }
        }
    
    }
}
