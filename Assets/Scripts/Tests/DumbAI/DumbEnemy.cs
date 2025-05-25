using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DumbEnemy : MonoBehaviour
{
    /// <summary>
    /// contexto del jugador , esta variable va a contener toda la info necesaria del jugador
    /// </summary>
    public PlayerContext playerContext;

    /// <summary>
    /// settear el playercontext del jugador
    /// </summary>
    /// <param name="player"> nuevo contexto</param>
    public virtual void SetPlayerReference(PlayerContext player) 
    {
        playerContext = player;
    }

    /// <summary>
    /// como el KCC no permite modificar transform de manera directa. usar este evento en su lugar
    /// </summary>
    /// <param name="newtransform"> nueva posicion</param>
    /// <param name="newrotation">nueva rotacion</param>
    public virtual void SetTransformAndRotation(Transform newtransform, Quaternion newrotation) { }


}
