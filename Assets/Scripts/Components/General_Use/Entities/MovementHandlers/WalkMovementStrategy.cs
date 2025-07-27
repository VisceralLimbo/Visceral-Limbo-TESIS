using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class WalkMovementStrategy : MonoBehaviour, IMovementStrategy, ICharacterController
{
    [Header("References")]
    [SerializeField] KinematicCharacterMotor _KCC;
    [SerializeField] GameObject _Model;
    [SerializeField] Rigidbody _RB;

    [Header("Variables")]
    [SerializeField] Vector3 _TargetVelocity;
    [SerializeField] Quaternion _TargetRotation;
    [SerializeField] bool _IsActive,_KillAllMovement;
    [SerializeField] float _MovementSpeed;
    [SerializeField] float _MovementAccel;
    [SerializeField] float _MaxRotationSpeed;

    Vector3 _AddExternalVelocity;


    public void Initialize(KinematicCharacterMotor _kcc, GameObject Model)
    {
        _KCC= _kcc;
        _Model= Model;
        _KCC.CharacterController = this;
        if(_RB == null)
        {
            if(Model.TryGetComponent<Rigidbody>(out Rigidbody Comp))
            {
                _RB = Comp;
                _KCC.AttachedRigidbodyOverride = _RB;
            }
            else
            {
                Comp = Model.GetComponentInChildren<Rigidbody>();
                if (Comp)
                {
                    _RB = Comp;
                    _KCC.AttachedRigidbodyOverride = _RB;
                }
            }
        }
    }

    public void UpdateVelocity(Vector3 Target)
    {
       _TargetVelocity = Target;
    }
    public void UpdateRotation(Quaternion Target)
    {
        _TargetRotation = Target;
    }
    public void KillAllMovement()
    {
        _KillAllMovement = true;
    }

    public void SetActiveState(bool setActive)
    {
       _IsActive = setActive;
    }


    public void ApplyExternalForce(Vector3 targetDirection, float Force)
    {
        _AddExternalVelocity = targetDirection * Force;
    }

    public void ApplyExternalRotation(Quaternion RotationDirection, float Force)
    {

    }


    #region KCC
    void ICharacterController.UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {

        if (!_IsActive)
        {
            if (_KillAllMovement)
            {
                _KillAllMovement = false;
                currentVelocity = Vector3.zero;
                _KCC.BaseVelocity = Vector3.zero;
                currentVelocity -= currentVelocity;
            }

            return;
        }

        if (_KCC.GroundingStatus.IsStableOnGround)
        {
            

            var groundedMovement = _KCC.GetDirectionTangentToSurface(
                direction: _TargetVelocity,
                surfaceNormal: _KCC.GroundingStatus.GroundNormal

                );


            var TargetVelocity = _MovementSpeed * groundedMovement;
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

        if (_KillAllMovement)
        {
            _KillAllMovement = false;
            currentVelocity = Vector3.zero;
            _KCC.BaseVelocity = Vector3.zero;
            currentVelocity -= currentVelocity;
        }

        if(_AddExternalVelocity.sqrMagnitude > 0.1f)
        {
            currentVelocity += _AddExternalVelocity;
        }

    }



    void ICharacterController.UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        //sentido adelante
        var forward = Vector3.ProjectOnPlane(
                vector: _TargetVelocity,
                _KCC.CharacterUp


            );
        /*if (forward.sqrMagnitude > 0.01f)  //evitar que rote por milesimas
        {
            //rotacion deseada
            var TargetRotation = Quaternion.LookRotation(forward, _KCC.CharacterUp);

            currentRotation = Quaternion.RotateTowards(
                currentRotation,
                TargetRotation,
                _MaxRotationSpeed * Time.deltaTime
                );
        }*/

        var TargetRotation = Quaternion.LookRotation(forward,_KCC.CharacterUp);

        currentRotation = TargetRotation;
    }


    void ICharacterController.AfterCharacterUpdate(float deltaTime)
    {
        _AddExternalVelocity = Vector3.zero;

    }

    void ICharacterController.BeforeCharacterUpdate(float deltaTime)
    {
        
    }

    bool ICharacterController.IsColliderValidForCollisions(Collider coll)
    {
        if(coll.gameObject.layer == 10)
        {
            return false;
        }

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
