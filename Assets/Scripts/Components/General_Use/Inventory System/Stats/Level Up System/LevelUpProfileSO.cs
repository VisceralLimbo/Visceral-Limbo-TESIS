using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum GrowthMode
{
    linealFlat,
    GrowthCurve,
    percentageBased
}

public class LevelUpProfileSO : ScriptableObject
{
    List<GrowthProfile> GrowthProfiles= new List<GrowthProfile>();
}

[System.Serializable]
public class GrowthProfile
{
    [SerializeField] StatIdentifier _Stat;
    [SerializeField] GrowthMode _GrowthMode;

    [Header("Solo se aplica una o otra dependiendo del GrowthMode.")]
    [SerializeField] float _GrowthValue;
    [SerializeField] AnimationCurve _GrowthCurve;

}
