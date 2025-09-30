using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class StatsManager : MonoBehaviour
{

     public FloatStat[] FloatStatList;

     public Dictionary<string, Stat> StatDictionary = new Dictionary<string, Stat>();

    public event Action<string,float> OnStatChanged;

    private void Awake()
    {
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
