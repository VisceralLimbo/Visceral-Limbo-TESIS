using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
[CreateAssetMenu(menuName = "Visceral_Limbo/General/BuffSystem/BuffScriptableObject")]
public class BuffSO : ScriptableObject
{
    [Header("Variables")]

    [Tooltip("Si es un Buff o un Debuff.")]
    public BuffTypes BuffType;

    [Tooltip("ID del buff, usado por el BufManager para sus diccionarios internos")]
    public string BuffID;

    [Tooltip("Nombre del buff, usado para gameplay")]
    public string BuffName;

    [Tooltip("Descripcion del buff")]
    public string BuffDescription;

    [Tooltip("Duracion del buff")]
    public float BuffDuration;

    [Tooltip("Este buff no expira por tiempo?")]
    public bool IsInfinityDuration;

    [Tooltip("Potencia máxima que puede ser aplicada al buff")]
    public float MaxBuffPotency;

    [Space]
    [Header("ADVERTENCIA, UN BUFFO SOLO PUEDE TENER UNO DE LOS DOS ACTIVOS" +
        ", PORQUE SON EXCLUYENTES")]
    [Tooltip("Si a la hora de ser reaplicado, este buffo debería de sobreescribir copias antiguas")]
    public bool ShouldOverrideSameBuffs;

    [Tooltip("Si a la hora de ser reaplicado, este buffo debería de empoderar a la copia vieja.")]
    public bool ShouldScaleWithMultipleInstances;


    [Space]
    [Header("Stats Identifiers")]
    public List<StatIdentifier> statIdentifiers = new List<StatIdentifier>();

    #if UNITY_EDITOR
    [Space]
    [Header("References")]
    [Tooltip("referencia de la clase del buffbehaviour")]
    public MonoScript BuffBehaviorScript;
#endif
    // usado para assembly y getType()
    public string AssemblyQualifiedName;

    private void OnValidate()
    {
        // XOR Switch
        if (ShouldOverrideSameBuffs == ShouldScaleWithMultipleInstances)
        {
            ShouldScaleWithMultipleInstances = !ShouldOverrideSameBuffs;
        }
#if UNITY_EDITOR
        

        if(BuffBehaviorScript != null)
        {
            var T = BuffBehaviorScript.GetClass();
            if( T != null)
            {
                // Sobreescribimos siempre para asegurar que esté al día
                AssemblyQualifiedName = T.AssemblyQualifiedName;

                // Forzar a Unity a saber que el archivo cambió (útil para guardar cambios)
                EditorUtility.SetDirty(this);
            }
        }
        // Si borraste el script, borramos el string para evitar errores fantasma
        else
        {
            AssemblyQualifiedName = "";
        }

#endif
    }

}
public enum BuffTypes
{
    Buff,
    Debuff,
}
