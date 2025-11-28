using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class Decayed_Attack : BaseState
{
    [Header("References")]
    [SerializeField] IMovementStrategy _MovementStrategy;
    [SerializeField] AnimatorHandler _AnimHandler;
    [SerializeField] Animator _Anim;
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

    [SerializeField] private AudioSource _AudioSource;
    [SerializeField] private AudioClip flashSound;
    Coroutine _FlashingCoroutine;

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
        StopAllCoroutines();
        _FlashingCoroutine = null;
    }

    public override void OnInitialize(VisceralStateMachine CTX)
    {
        stateMachine = CTX;
        _Target = FindObjectOfType<Player_Movement>().transform;
        _KCC = CTX.GetComponentInChildren<KinematicCharacterMotor>();
        _AnimHandler = CTX.GetComponentInChildren<AnimatorHandler>();
        _MovementStrategy = stateMachine.GetComponentInChildren<IMovementStrategy>();
        _AnimHandler.TryGetAnimator("Decayed", out Animator _Anime);
        _Anim = _Anime;
    }

    float pulse = 0;
    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        // matar movimiento del personaje restante (entre updates)
        _MovementStrategy.KillAllMovement();

        // direccion del ataque
        TargetDirection = _Target.position - _KCC.Capsule.transform.position;

        float targetDistance = Vector3.Distance(_Target.position, _KCC.Capsule.transform.position);

        _MovementStrategy.UpdateRotation(TargetDirection.normalized);

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

        // Si no está haciendo flash, iniciarlo
        if (!isFlashing && _FlashingCoroutine == null)
        {
            _FlashingCoroutine = StartCoroutine(FlashThenShoot());
        }
    }

    private IEnumerator FlashThenShoot()
    {
        isFlashing = true;



        // Animación y disparo de bala
        _AnimHandler?.SetParameter("Decayed", "Attack", AnimatorControllerParameterType.Trigger);

        //esperamos a que estemos oficialment en el estado de ataque
        yield return new WaitUntil(() => _Anim !=  null || _Anim.GetCurrentAnimatorStateInfo(0).IsName("Decayed AttackAnim"));

        // esperamos a estar en el frame de ataque
        yield return new WaitUntil(() =>_Anim != null || _Anim.GetCurrentAnimatorStateInfo(0).normalizedTime % 1.0f >= 0.7f);

        if (_Anim == null || _AnimHandler == null)
        {
            yield break;
        }

        //como estaba lo anterior
        var correctTarget = _Target.position + Vector3.up;
        _BulletSpawnPoint.LookAt(correctTarget, _KCC.CharacterUp);

        var bullet = Instantiate(_BulletPrefab, _BulletSpawnPoint.position, _BulletSpawnPoint.rotation);
        bullet.GetComponent<BulletDumb>().SetOwner(stateMachine.gameObject, stateMachine.GetComponent<PlayerContext>());


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


        if (_AudioSource != null && flashSound != null)
        {
            _AudioSource.PlayOneShot(flashSound);
        }


        pulse = 0;
        isFlashing = false;
        _FlashingCoroutine = null;
    }
}
