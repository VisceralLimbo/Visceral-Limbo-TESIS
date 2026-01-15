using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class StatsManager : MonoBehaviour
{
    [Tooltip("Quick Setup, cargar SO aquí con los datos de stats deseados")]
     public StatBlockData blockData;

    [Tooltip("Los Stats del StatManager, estos valores son de setup, que quedan guardados internamente")]
     public List<FloatStat> FloatStatList = new List<FloatStat>();

     public Dictionary<string, Stat> StatDictionary = new Dictionary<string, Stat>();

    public event Action<string,float> OnStatChanged;

    private void Awake()
    {
        // si tenemos un blockstat
        if(blockData!= null)
        {
            // COMO ESTAMOS USANDO UN SCRIPTABLEOBJECT
            // NOS VEMOS OBLIGADOS A CREAR UNA COPIA DE LA LISTA DE ESTADISTICAS
            // PARA EVITAR ACCIDENTALMENTE MODIFICAR LA LISTA DE STATS DEL SO.
            foreach(var stat in blockData.Stats)
            {
                FloatStat ClonedStat = new FloatStat(stat);
                FloatStatList.Add(ClonedStat);
            }
            
        }

        var StatList = new List<Stat>();
        foreach(var stat in FloatStatList)
        {
            StatList.Add(stat);
        }
       

        foreach(Stat stat in StatList)
        {
            if(StatDictionary.TryGetValue(stat.StatID,out Stat value))
            {
                print("existe stat: " + value.StatID);
                continue;
            }
            else
            {
                StatDictionary.Add(stat.StatID, stat);
                print("añadimos stat: " + stat.StatID + " con valor: " + stat.FinalValue);
                stat.SetCurrentManager(this);
            }

        }
    }

    public float GetFloatStatValue(string StatID)
    {
        float returningvalue = 0;

        if(StatDictionary.TryGetValue(StatID, out Stat value)) 
        { 
            if(value is FloatStat floatvalue)
            {
                floatvalue.RecalculateStat();
                returningvalue = floatvalue.FinalValue;
            }
            else
            {
                Debug.LogError("[Visceral Error] StatManager, accessed stat wasnt a float stat");
                return -666;
            }
            return returningvalue;
        }
        else
        {
            Debug.LogError("[Visceral Error] StatManager: no stat found under key " + StatID);
            return -666;
        }
    }

    public void UpdateFloatStatValue(string StatID,StatModifiers _Mod)
    {
        if (StatDictionary.TryGetValue(StatID, out Stat value))
        {
            if (value is FloatStat floatvalue)
            {
                floatvalue.AddModifiers(_Mod,_Mod.EffectName);
                OnStatChanged?.Invoke(StatID,floatvalue.FinalValueFloat);
                print("Updating value" + StatID);
            }
            else
            {
                Debug.LogError("[Visceral Error] StatManager, accessed stat wasnt a float stat");
                return;
            }
        }
        else
        {
            Debug.LogWarning("[Visceral Error] StatManager, no stat exist called " + StatID);
        }
    }

    // lo mismo que en stat no haba remover haci q lo hice 
    public void RemoveFloatStatModifier(string StatID, string EffectID)
    {
        if (StatDictionary.TryGetValue(StatID, out Stat value))
        {
            if (value is FloatStat floatvalue)
            {
                floatvalue.RemoveModifierByEffectID(EffectID);

                // notif el cambio dsp de recalcular
                OnStatChanged?.Invoke(StatID, floatvalue.FinalValue);
                Debug.Log($"remuevo modificador de EffectID: {EffectID} de stat: {StatID}.");
            }
            else
            {
                Debug.LogError($"[Visceral Error] StatManager, stat {StatID} wasn't a float stat");
                return;
            }
        }
    }

}
