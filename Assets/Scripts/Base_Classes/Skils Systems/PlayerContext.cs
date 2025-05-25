using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

/// <summary>
/// Este script servirá para pasar contexto basico del usuario, elementos que sepamos que van a ser utiles o muy pesados de andar obteniendo
/// </summary>
public class PlayerContext : Visceral_Script
{
  /// <summary>
  /// La salud del usuario
  /// </summary>
    public float Health;
    /// <summary>
    /// El GameObject general del usuario
    /// </summary>
    public GameObject PlayerGameObject;
    /// <summary>
    /// El transform del usuario
    /// </summary>
    public Transform PlayerTransform;

    /// <summary>
    /// El Kinematic Character Motor del usuario
    /// </summary>
    public KinematicCharacterMotor KCCMotor;

    public override void VS_Initialize()
    {
        //PlayerGameObject = gameObject;
        //PlayerTransform= GetComponent<Transform>();
    }

    /// <summary>
    /// La ID de la faccion que este personaje pertenece
    /// </summary>
    public FactionID faction;

    public float EnemyValueScore;
}

public enum FactionID
{
    Player,
    LimboMonster1,
    LimboMonster2,
    LimboEntity,
    LimboTrap,

}
