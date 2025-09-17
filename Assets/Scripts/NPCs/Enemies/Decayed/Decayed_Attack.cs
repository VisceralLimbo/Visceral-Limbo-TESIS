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
    //la mesh del enemigo
    [SerializeField] SkinnedMeshRenderer _EnemyMesh;
    //el mat del flash (se setea en la corutina)
    [SerializeField] Material FlashEnemyMat;

    [Header("Variables")]
    [SerializeField] float _AttackSpeed;
    [SerializeField] float _SafeSpace,_FarAway;
    Vector3 TargetDirection;
    [SerializeField] bool _TargetIsTooClose,_TargetIsTooFar;
    //duracion del flash
    [SerializeField] float flashDuration = 1f;

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

        //la llamo antes de disparar para que haga el flash (el cooldown es poco asi que no se como quieren trabajar eso)
        TriggerFlash();

        //realizar ataque
        _AnimHandler.SetParameter("Decayed", "Attack", AnimatorControllerParameterType.Trigger);

        //obtener el animador que usamos
        if(_AnimHandler.TryGetAnimator("Decayed",out Animator Anim))
        {
            // chequeo si termino la animacion actual
            if(Anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f)
            {
                pulse = 0;
                var correctTarget = _Target.transform.position + Vector3.up;
                //apuntado al player
                _BulletSpawnPoint.LookAt(correctTarget, _KCC.CharacterUp);


                // instanciado de bala
                var bullet =Instantiate(_BulletPrefab, _BulletSpawnPoint.position, _BulletSpawnPoint.rotation);
                bullet.GetComponent<BulletDumb>().SetOwner(CTX.gameObject, CTX.GetComponent<PlayerContext>());
           
            }
        }
    
    }

    public void TriggerFlash()
    {
        //funcion para llamar a la courtina
        StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        // agarro la lista de materiales q tiene el enemigo
        Material[] currentMats = _EnemyMesh.materials;

        // si tiene 1 agrego slot para otro mat el flash
        if (currentMats.Length == 1)
        {
            System.Array.Resize(ref currentMats, 2);
        }

        // el original va en el 0
        Material baseMat = currentMats[0];

        // el flash 1 (seria el segundo)
        currentMats[1] = FlashEnemyMat;
        _EnemyMesh.materials = currentMats;

        // espero la duracion del flash
        yield return new WaitForSeconds(flashDuration);

        // vuelvo a tener el material base
        Material[] restoreMats = new Material[1];
        restoreMats[0] = baseMat;
        _EnemyMesh.materials = restoreMats;
    }
}
