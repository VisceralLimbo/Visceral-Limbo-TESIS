using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Nuevo nodo de Dialogo", menuName = "Visceral_Limbo/Systems/DialogueSystem/DialogueNode")]
public class DialogueNode :ScriptableObject
{
    /// <summary>
    /// Data del dialogo
    /// </summary>
    [TextArea(10, 3)]
    [SerializeField] public string TextData;
    /// <summary>
    /// Nombre del personaje que habla
    /// </summary>
    [SerializeField] public string SpeakerName;
    /// <summary>
    /// Sprite usado para representar al personaje
    /// </summary>
    [SerializeField] public Sprite SpeakerSprite;
    /// <summary>
    /// El ID del siguiente dialogo
    /// </summary>
    /// 
    [SerializeField] public int NextDialogeOption = -1; // dejar en  -1 si es el fin de dialogo

    [SerializeField] public bool AutoAdvance;
    [SerializeField] public float TimeToAdvance;
    [SerializeField] public float TimeBetweenChars;

    /// <summary>
    /// Lista de opciones posibles, dejar nulo si lineal
    /// </summary>
    [SerializeField] public List<DialogeOption> Options= new List<DialogeOption>(); 


}
