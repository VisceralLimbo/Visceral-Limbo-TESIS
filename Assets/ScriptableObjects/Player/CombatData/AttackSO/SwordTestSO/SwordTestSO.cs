using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Visceral_Limbo/Combat Data/Attack Data/Scriptable Objects/ Sword Test SO") ]
public class SwordTestSO : ScriptableObject
{
    //
    // Character Identifiers:
    // Executioner : Exe_
    [Header("Animation Info")]

    [Tooltip("La ID de la animacion del ataque, respetar asignaciones y chequear por mayus, usar camelback y backspaces en lugar de espacios." +
        "respetar comienzo por personaje: Exe => ejecutor ")]
    public string AttackID = "Exe_Attack_Null";


    [Tooltip("la duracion de la animacion, tiene que ser igual o ligeramente menor que la duracion del clip")]
    public float AnimationLenght = 1.0f;

    [Header("Gameplay Info")]
    [Tooltip("Daño base del ataque, este daño se le suma otros valores de daño del player")]
    public float Damage;

    [Tooltip("knockback base del ataque, este knockback se le suma otros valores de knockback del player")]
    public float KnockBack;
    [Tooltip("Hiperarmadura, si el jugador puede ser knockeado del ataque")]

    public bool HyperArmor;

    [Header("Config")]

    [Tooltip("la ID hasheada, llevar esto hacia el animator para configurar el state machine")]
    public int HashedID;

    public void OnValidate()
    {
        HashedID = HashStringToInt(AttackID ?? "");

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // marca el objeto como "sucio" para que Unity guarde el valor en escena/asset
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }

    public void RecalculateHash() => HashStringToInt(AttackID ?? "");

    private int HashStringToInt(string s)
    {
        const uint fnvOffset = 2166136261u;
        const uint fnvPrime = 16777619u;
        uint hash = fnvOffset;

        for(int I = 0; I < s.Length; I++)
        {
            hash ^= s[I];
            hash *= fnvPrime;
        }

        return unchecked ((int)hash);
    }

}
