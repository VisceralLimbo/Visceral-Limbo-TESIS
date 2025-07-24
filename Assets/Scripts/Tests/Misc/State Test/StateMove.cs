using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class StateMove : BaseState, ICharacterController
{
    [Header("References")]
    [SerializeField] KinematicCharacterMotor _KCC;
    [SerializeField] Transform _Target;
    [SerializeField] IMovementStrategy _MovementStrategy;

    [Space]

    [Header("variables")]
    [SerializeField] float _Speed;
    [SerializeField] float _MovementAccel;
    [SerializeField] float _MaxRotationSpeed;
    [SerializeField] float _MaxDistance;

    Vector3 targetdirection;
    public override void OnInitialize(VisceralStateMachine CTX)
    {
        base.OnInitialize(CTX);
        if(_KCC == null )
        {
            _KCC = CTX.GetComponent<KinematicCharacterMotor>();
            if(_KCC == null)
            {
                _KCC = CTX.GetComponentInChildren<KinematicCharacterMotor>();
            }
            _Target = FindObjectOfType<Player_Movement>().transform;
            _KCC.CharacterController = this;
        }
    }

    public override void OnEnter(VisceralStateMachine CTX)
    {
        this.enabled= true;

    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        this.enabled = false;
        _KCC.CharacterController = null;
    }


    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        
    }


    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        //sentido adelante
        var forward = Vector3.ProjectOnPlane(
                vector: targetdirection,
                _KCC.CharacterUp


            );
        if (forward.sqrMagnitude > 0.01f)  //evitar que rote por milesimas
        {
            //rotacion deseada
            var TargetRotation = Quaternion.LookRotation(forward, _KCC.CharacterUp);

            currentRotation = Quaternion.RotateTowards(
                currentRotation,
                TargetRotation,
                _MaxRotationSpeed * Time.deltaTime
                );
        }

    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (_KCC.GroundingStatus.IsStableOnGround)
        {
            targetdirection = _Target.position - _KCC.Capsule.transform.position;
            print(_KCC.GroundingStatus.GroundCollider);


            var groundedMovement = _KCC.GetDirectionTangentToSurface(
                direction: targetdirection,
                surfaceNormal: _KCC.GroundingStatus.GroundNormal

                );


            var TargetVelocity = _Speed * groundedMovement;
            currentVelocity = Vector3.Slerp
                 (
                     a: currentVelocity,
                     b: TargetVelocity,
                     t: 1f - Mathf.Exp(-_MovementAccel * deltaTime)

                 );
        }
        else
        {
            currentVelocity = new Vector3(Physics.gravity.x, Physics.gravity.y, Physics.gravity.z);
            print("im free falling");
        }

    }



    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        return base.EvaluateTransitions(GlobalParams, out TO);
    }



    public void AfterCharacterUpdate(float deltaTime)
    {
     
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {

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
        print("groundcheck!");
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

}
