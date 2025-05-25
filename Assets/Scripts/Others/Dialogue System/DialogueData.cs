using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "Nuevo Dialogo",menuName = "Visceral_Limbo/Systems/DialogueSystem/DialogueData")]
public class DialogueData : ScriptableObject
{

 
    [SerializeReference] public List<DialogueNode> DialogueNodes = new List<DialogueNode>();

}
