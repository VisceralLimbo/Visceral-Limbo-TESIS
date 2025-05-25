using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class DumbRanger : DumbEnemy, ICharacterController
{
    [SerializeField] KinematicCharacterMotor _KKC;
    [SerializeField] KinematicCharacterMotorState _State;
    [SerializeField] Rigidbody _RB;
    public PlayerContext Context;
    [SerializeField] Material _mat;
    [SerializeField] Transform target,SpawnPoint;
    [SerializeField] GameObject _BulletPrefab;
    [SerializeField] float speed, movementspeed,AttackSpeed, MovementAcceleration, distanceToTarget;
    [SerializeField] float distanceToAttack,SafeSpace;
    [SerializeField] float rotationspeed;
    [SerializeField] bool SuccessfullAttack;

    Vector3 RequestedAdditiveVelocity;
    Vector3 RequestedForceVelocity;

    private Vector3 targetdirection;


    private void Start()
    {
        _KKC = GetComponentInChildren<KinematicCharacterMotor>();
        _KKC.CharacterController = this;

        if (_RB == null) _RB.GetComponentInChildren<Rigidbody>();

        _KKC.AttachedRigidbodyOverride = _RB;
        Context = GetComponent<PlayerContext>();
        target = PlayerTransform();
        if(Context == null)
        {
            Context = this.gameObject.AddComponent<PlayerContext>();
        }
    }


    private void Update()
    {
        if (target == null)
        {
            target = PlayerTransform();
            if(target == null) { Debug.LogError("Player is null"); return; }
        }
        if (_KKC.AttachedRigidbody == null) _KKC.AttachedRigidbodyOverride = _RB;

        targetdirection = target.transform.position - _KKC.Capsule.transform.position;
        distanceToTarget = Vector3.Distance(_KKC.Capsule.transform.position, target.transform.position);

        SpawnPoint.transform.position = _KKC.Capsule.transform.position + _KKC.Capsule.transform.forward/2;

        if (distanceToTarget < distanceToAttack && distanceToTarget > SafeSpace)
        {
            if(SuccessfullAttack == false)
            {
                SuccessfullAttack = true;
                StartCoroutine(FireSequence());
            }   
        }
        else
        {
            StopAllCoroutines();
            SuccessfullAttack = false;
        }
    }

    IEnumerator FireSequence()
    {
        while(SuccessfullAttack)
        {
            yield return new WaitForSeconds(AttackSpeed);

            var bullet = Instantiate(_BulletPrefab);
            bullet.GetComponent<BulletDumb>().SetOwner(Context.PlayerGameObject,Context);
            bullet.transform.position = SpawnPoint.transform.position;
            bullet.transform.forward = _KKC.CharacterForward;
            yield return null;
        }
      
    }

    private void RequestExtraVelocity(Vector3 ExtraVelocity)
    {
        _mat.color = Color.red;
        
        RequestedAdditiveVelocity += ExtraVelocity;
    }

    private void RequestForceVelocity(Vector3 ForceVelocity)
    {
        _mat.color = Color.red;
        
        RequestedForceVelocity = ForceVelocity;
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (SuccessfullAttack)
        {
            _KKC.AttachedRigidbody.velocity = Vector3.zero;
            currentVelocity = Vector3.zero;
            return;
        }

        //player muy lejos
        if (_KKC.GroundingStatus.IsStableOnGround && distanceToTarget > distanceToAttack + (int)Random.Range(-2,2) && distanceToTarget > SafeSpace)
        {


            var groundedMovement = _KKC.GetDirectionTangentToSurface(
                direction: targetdirection,
                surfaceNormal: _KKC.GroundingStatus.GroundNormal

                );


            var TargetVelocity = speed * groundedMovement;
            currentVelocity = Vector3.Slerp
                (
                    a: currentVelocity,
                    b: TargetVelocity,
                    t: 1f - Mathf.Exp(-MovementAcceleration * deltaTime)

                );

        }
        //player to close to comfort
        else if(_KKC.GroundingStatus.IsStableOnGround && distanceToTarget < SafeSpace)
        {

            var groundedMovement = _KKC.GetDirectionTangentToSurface(
                direction: -targetdirection,
                surfaceNormal: _KKC.GroundingStatus.GroundNormal

                ) * 1.2f;


            var TargetVelocity = speed * groundedMovement;
            currentVelocity = Vector3.Slerp
                (
                    a: currentVelocity,
                    b: TargetVelocity,
                    t: 1f - Mathf.Exp(-MovementAcceleration * deltaTime)

                );
        }

        else
        {
            currentVelocity = new Vector3(0, -10, 0);
        }


        if (RequestedAdditiveVelocity.sqrMagnitude > 0)
        {

            currentVelocity += RequestedAdditiveVelocity;
            RequestedAdditiveVelocity = Vector3.zero;
        }

        if (RequestedForceVelocity.sqrMagnitude > 0)
        {
            _KKC.AttachedRigidbody.AddForce(RequestedForceVelocity, ForceMode.Impulse);
            RequestedForceVelocity = Vector3.zero;
        }

        if (_KKC.AttachedRigidbody.velocity.sqrMagnitude > 0)
        {
            _KKC.AttachedRigidbody.velocity = Vector3.Slerp(_KKC.AttachedRigidbodyVelocity, Vector3.zero, 0.1f + Time.deltaTime);
        }
    }


    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        var forward = Vector3.ProjectOnPlane(
                vector: targetdirection,
                _KKC.CharacterUp


            );

        currentRotation = Quaternion.LookRotation(forward, _KKC.CharacterUp);
    }

    private Transform PlayerTransform()
    {
        return playerContext.PlayerTransform;
    }


    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
 
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {

    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {

    }

    public void PostGroundingUpdate(float deltaTime)
    {

    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {

    }

    public void BeforeCharacterUpdate(float deltaTime)
    {  
        _State = _KKC.GetState();
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        Context.KCCMotor = _KKC;
        Context.PlayerGameObject = this.gameObject;
        Context.PlayerTransform = _KKC.Capsule.transform;
    }

    public override void SetTransformAndRotation(Transform newtransform, Quaternion newrotation)
    {
        _KKC.SetPositionAndRotation(newtransform.position, newrotation);
    }
}
