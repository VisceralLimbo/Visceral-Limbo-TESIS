using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Visceral_Limbo/Combat Data/Attack Data/Scriptable Objects/ AttackComboLoadoutSO")]
public class Player_AttackComboLoadoutSO : ScriptableObject
{
    [Header("ID")]
    public string WeaponName;

    [Tooltip("Tiempo muerto entre combo")]
    public float ComboResetTime;

    [Space]
    [Header("Combo Animations")]
    [Tooltip("La secuencia de ataques del combo")]
    public SwordTestSO[] ComboSequence;

    [Space]
    [Header("Visuals / Sounds")]
    [Tooltip("Los sonidos default de ataques del arma")]
    public SoundData[] DefaultSounds;
}
