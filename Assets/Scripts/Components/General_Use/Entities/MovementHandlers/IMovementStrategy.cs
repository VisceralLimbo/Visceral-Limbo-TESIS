using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public interface IMovementStrategy
{
    public void Initialize(KinematicCharacterMotor _kcc, GameObject Model);

    public void UpdateVelocity(Vector3 Target);
    public void UpdateRotation(Quaternion Target);

    public void ApplyExternalForce(Vector3 targetDirection,float Force);
    public void ApplyExternalRotation(Quaternion RotationDirection, float Force);

}
