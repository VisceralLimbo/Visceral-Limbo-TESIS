using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interfaz general de RayCastDetectable, usado para cualquier script que tenga
/// que ser detectado por El sistema de RayCast Custom (RaycastWrapper)
/// </summary>
public interface IRaycastDetectable
{
    /// <summary>
    /// Funcion de entrada de Raycast, llamado 1 vez al ser detectado
    /// </summary>
    /// <param name="Detector"> el RayCastWrapper que detecto a este script</param>
    public abstract void OnRayCastEnter(RayCastWrapper Detector = null);

    /// <summary>
    /// Funcion de update de Raycast, llamado cada frame mientras este siendo detectado
    /// </summary>
    /// <param name="Detector"></param>
    public abstract void OnRayCastStay(RayCastWrapper Detector = null);

    /// <summary>
    /// Funcion de salida de Raycast, llamado 1 vez al dejar de ser detectado
    /// </summary>
    /// <param name="Detector"></param>
    public abstract void OnRayCastExit(RayCastWrapper Detector = null);


}

/// <summary>
/// Interfaz de interactuable por Raycast, hereda de IRaycastDetectable
/// </summary>
public interface IRaycastInteractable : IRaycastDetectable
{
    /// <summary>
    /// Funcion especifica de Interaccion.
    /// </summary>
    public abstract void OnInteract();
}
