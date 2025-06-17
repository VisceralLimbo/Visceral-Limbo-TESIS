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
    public abstract void AddModifiers(StatModifiers mod);

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

     public List<StatModifierFloat> Modifiers = new();

    public override void AddModifiers(StatModifiers mod)
    {
        if (!Modifiers.Contains((StatModifierFloat)mod))
        {
            Modifiers.Add((StatModifierFloat)mod);
        }
    }


    public override void RecalculateStat()
    { 


        float newFinalValue = BaseValueFloat;

       var OrderedList = Modifiers.OrderBy(x => x.ModType).ToList();

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
        
    }
}

