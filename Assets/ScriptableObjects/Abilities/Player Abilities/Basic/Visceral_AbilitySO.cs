using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Este ScriptableObject sera utilizado como un prefab de cada habilidad que el jugador posee
/// aqui se necesita cargar las variables compartidas y un gameobject que sirva como prefab de la habilidad
/// </summary>
/// 
[CreateAssetMenu(menuName = "Visceral_Limbo/Combat Data/SkillData/Scriptable Objects/VisceralSkillSO")]
public class Visceral_AbilitySO : ScriptableObject
{
    /// <summary>
    /// El nombre de la habilidad
    /// </summary>
    public string SkillName;

    /// <summary>
    /// El ID de la habilidad, es separada del SkillName por conveniencia :)
    /// </summary>
    public string SkillID;

    /// <summary>
    /// La descripcion de la habilidad
    /// </summary>
    public string SkillDescription;

    /// <summary>
    /// El sprite que será utilizado en el UI
    /// </summary>
    public Sprite SkillSprite;

    /// <summary>
    /// El Cooldown de la habilidad, este cooldown solo afecta la activacion inicial de la habilidad
    /// </summary>
    public float Cooldown;

    /// <summary>
    /// Un GameObject que sirve de prefab de componente, este GameObject tiene que contener el script de habilidad.
    /// Cuando el jugador utilice una habilidad, el ability manager creara un gameobject como hijo del player con el componente de habilidad
    /// </summary>
    ///

    public GameObject behaviorComponentPrefab;

    public PlayerContext UserContext;
}
