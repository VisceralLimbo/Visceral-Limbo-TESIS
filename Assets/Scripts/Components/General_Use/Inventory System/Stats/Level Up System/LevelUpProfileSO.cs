using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GrowthMode
{
    linealFlat,
    GrowthCurve,
    percentageBased
}


[CreateAssetMenu(menuName = "Visceral_Limbo/General/LevelUpSystem/LevelUpProfileScriptableObject")]
public class LevelUpProfileSO : ScriptableObject
{
    public List<GrowthProfile> GrowthProfiles= new List<GrowthProfile>();
}

[System.Serializable]
public class GrowthProfile
{
    [SerializeField] public StatIdentifier _Stat;
    [SerializeField] public GrowthMode _GrowthMode;

    [Header("Solo se aplica una o otra dependiendo del GrowthMode.")]
    [SerializeField] public float _GrowthValue;
    [SerializeField] public AnimationCurve _GrowthCurve;

}
