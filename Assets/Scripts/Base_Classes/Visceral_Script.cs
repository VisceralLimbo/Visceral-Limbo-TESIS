using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visceral_Script : MonoBehaviour
{
    //patricio malvasio
    // 6/4/2025 21:57
    // hice este Script como componente base de TODOS los scripts del juego
    //la idea detras es poder unificar un poco los scripts y tener eventos universales


    //initialize, usado como start pero tenemos control más exacto de cuando

    /// <summary>
    /// esta funcion sirve para iniciar el script, similar a start. pero tiene que ser llamado de manera externa
    /// </summary>
    public virtual void VS_Initialize() { }

    //Initialize with parameters, similar pero podemos pasar parametros con params
    //despues es importante castearlos explicitamente al type necesario

    /// <summary>
    /// esta funcion sirve para iniciar el script, similar a start. pero tiene que ser llamado de manera externa   
    /// permite el uso de parametros
    /// </summary>
    /// <param name="a"> parametros usados a la hora de iniciar </param>
    /// 
    public virtual void VS_InitializeWithParameters(params object[] a) { }

    /// <summary>
    /// esta funcion sirve como nuestro update, pero necesita de ser llamado por otro script
    /// </summary>
    public virtual void VS_RunLogic()
    {

    }
    /// <summary>
    /// esta funcion sirve como nuestro update, pero necesita de ser llamado por otro script
    /// tambien puede recibir parametros
    /// </summary>
    /// <param name="a"> array de parametros, utilizar type casting</param>
    public virtual void VS_Runlogic(params object[] a) { }
}
