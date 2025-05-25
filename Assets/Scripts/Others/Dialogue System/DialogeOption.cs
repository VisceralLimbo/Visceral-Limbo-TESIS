using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Script usado para crear opciones de dialogo
/// </summary>


[CreateAssetMenu(fileName = "Nueva respuesta de Dialogo", menuName = "Visceral_Limbo/Systems/DialogueSystem/DialogueOption")]
public class DialogeOption : ScriptableObject
{
    /// <summary>
    /// Respuesta del jugador
    /// </summary>
    public string TextReply;

    /// <summary>
    /// Siguiente linea de dialogo, usar numero de posicion de dialogo
    /// </summary>
    public int nextDialogueID;

}
