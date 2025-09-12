using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;


public class RayCastWrapper : MonoBehaviour
{
    /// <summary>
    /// El tipo de Raycast que se va a realizar
    /// </summary>
    protected enum RaycastType
    {
        Raycast,
        SphereCast
    }
    [Header("References")]

    /// <summary>
    /// El punto de origen del raycast
    /// </summary>
    [SerializeField] protected Transform RayCastOrigin;

    /// <summary>
    /// El motor del Raycaster
    /// </summary>
    protected RayCastCore Core;

    /// <summary>
    /// El objeto detectado actualmente
    /// </summary>
    [SerializeField] protected IRaycastDetectable CurrentDetected;

    [Space]
    [Header("Variables")]
    /// <summary>
    /// La distancia maxima del RayCast
    /// </summary>
    [SerializeField] protected float RayDistance;

    /// <summary>
    /// El radio maximo del SphereCast
    /// </summary>
    [SerializeField] protected float SphereRadius = 0.05f;

    /// <summary>
    /// los layer masks afectados por el RayCaster
    /// </summary>
    [SerializeField] protected LayerMask _RaycastLayers;


    /// <summary>
    /// El tipo de raycast seleccionado
    /// </summary>
    [SerializeField] protected RaycastType CastType;

    [Space]
    [Header("Debug")]
    [SerializeField] protected bool Debug_DrawGizmos;

    protected void Awake()
    {
        Core = new RayCastCore(RayDistance,_RaycastLayers,SphereRadius);
    }

    /// <summary>
    /// Funcion de realizado de RayCast
    /// </summary>
    protected virtual void PerformRayCast()
    {
        //calculo de origen y direccion del raycast
        Vector3 Origin = RayCastOrigin.position;
        Vector3 Direction = RayCastOrigin.forward;

        // la deteccion del Raycast
        IRaycastDetectable newDetected = null;

        switch (CastType)
        {
            case RaycastType.Raycast:
                newDetected = Core.CheckRayCast(Origin, Direction);
                break;
            case RaycastType.SphereCast:
                newDetected = Core.CheckSphereCast(Origin, Direction);
                break;
        }

        if (newDetected != null && newDetected != CurrentDetected)
        {
            // Se ha detectado un nuevo objeto

            if (CurrentDetected != null) // si tenemos un currentDetected
            {
                CurrentDetected.OnRayCastExit(this);
            }
            CurrentDetected = newDetected;
            CurrentDetected.OnRayCastEnter(this);
        }
        else if (newDetected == null && CurrentDetected != null)
        {
            // Se ha dejado de detectar el objeto anterior
            CurrentDetected.OnRayCastExit(this);
            CurrentDetected = null;
        }
        else if (newDetected != null && newDetected == CurrentDetected)
        {
            // Se sigue detectando el mismo objeto
            CurrentDetected.OnRayCastStay(this);
        }
    }

    protected virtual void OnDrawGizmos()
    {
        if (!Debug_DrawGizmos) return;

        switch(CastType)
        {
            case RaycastType.Raycast:
                if(CurrentDetected == null)
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    Gizmos.color = Color.green;
                }

                Gizmos.DrawRay(RayCastOrigin.position, RayCastOrigin.forward * RayDistance);
                break;

            case RaycastType.SphereCast:
                if(CurrentDetected == null)
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    Gizmos.color = Color.green;
                }
                Gizmos.DrawWireSphere(RayCastOrigin.position 
                                     + RayCastOrigin.forward * RayDistance
                                     , SphereRadius);
                break;
        }
    }

}
       
          
