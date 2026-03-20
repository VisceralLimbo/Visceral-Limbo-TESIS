using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUpComponent : MonoBehaviour
{
    [SerializeField] LevelUpProfileSO _LevelProfile;
    [SerializeField] StatsManager _StatManager;

    private void Start()
    {
        if(_StatManager == null) _StatManager = GetComponent<StatsManager>();
        GlobalLevelManager.OnGlobalLevelStageChange += ApplyGrowth;

        Health_Component _HpComp = GetComponent<Health_Component>();
        if(_HpComp == null) _HpComp = GetComponentInChildren<Health_Component>();

        if (_HpComp != null) _HpComp.OnDeath += Unsubscribe;
    }

    private void ApplyGrowth(int GlobalLevel)
    {

        foreach(GrowthProfile Growth in _LevelProfile.GrowthProfiles)
        {
            StatModifierFloat Mod = new StatModifierFloat();
            Mod.Source = Growth;
            Mod.EffectName = "Level Scaling";

            switch (Growth._GrowthMode)
            {
                case GrowthMode.linealFlat:

                    float FinalGrowth = Growth._GrowthValue * GlobalLevel;
                    Mod.ModType = ModifierType.flat;
                    Mod.ModifierValue = FinalGrowth;
                    _StatManager.UpdateFloatStatValue(Growth._Stat,Mod);


                    break;

                case GrowthMode.GrowthCurve:

                    // Obtenemos el nivel maximo de la curva
                    float maxCurveLevel = Growth._GrowthCurve.keys[Growth._GrowthCurve.keys.Length - 1].time;

                    float finalValue = 0f;

                    if (GlobalLevel <= maxCurveLevel)
                    {
                        // Si estamos dentro del gráfico, evaluamos la curva normalmente
                        finalValue = Growth._GrowthCurve.Evaluate(GlobalLevel);
                    }
                    else
                    {
                        // Si nos pasamos, agarramos el maximo y lo escalamos linealmente
                        float maxCurveValue = Growth._GrowthCurve.Evaluate(maxCurveLevel);
                        int extraLevels = GlobalLevel - (int)maxCurveLevel;

                        finalValue = maxCurveValue + (extraLevels * Growth._GrowthValue);
                    }

                    Mod.ModifierValue = finalValue;
                    Mod.ModType = ModifierType.flat;

                    _StatManager.UpdateFloatStatValue(Growth._Stat, Mod);


                    break;
                case GrowthMode.percentageBased:
                    float FinalPercentageGrowth = Growth._GrowthValue * GlobalLevel;
                    Mod.ModType = ModifierType.percentAdd;
                    Mod.ModifierValue = FinalPercentageGrowth;
                    _StatManager.UpdateFloatStatValue(Growth._Stat, Mod);


                    break;
            }
        }
    }

    private void Unsubscribe()
    {
        GlobalLevelManager.OnGlobalLevelStageChange -= ApplyGrowth;
    }

    private void OnDestroy()
    {
        GlobalLevelManager.OnGlobalLevelStageChange -= ApplyGrowth;
    }

    private void OnDisable()
    {
        GlobalLevelManager.OnGlobalLevelStageChange -= ApplyGrowth;
    }
}
