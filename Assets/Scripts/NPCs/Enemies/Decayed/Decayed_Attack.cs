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
    [SerializeField] GameObject flashAttackParticle;

    [Header("Variables")]
    [SerializeField] float _AttackSpeed;
    [SerializeField] float _SafeSpace,_FarAway;
    Vector3 TargetDirection;
    [SerializeField] bool _TargetIsTooClose,_TargetIsTooFar;

    bool isFlashing = false;

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
        _AnimHandler.SetParameter("Decayed", "Idle", AnimatorControllerParameterType.Trigger);
        _MovementStrategy.KillAllMovement();
        pulse = 0;

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

    float pulse = 0;
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
            print("Target too close");
            // salir del estado
            _TargetIsTooClose = true;
            return;
            
        }
        // segundo check, el enemigo esta muy lejos
        else if(_FarAway < targetDistance)
        {
            print("Target too Far");
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

        // Si no está haciendo flash, iniciarlo
        if (!isFlashing)
        {
            CTX.StartCoroutine(FlashThenShoot());
        }
    }

    private IEnumerator FlashThenShoot()
    {
        isFlashing = true;

        // prefab de la paritucla
        if (flashAttackParticle != null)
        {
            GameObject flashGO = Instantiate(flashAttackParticle, _BulletSpawnPoint.position, _BulletSpawnPoint.rotation);
            ParticleSystem flashPS = flashGO.GetComponent<ParticleSystem>();
            if (flashPS != null)
                flashPS.Play();

            // destruyo dsp de la duracion
            Destroy(flashGO, flashPS.main.duration); ;
        }

        // espero 1 seg para disparar (ver de bajar subir etc)
        yield return new WaitForSeconds(1f);

        // Animación y disparo de bala
        _AnimHandler.SetParameter("Decayed", "Attack", AnimatorControllerParameterType.Trigger);

        //como estaba lo anterior
        var correctTarget = _Target.position + Vector3.up;
        _BulletSpawnPoint.LookAt(correctTarget, _KCC.CharacterUp);

        var bullet = Instantiate(_BulletPrefab, _BulletSpawnPoint.position, _BulletSpawnPoint.rotation);
        bullet.GetComponent<BulletDumb>().SetOwner(stateMachine.gameObject, stateMachine.GetComponent<PlayerContext>());

        pulse = 0;
        isFlashing = false;
    }
}
