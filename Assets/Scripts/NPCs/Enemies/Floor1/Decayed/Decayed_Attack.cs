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
    [SerializeField] PlayerContext _Context;

    [Header("Variables")]
    [SerializeField] float _AttackSpeed,_attack;
    [SerializeField] float _SafeSpace,_FarAway;
    Vector3 TargetDirection;

    [SerializeField] private AudioSource _AudioSource;
    [SerializeField] private AudioClip flashSound;
    bool isFlashing = false;

    [Header("Attack Sequence Variables")]
    [Tooltip("En que porcentaje de la animacion se dispara el proyectil")]
    [SerializeField] float _ShootPointTimer = 0.7f;
    [SerializeField] bool _IsAttackingSequence;
    [SerializeField] bool _HasFired;
    [SerializeField] float _AttackPulse = 0f;

    [Space]
    [Header("Stats")]
    [SerializeField] StatIdentifier AttackStatID, AttackSpeedStatID;

    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        // Bloquear transiciones si estamos en medio de la animación
        if (!_IsAttackingSequence)
        {
            return base.EvaluateTransitions(GlobalParams, out TO);
        }

        TO = null;
        return false;
    }

    public override void OnEnter(VisceralStateMachine CTX)
    {
        stateMachine.SetGlobalCondition("Moving", false);
        _AnimHandler.SetParameter("Decayed", "Idle", AnimatorControllerParameterType.Trigger);
        _MovementStrategy.KillAllMovement();
        pulse = 0;

        _IsAttackingSequence = false;
        _HasFired = false;
        _AttackPulse = 0f;

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
        _AnimHandler.TryGetAnimator("Decayed", out Animator _Anime);
        _Anim = _Anime;
        if (_Context == null) _Context = CTX.GetComponent<PlayerContext>();


        StatsManager _StatMan = CTX.GetComponent<StatsManager>();
        if(_StatMan == null)
        {
            _StatMan = CTX.GetComponentInChildren<StatsManager>();
        }

        if(_StatMan != null)
        {
            _StatMan.OnStatChanged += UpdateStats;

            _attack = _StatMan.GetFloatStatValue(AttackStatID);
            _AttackSpeed = _StatMan.GetFloatStatValue(AttackSpeedStatID);
        }
    }

    float pulse = 0;
    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        // matar movimiento del personaje restante (entre updates)
        _MovementStrategy.UpdateVelocity(Vector3.zero);

        // direccion del ataque
        TargetDirection = _Target.position - _KCC.Capsule.transform.position;
        TargetDirection.y = 0;

        _MovementStrategy.UpdateRotation(TargetDirection.normalized);

        // paso 1) espera al ataque / cooldown
        if (!_IsAttackingSequence)
        {
            if (_AttackPulse < _AttackSpeed)
            {
                _AttackPulse += Time.deltaTime * TimeDilationManager.GlobalTimeScale;
                return;
            }
            else
            {
                // iniciamos ataque
                _IsAttackingSequence = true;
                _HasFired = false;
                _AnimHandler.SetParameter("Decayed", "Attack", AnimatorControllerParameterType.Trigger);
                return;
            }
        }

        AnimatorStateInfo AnimInfo = _Anim.GetCurrentAnimatorStateInfo(0);

        // paso 2) animacion y ataque
        if (AnimInfo.IsTag("Shooting"))
        {
            // OBTENEMOS EL PORCENTAJE REAL (0.0 a 1.0)
            float currentAnimTime = AnimInfo.normalizedTime % 1f;

            // Ignorar frames del ataque anterior si acabamos de entrar
            if (!_HasFired && currentAnimTime > 0.8f) return;

            // si pasamos X% de la animacion y no disparamos => disparamos
            if (currentAnimTime >= _ShootPointTimer && !_HasFired)
            {
                _HasFired = true;
                ExecuteShoot();
            }
            //terminamos la animacion de ataque
            else if (currentAnimTime >= 0.95f && _HasFired)
            {
                _AnimHandler.SetParameter("Decayed", "Idle", AnimatorControllerParameterType.Trigger);
                ResetAttackStatus();
            }
        }
    }


    private void ExecuteShoot()
    {
        var CorrectTarget = _Target.transform.position + Vector3.up;
        _BulletSpawnPoint.LookAt(CorrectTarget, _KCC.CharacterUp);

        var Bullet = Instantiate(_BulletPrefab, _BulletSpawnPoint.transform.position, _BulletSpawnPoint.rotation);
        BulletDumb BulletScript = Bullet.GetComponent<BulletDumb>();

        BulletScript.SetDamage(_attack);
        BulletScript.SetOwner(stateMachine.gameObject, _Context);

        if(flashAttackParticle != null)
        {
            GameObject FlashAttackGO = Instantiate(flashAttackParticle, _BulletSpawnPoint.transform.position, _BulletSpawnPoint.rotation);
            ParticleSystem PartSys = FlashAttackGO.GetComponent<ParticleSystem>();

            if (PartSys != null && !PartSys.isPlaying) PartSys.Play();

            Destroy(PartSys,PartSys.main.duration);
        }

        if (_AudioSource != null && flashSound != null)
        {
            _AudioSource.PlayOneShot(flashSound);
        }
    }

    private void ResetAttackStatus()
    {
        // Reseteamos variables para el próximo disparo
        _IsAttackingSequence = false;
        _HasFired = false;
        _AttackPulse = 0;

        // Evaluamos distancia post-ataque
        float postAttackDistance = Vector3.Distance(_Target.position, _KCC.Capsule.transform.position);
        if (postAttackDistance < _SafeSpace || postAttackDistance > _FarAway)
        {
            stateMachine.SetGlobalCondition("Attack", false);
            stateMachine.SetGlobalCondition("Moving", true);
        } 
    }
    #region Deprecated:
    /*private IEnumerator FlashThenShoot()
    {
        isFlashing = true;



        // Animación y disparo de bala
        _AnimHandler?.SetParameter("Decayed", "Attack", AnimatorControllerParameterType.Trigger);

        //esperamos a que estemos oficialment en el estado de ataque
        yield return new WaitUntil(() => _anim !=  null || _anim.GetCurrentAnimatorStateInfo(0).IsName("Decayed AttackAnim"));

        // esperamos a estar en el frame de ataque
        yield return new WaitUntil(() =>_anim != null || _anim.GetCurrentAnimatorStateInfo(0).normalizedTime % 1.0f >= 0.7f);

        if (_anim == null || _AnimHandler == null)
        {
            yield break;
        }

        //como estaba lo anterior
        var correctTarget = _Target.position + Vector3.up;
        _BulletSpawnPoint.LookAt(correctTarget, _KCC.CharacterUp);

        var bullet = Instantiate(_BulletPrefab, _BulletSpawnPoint.position, _BulletSpawnPoint.rotation);
        BulletDumb BulletScript = bullet.GetComponent<BulletDumb>();
        BulletScript.SetOwner(stateMachine.gameObject, stateMachine.GetComponent<PlayerContext>());
        BulletScript.SetDamage(_attack);

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
    */
    #endregion
    private void UpdateStats(StatIdentifier StatID,float Value)
    {
        if (StatID == AttackSpeedStatID) _AttackSpeed = Value;
        else if (StatID == AttackStatID) _attack = Value;
    }
}
