using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class WalkMovementStrategy : MonoBehaviour, IMovementStrategy, ICharacterController,IKnockback
{
    [Header("References")]
    [SerializeField] KinematicCharacterMotor _KCC;
    [SerializeField] GameObject _Model;
    [SerializeField] Rigidbody _RB;

    [Header("Variables")]
    [SerializeField] Vector3 _TargetVelocity;
    [SerializeField] Vector3? _TargetRotation;
    [SerializeField] bool _IsActive,_KillAllMovement;
    [SerializeField] float _MovementSpeed;
    [SerializeField] float _MovementAccel;
    [SerializeField] float _MaxRotationSpeed;

    [Header("Knockback Config")]
    [SerializeField] bool GotKnockbacked;
    [SerializeField] float _KnockbackDecay;
    [SerializeField] Vector3 _KnockbackVelocity;
    [SerializeField] float _KnockbackResistance;

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

        if(Model.TryGetComponent(out Health_Component HP))
        {

            HP.OnDeath += OnDeath;
            HP.OnKnockbackTaken += ApplyKnockBack;
            print("death subscription");
        }
        else
        {
           if(this.TryGetComponent(out Health_Component _HP))
            {
                _HP.OnDeath += OnDeath;
                _HP.OnKnockbackTaken += ApplyKnockBack;
                print("death subscription");
            }
        }
    }

    private void OnDeath()
    {
        print("walk death");
        _KCC.CharacterController = null;
        _KCC.Capsule.enabled = false;
        _KCC.enabled = false;  
        this.enabled = false;
    }

    public void UpdateVelocity(Vector3 Target)
    {
       _TargetVelocity = Target;
    }
    public void UpdateRotation(Vector3 Target)
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

        // bail!
        if (!_IsActive)
        {
            currentVelocity = Vector3.zero;
            return;
        }

        if (_KillAllMovement)
        {
            currentVelocity = Vector3.zero; // no movement
            return;
        }

        //Si tenemos un knockback aplicado, lo aplicamos y cancelamos movimiento
        if(_KnockbackVelocity.sqrMagnitude > 0.1f)
        {
            currentVelocity = _KnockbackVelocity;

            if (!_KCC.GroundingStatus.IsStableOnGround)
            {
                currentVelocity += currentVelocity * (deltaTime * TimeDilationManager.GlobalTimeScale);
            }

            _KnockbackVelocity = Vector3.MoveTowards
                (
                    current: _KnockbackVelocity,
                    target: Vector3.zero,
                    maxDistanceDelta: _KnockbackDecay * (deltaTime * TimeDilationManager.GlobalTimeScale)

                );
        }
        // we are on stable ground
        else if (_KCC.GroundingStatus.IsStableOnGround)
        {
            

            var groundedMovement = _KCC.GetDirectionTangentToSurface(
                direction: _TargetVelocity,
                surfaceNormal: _KCC.GroundingStatus.GroundNormal

                );


            var TargetVelocity = _MovementSpeed * groundedMovement;
            var ScaledVelocity = TargetVelocity * TimeDilationManager.GlobalTimeScale;

            currentVelocity = Vector3.Slerp
                 (
                     a: currentVelocity,
                     b: ScaledVelocity,
                     t: 1f - Mathf.Exp(-_MovementAccel * (TimeDilationManager.GlobalTimeScale * deltaTime))

                 );
        }
        else
        {
            // we are falling
            currentVelocity += Physics.gravity * (TimeDilationManager.GlobalTimeScale * deltaTime);
        }




        // we applied external velocity
        if(_AddExternalVelocity.sqrMagnitude > 0.1f)
        {
            if (GotKnockbacked)
            {
                currentVelocity = Vector3.zero;
                GotKnockbacked = false;
            }

            currentVelocity += _AddExternalVelocity;
        }

    }



    void ICharacterController.UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (_TargetRotation.HasValue)
        {
            var Calculatedforward = Vector3.ProjectOnPlane(
                   vector: _TargetRotation.Value,
                   _KCC.CharacterUp


               );

            //limite minimo de rotacion abs.
          if(Calculatedforward.sqrMagnitude > 0.001f)
          {

                var rotation = Quaternion.LookRotation(Calculatedforward, _KCC.CharacterUp);
                currentRotation = Quaternion.RotateTowards
                    (
                        from:currentRotation,
                        to:rotation,
                        maxDegreesDelta:_MaxRotationSpeed * (TimeDilationManager.GlobalTimeScale * deltaTime)
                    );
          }
            return;
        }
            //sentido adelante
            var forward = Vector3.ProjectOnPlane(
                    vector: _TargetVelocity,
                    _KCC.CharacterUp


                );

        // si la diferencia de rotacion es muy menor
        if (forward.sqrMagnitude > 0.001f)
        {
            var calculatedRotation = Quaternion.LookRotation(forward, _KCC.CharacterUp);
            // También es buena idea suavizar esta rotación
            currentRotation = Quaternion.RotateTowards(
                 from:currentRotation
                , to: calculatedRotation
                , maxDegreesDelta: _MaxRotationSpeed *(TimeDilationManager.GlobalTimeScale * deltaTime));
        }

    }


    void ICharacterController.AfterCharacterUpdate(float deltaTime)
    {
        _AddExternalVelocity = Vector3.zero;
        _KillAllMovement = false;
        _TargetRotation = null;

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

    public void ApplyKnockBack(Vector3 KnockbackDir, float Force)
    {
        float FinalFloat = Force - _KnockbackResistance;
        if (FinalFloat <= 0f) FinalFloat = 0f;
        _KnockbackVelocity = KnockbackDir * FinalFloat;
        GotKnockbacked = true;
    }



    #endregion
}
