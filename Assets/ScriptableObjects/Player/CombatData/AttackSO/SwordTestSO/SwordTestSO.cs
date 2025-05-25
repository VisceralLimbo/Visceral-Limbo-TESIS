using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Visceral_Limbo/Combat Data/Attack Data/Scriptable Objects/ Sword Test SO") ]
public class SwordTestSO : ScriptableObject
{

    public AnimatorOverrideController _AnimatorOV;
    public float Damage;
    public float KnockBack;
    public float HyperArmor;
    public string AttackName;

}
