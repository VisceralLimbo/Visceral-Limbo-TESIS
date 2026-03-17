using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class Obstacle_Avoidance_Component : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerContext context;
    [SerializeField] KinematicCharacterMotor _KCC;
    [SerializeField] LayerMask _Layermasks;


    [Header("Variables for Simple system")]
    [SerializeField] int numberOfRays;
    [SerializeField] float AngleOfRays;
    [SerializeField] float RaycastLenght;
    [SerializeField] float _ProximityStrenght;

    [Header("Variables for Custom System, incompatible with simple")]
    [SerializeField] RayCastCore[] CustomRaycastFunctions;

    private bool IsUsingComplexSystem = false;

    private void Start()
    {
        if (context == null) context.GetComponentInParent<PlayerContext>();
        if (context != null & _KCC == null) _KCC = context.KCCMotor;

        if(CustomRaycastFunctions != null)
        {
            if(CustomRaycastFunctions.Length > 0)
            {
                IsUsingComplexSystem = true;
            }
        }

    }

    public Vector3 PerformObstacleAvoidance(Vector3 TargetDirection, float OriginalSpeed) 
    {
        if (!IsUsingComplexSystem)
        {
            return SimpleObstacleAvoidance(TargetDirection, OriginalSpeed);
        }
        else
        {
            return Vector3.forward;
        }
    }


    private Vector3 SimpleObstacleAvoidance(Vector3 TargetDirection, float OriginalSpeed)
    {

        // vamos a cachear mis dos variables principales
        Vector3 finalDirection = TargetDirection;
        Vector3 AvoidanceForce = Vector3.zero;

        // por cada raycast vamos a realizar un calculo de angulos
        for(int i = 0; i < numberOfRays; i++)
        {
            // Por cuanto tengo que girar mi raycast en funcion de la cantidad
            // basicamente el step
            float AngleFraction = (float) i / (numberOfRays - 1);

            // cuanta es la rotacion del raycast
            var RotationMod = Quaternion.AngleAxis(AngleFraction * AngleOfRays * 2 - AngleOfRays,_KCC.CharacterUp);

            // cual es la direccion del raycast
            var Direction = RotationMod * _KCC.CharacterForward;

            // calculo de fisica del raycast
            RaycastHit HitInfo;
            var RayCast = new Ray(_KCC.Capsule.transform.position, Direction);

            if (Physics.Raycast(RayCast, out HitInfo, RaycastLenght,_Layermasks))
            {
                // vamos a calcular cuanta fuerza tiene el raycast en funcion de la distancia al obstaculo
                // mientras más cerca el origen del raycast al obstaculo, mas fuerza
                // de esta manera podemos calcular el desplazamiento final, basandonos en el valor de los raycasts
                float ProximityMultipler = 1.0f - (HitInfo.distance / RaycastLenght);
                AvoidanceForce += HitInfo.normal * ProximityMultipler * _ProximityStrenght;
            }
        }

        //a la direccion original le vamos a sumar el desplazamiento de obstacle avoidance

        finalDirection = (finalDirection + AvoidanceForce).normalized * OriginalSpeed;

        return finalDirection;
    }



    private void OnDrawGizmosSelected()
    {
        for(int i = 0; i < numberOfRays; i++)
        {
            // Por cuanto tengo que girar mi raycast en funcion de la cantidad
            // basicamente el step
            float AngleFraction = (float) i / (numberOfRays - 1);

            // cuanta es la rotacion del raycast
            var RotationMod = Quaternion.AngleAxis(AngleFraction * AngleOfRays * 2 - AngleOfRays, this.transform.up);

            // cual es la direccion del raycast
            var Direction = RotationMod * this.transform.forward;

            var RayCast = new Ray(this.transform.position, Direction);

            Gizmos.DrawRay(RayCast);
        }
    }
}
