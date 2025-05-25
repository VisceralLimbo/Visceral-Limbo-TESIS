using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Trigger_Component : MonoBehaviour
{

    [Tooltip("Lista de eventos que queremos activar")]
    /// <summary>
    /// Lista de eventos de trigger
    /// </summary>
    [SerializeField] List<TriggerData> TriggerEventList = new List<TriggerData>();

    [Tooltip("Si queremos que los eventos de este trigger component puedan reactivarse")]
    /// <summary>
    /// si este trigger puede reactivarse
    /// </summary>
    [SerializeField] bool _Reusable;


    private void OnTriggerEnter(Collider other)
    {
       foreach (var triggerDT in TriggerEventList) 
       {

            print(other.tag + " trigger:" + triggerDT.TargetTag);
            if (other.CompareTag(triggerDT.TargetTag))
            {
                // si el evento esta triggered y no es reusable 
                if (triggerDT._IsTriggered && !_Reusable)
                {
                    return;
                }
                print(triggerDT.TargetTag + " passed trigger");

                triggerDT.TriggerEvent.Invoke();
                triggerDT._IsTriggered = true;
            }
            
       }


    }





}



[Tooltip("la data que compone el trigger que deseamos utilizar")]
/// <summary>
/// La data que compone un evento
/// </summary>
[System.Serializable]
public class TriggerData
{
    [Tooltip("El Tag que vamos a comparar para activar el evento")]
    /// <summary>
    /// el tag que queremos comparar para activar este evento
    /// </summary>
    public string TargetTag;

    [Tooltip("El evento que queremos activar")]
    /// <summary>
    /// el evento que queremos activar
    /// </summary>
    public UnityEvent TriggerEvent;

    /// <summary>
    /// si este evento llamado ya fue activado
    /// </summary>
    [HideInInspector] public bool _IsTriggered;

}