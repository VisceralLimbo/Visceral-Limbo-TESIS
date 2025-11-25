using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.UIElements;

public class LocalEventBusComponent : MonoBehaviour
{

    [Tooltip("Lista de eventos que queremos activar")]
    /// <summary>
    /// Lista de eventos de UnityEvents
    /// </summary>
    [SerializeField] List<UnityEventData> UnityEventList = new List<UnityEventData>();

    [SerializeField] List<EventData> EventList = new List<EventData>();

    [Tooltip("Si queremos que los eventos de este trigger component puedan reactivarse")]
 

    Dictionary<string, UnityEventData> _TriggerDictionary = new Dictionary<string, UnityEventData>();
    Dictionary<string, EventData> _SystemEventDictionary = new Dictionary<string, EventData>();

    private void Awake()
    {
        foreach(UnityEventData Event in UnityEventList)
        {
            if(!_TriggerDictionary.TryAdd(Event.EventID, Event))
            {
                Debug.LogWarning("[Visceral Warning]: Bus de eventos local trato de guardar multiples veces el mismo evento: nombre de evento:" + Event.EventID, this);
            }
        }

        foreach(EventData Event in EventList)
        {
            if(!_SystemEventDictionary.TryAdd(Event.EventID, Event))
            {
                Debug.LogWarning("[Visceral Warning]: Bus de eventos local trato de guardar multiples veces el mismo evento: nombre de evento:" + Event.EventID, this);
            }
        }
    }

    public void TriggerUnityEvent(string EventName)
    {
        if(_TriggerDictionary.TryGetValue(EventName,out UnityEventData EventDT))
        {
            if (EventDT._Reusable)
            {
                EventDT.Event?.Invoke();
            }
            else if(EventDT._Reusable == false && !EventDT._IsTriggered)
            {
                EventDT.Event?.Invoke();
                EventDT._IsTriggered = true;
            }
            else 
            {
                EventDT.Event.RemoveAllListeners();
                return;
            }
        }
        else
        {
            Debug.LogError("[Visceral Error]: Se intento llamar un evento no existente, Nombre del evento: " + EventName,this);
        }
    }

    public void SubscribeToUnityEvent(UnityAction Subscriber,string EventName)
    {
        if(_TriggerDictionary.TryGetValue(EventName,out UnityEventData EventDT))
        {
            EventDT.Event.AddListener(Subscriber);
        }
        else
        {
            Debug.LogError("[Visceral Error]: Se intento subscribir a un evento no existente, Nombre del evento: " + EventName, this);
        }
    }

    public void TriggerEvent(string EventID)
    {
        if(_SystemEventDictionary.TryGetValue(EventID,out EventData EventDT))
        {
            EventDT.LaunchEvent();
        }
        else
        {
            Debug.LogError("[Visceral Error]: Se intento llamar un evento no existente, Nombre del evento: " + EventID, this);
        }
    }

    public void SubscribeToEvent(string EventID, Action MethodToSubscribe)
    {
        if (_SystemEventDictionary.TryGetValue(EventID, out EventData EventDT))
        {
            EventDT.Event += MethodToSubscribe;
        }
        else
        {
            Debug.LogError("[Visceral Error]: Se intento subscribir a un evento no existente, Nombre del evento: " + EventID, this);
        }
    }

    public void OnDestroy()
    {
        foreach(UnityEventData UnityEvent in _TriggerDictionary.Values)
        {
            UnityEvent.Event.RemoveAllListeners();
        }

        foreach(EventData Event in _SystemEventDictionary.Values)
        {
            Event.CleanUp();
        }

    }

    [System.Serializable]
    protected class UnityEventData
    {
        [Tooltip("El ID del evento")]
        /// <summary>
        /// el tag que queremos comparar para activar este evento
        /// </summary>
        public string EventID;

        [Tooltip("El evento que queremos activar")]
        /// <summary>
        /// el evento que queremos activar
        /// </summary>
        public UnityEvent Event;

        /// <summary>
        /// si este evento llamado ya fue activado
        /// </summary>
        [HideInInspector] public bool _IsTriggered;

        /// <summary>
        /// si este trigger puede reactivarse
        /// </summary>
        [SerializeField] public bool _Reusable;
    }

    [System.Serializable]
    protected class EventData
    {

        [Tooltip("El ID Del Evento")]
        /// <summary>
        /// el tag que queremos comparar para activar este evento
        /// </summary>
        public string EventID;

        [Tooltip("El evento que queremos activar")]
        /// <summary>
        /// el evento que queremos activar
        /// </summary>
        public Action Event;

        /// <summary>
        /// si este evento llamado ya fue activado
        /// </summary>
        [HideInInspector] public bool _IsTriggered;

        /// <summary>
        /// Si el evento es reusable
        /// </summary>
        [SerializeField] public bool _Reusable;

        public void LaunchEvent()
        {
            if (_Reusable)
            {
                Event?.Invoke();
            }
            else if( !_Reusable && !_IsTriggered)
            {
                Event?.Invoke();
                _IsTriggered = true;
            }
            else
            {
                // TODO: POTENTIAL MEMORY LEAK DUE TO LACK OF UNSUBSCRIPTION???
                return;
            }

        }

        public void CleanUp()
        {
            Event = null;
        }


    }

}
