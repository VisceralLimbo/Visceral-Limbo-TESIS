using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ModifierType
{
    flat,
    percentAdd,
    PercentMult

}

public abstract class StatModifiers
{
    [SerializeField]public abstract object ModifierValue { get; set; }
    [SerializeField]public ModifierType ModType;
    [SerializeField]public ItemLogic Source;
    [SerializeField] public string EffectName;
}

[Serializable]
public class StatModifierFloat : StatModifiers
{
    public float ModifierValueFloat;

    public override object ModifierValue { get => ModifierValueFloat; set => ModifierValueFloat = (float)value ; }
}
