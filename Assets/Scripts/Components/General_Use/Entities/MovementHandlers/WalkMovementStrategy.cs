using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class WalkMovementStrategy : MonoBehaviour, IMovementStrategy, ICharacterController
{
    [SerializeField] KinematicCharacterMotor _KCC;
    [SerializeField] GameObject _Model;

    [Header("V")]
    [SerializeField] Vector3 _TargetVelocity;
    [SerializeField] Quaternion _TargetRotation;




    void IMovementStrategy.Initialize(KinematicCharacterMotor _kcc, GameObject Model)
    {
        _KCC= _kcc;
        _Model= Model;
        _KCC.CharacterController = this;
    }

    void IMovementStrategy.UpdateVelocity(Vector3 Target)
    {
       _TargetVelocity = Target;
    }
    void IMovementStrategy.UpdateRotation(Quaternion Target)
    {
        _TargetRotation = Target;
    }

    public void ApplyExternalForce(Vector3 targetDirection, float Force)
    {
        
    }

    public void ApplyExternalRotation(Quaternion RotationDirection, float Force)
    {

    }


    #region KCC
    void ICharacterController.UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        
    }

    void ICharacterController.UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
      
    }


    void ICharacterController.AfterCharacterUpdate(float deltaTime)
    {
        
    }

    void ICharacterController.BeforeCharacterUpdate(float deltaTime)
    {
        
    }

    bool ICharacterController.IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    void ICharacterController.OnDiscreteCollisionDetected(Collider hitCollider)
    {
        
    }

    void ICharacterController.OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    void ICharacterController.OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {

    }

    void ICharacterController.PostGroundingUpdate(float deltaTime)
    {
       
    }

    void ICharacterController.ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
       
    }

  
    #endregion
}
