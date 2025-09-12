using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCastCore
{
    float _Distance;
    float _Radius;
    LayerMask _Masks;

    /// <summary>
    /// Builder del RaycastCore
    /// </summary>
    /// <param name="distance">Distancia del Raycast</param>
    /// <param name="radius">Radio del Raycast, solo para SphereCast</param>
    public RayCastCore(float distance,LayerMask newMask, float radius = 0)
    {
        _Distance = distance;
        _Radius = radius;
        _Masks = newMask;
    }

    /// <summary>
    /// Funcion de Raycast, devuelve una Interfaz de tipo IRaycastDetectable
    /// </summary>
    /// <param name="From"> origen del raycast</param>
    /// <param name="Direction"> direccion del raycast</param>
    /// <returns></returns>
    public IRaycastDetectable CheckRayCast(Vector3 From, Vector3 Direction)
    {
        //chequeo de RayCast
        if (Physics.Raycast(From,Direction.normalized,out RaycastHit Hit, _Distance,_Masks,QueryTriggerInteraction.Collide))
        {
            //devolver la Interfaz de RaycastDetectable
            if (Hit.collider.TryGetComponent(out IRaycastDetectable Raycasted)) return Raycasted;
        }
        return null;

    }

    /// <summary>
    /// Funcion de SphereCast, devuelve una Interfaz de tipo IRaycastDetectable
    /// </summary>
    /// <param name="From">origen del raycast </param>
    /// <param name="Direction"> direccion del raycast</param>
    /// <returns></returns>
    public IRaycastDetectable CheckSphereCast(Vector3 From,Vector3 Direction)   
    {
        //chequeo de SphereCast
        if(Physics.SphereCast(From,_Radius,Direction.normalized,out RaycastHit Hit, _Distance,_Masks, QueryTriggerInteraction.Collide))
        {
            //devolver la Interfaz de RaycastDetectable
            if (Hit.collider.TryGetComponent(out IRaycastDetectable RayCasted)) return RayCasted;

        }

        return null;

    }




}
    
