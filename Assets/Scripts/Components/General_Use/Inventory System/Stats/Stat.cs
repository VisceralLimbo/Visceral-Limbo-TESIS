using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

/// <summary>
/// CLASE BASE
/// </summary>

[System.Serializable]
public abstract class Stat
{
    /// <summary>
    /// valor base, no tocar
    /// </summary>
    public abstract object BaseValue { get; set; }

    /// <summary>
    /// valor final con modificadores
    /// </summary>
    public abstract float FinalValue { get;}

    /// <summary>
    /// identificador de stat
    /// </summary>

    public string StatID;

    /// <summary>
    /// modificadores activos de stat
    /// </summary>
    /// <param name="mod"></param>
    public abstract void AddModifiers(StatModifiers mod, string EffectID);

    /// <summary>
    /// recalcular stat
    /// </summary>
    public abstract void RecalculateStat();

    [SerializeField]protected StatsManager Statmanager;

    public virtual void SetCurrentManager(StatsManager manager)
    {
        Statmanager = manager;
    }

}

[System.Serializable]
public class FloatStat : Stat
{
     public override object BaseValue { get => BaseValueFloat; set => BaseValueFloat = (float)value; }

     public override float FinalValue => FinalValueFloat;

     public float BaseValueFloat;

     public float FinalValueFloat;

     public List<StatModifierFloat> ModifiersList = new();
     private Dictionary<string, StatModifierFloat> Modifiers = new Dictionary<string, StatModifierFloat>();

    

    public override void AddModifiers(StatModifiers mod,string EffectID)
    {
        var floatmod = (StatModifierFloat)mod;

        if(EffectID == "")
        {
            Debug.LogError("Key is null" + mod.EffectName);
            return;
        }

        if(!Modifiers.ContainsKey(EffectID))
        {
            Modifiers.Add(EffectID, floatmod);

            ModifiersList.Add(floatmod);
        }
        else
        {
            Debug.Log("modificador ya existe: " + mod.ToString() + mod.ModifierValue);
            Modifiers[EffectID] = floatmod;
            var debuglist = ModifiersList.First(x => x.EffectName == floatmod.EffectName);
            ModifiersList.Remove(debuglist);
            ModifiersList.Add(floatmod);
            Debug.Log(Modifiers[EffectID].ModifierValue);
        }

        RecalculateStat(); 
    }


    public override void RecalculateStat()
    { 
        float newFinalValue = BaseValueFloat;

        var OrderedList = Modifiers.Values.OrderBy(x => x.ModType).ToList();

        foreach (StatModifierFloat _Mod in OrderedList)
        {
            if (_Mod.ModType == ModifierType.flat)
            {
                newFinalValue += _Mod.ModifierValueFloat;
            }
            else if (_Mod.ModType == ModifierType.percentAdd)
            {
                newFinalValue += (BaseValueFloat * _Mod.ModifierValueFloat);
            }
            else
            {
                newFinalValue *= _Mod.ModifierValueFloat;
            }
        }

        if (newFinalValue != FinalValueFloat)
        {
            FinalValueFloat = newFinalValue;
        }
    }

    // no habia para remover y me tome la libertad de hacerlo (para cumplir con el item de doble de daño al     30% de vida)
    public void RemoveModifierByEffectID(string EffectID)
    {
        if (Modifiers.ContainsKey(EffectID))
        {
            // remuevo de list
            var modToRemove = Modifiers[EffectID];
            ModifiersList.Remove(modToRemove);

            // remuevo del diccionario
            Modifiers.Remove(EffectID);

            // recalculo
            RecalculateStat();
        }


       
    }

    public FloatStat(FloatStat original)
    {
        this.StatID = original.StatID;
        this.FinalValueFloat = original.FinalValueFloat;
        this.BaseValue = original.BaseValue;
        this.Modifiers = new Dictionary<string, StatModifierFloat>();
        this.ModifiersList = new List<StatModifierFloat>();
        this.BaseValueFloat = original.BaseValueFloat;

    }

    /*var OrderedList = ModifiersList.OrderBy(x => x.ModifierValue).ToList();

   foreach(StatModifierFloat _Mod in OrderedList)
   {
       if(_Mod.ModType == ModifierType.flat)
       {
           newFinalValue += _Mod.ModifierValueFloat;
       }
       else if(_Mod.ModType == ModifierType.percentAdd)
       {
           newFinalValue += (BaseValueFloat * _Mod.ModifierValueFloat);
       }
       else
       {
           newFinalValue *= _Mod.ModifierValueFloat;
       }
   }

   if(newFinalValue != FinalValueFloat)
   {
       FinalValueFloat = newFinalValue;
   }
   */


}

