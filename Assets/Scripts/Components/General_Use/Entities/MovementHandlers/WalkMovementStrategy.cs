using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class WalkMovementStrategy : MonoBehaviour, IMovementStrategy, ICharacterController, IKnockback
{
    [Header("References")]
    [SerializeField] KinematicCharacterMotor _KCC;
    [SerializeField] GameObject _Model;
    [SerializeField] Rigidbody _RB;
    [SerializeField] StatsManager _StatMan;
    [Space]

    [Header("Variables")]
    [SerializeField] Vector3 _TargetVelocity;
    [SerializeField] Vector3? _TargetRotation;
    [SerializeField] bool _IsActive, _KillAllMovement;
    [SerializeField] float _MovementSpeed;
    [SerializeField] float _BaseMovementSpeed;
    [SerializeField] float _MovementAccel;
    [SerializeField] float _MaxRotationSpeed;
    [Space]

    [Header("Knockback Config")]
    [SerializeField] bool GotKnockbacked;
    [SerializeField] float _KnockbackDecay;
    [SerializeField] Vector3 _KnockbackVelocity;
    [SerializeField] float _KnockbackResistance;
    [Space]

    [Header("Stats Setup")]
    [SerializeField] string _MovementStat;
    [SerializeField] string _KnockbackResistanceStat;

    Vector3 _AddExternalVelocity;


    public void Initialize(KinematicCharacterMotor _kcc, GameObject Model)
    {

        _KCC = _kcc;
        _Model = Model;
        _KCC.CharacterController = this;
        if (_RB == null)
        {
            if (Model.TryGetComponent<Rigidbody>(out Rigidbody Comp))
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

        if (Model.TryGetComponent(out Health_Component HP))
        {

            HP.OnDeath += OnDeath;
            HP.OnKnockbackTaken += ApplyKnockBack;
            print("death subscription");
        }
        else
        {
            if (this.TryGetComponent(out Health_Component _HP))
            {
                _HP.OnDeath += OnDeath;
                _HP.OnKnockbackTaken += ApplyKnockBack;
                print("death subscription");
            }
        }
        if(_StatMan == null)
        {
            StatsManager _stat = this.GetComponentInParent<StatsManager>();
            if(_stat != null)
            {
                _StatMan = _stat;

                _StatMan.OnStatChanged += UpdateStatValues;

                _MovementSpeed = _StatMan.GetFloatStatValue(_MovementStat);
                _KnockbackResistance = _StatMan.GetFloatStatValue(_KnockbackResistanceStat);
            }
        }


        _BaseMovementSpeed = _MovementSpeed;
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

    public void SetMovementSpeed(float Speed)
    {
        _MovementSpeed = Speed;
    }

    public void ResetMovementSpeed()
    {
        _MovementSpeed = _BaseMovementSpeed;
    }

    private void UpdateStatValues(string StatID, float Value)
    {
        if(StatID == _MovementStat)
        {
            _MovementSpeed = Value;
        }
        else if ( StatID == _KnockbackResistanceStat)
        {
            _KnockbackResistance = Value;
        }
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
        if (_KnockbackVelocity.sqrMagnitude > 0.1f)
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
        if (_AddExternalVelocity.sqrMagnitude > 0.1f)
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
        // CASO 1: Tenemos una rotación forzada (El Target)
        if (_TargetRotation.HasValue)
        {
            // 1. Tomamos el valor directamente (Asumimos que el State ya nos manda la DIRECCIÓN, no la posición)
            Vector3 lookDirection = _TargetRotation.Value;

            // 2. Aplanamos el vector para que no mire hacia arriba/abajo (evita inclinaciones raras)
            lookDirection.y = 0;

            // 3. Normalizamos para asegurar que la magnitud sea 1 (vital para evitar jitter en math)
            lookDirection.Normalize();

            // 4. Si la dirección es válida, rotamos
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                var targetRotation = Quaternion.LookRotation(lookDirection, _KCC.CharacterUp);

                currentRotation = Quaternion.RotateTowards(
                    from: currentRotation,
                    to: targetRotation,
                    maxDegreesDelta: _MaxRotationSpeed * (TimeDilationManager.GlobalTimeScale * deltaTime)
                );
            }
            return;
        }

        //
        //CASO 2: sentido adelante
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
                 from: currentRotation
                , to: calculatedRotation
                , maxDegreesDelta: _MaxRotationSpeed * (TimeDilationManager.GlobalTimeScale * deltaTime));
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
        if (coll.gameObject.layer == 10)
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

    public void ForceUngroundSelf(float time)
    {
        _KCC.ForceUnground(time);

    }

    public KinematicCharacterMotor GetKCC()
    {
        return _KCC;
    }





    #endregion


    private void OnDrawGizmos()
    {
        if (_TargetRotation.HasValue && _KCC != null)
        {
            Gizmos.color = Color.red;
            // Dibuja una linea desde el personaje hacia donde la IA le dice que mire
            Gizmos.DrawRay(_KCC.transform.position, _TargetRotation.Value * 5f);
        }
    }
}
