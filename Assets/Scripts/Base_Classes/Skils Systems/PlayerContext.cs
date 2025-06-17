using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

/// <summary>
/// Este script servirá para pasar contexto basico del usuario, elementos que sepamos que van a ser utiles o muy pesados de andar obteniendo
/// </summary>
public class PlayerContext : Visceral_Script
{

    public StatsManager Stats;
    public InventoryManager Inventory;

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

    public IKnockback knockback;

    public override void VS_Initialize()
    {
        //PlayerGameObject = gameObject;
        //PlayerTransform= GetComponent<Transform>();

        knockback = this.transform.root.GetComponentInChildren<IKnockback>();
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
