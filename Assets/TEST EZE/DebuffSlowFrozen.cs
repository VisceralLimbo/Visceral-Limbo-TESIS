using UnityEngine;
using System.Collections.Generic;

public class DebuffFrozen : BuffBehavior
{
    // hago lista para menejar varios stats. ej ahora estoy tocando attackspeed y movement
    private List<StatIdentifier> _appliedStats = new List<StatIdentifier>();
    [SerializeField] float _slowAmount = 0.5f; // la cantidad de slow, ta en un 50% ahroa para q se note pero lo podemos ajustar obvio xd

    public override void OnApply(BuffManager manager, StatsManager StatMan)
    {
        if (StatMan == null || manager == null || _BuffSO == null) return;

        _buffManager = manager;
        _StatMan = StatMan;

        if (_BuffSO.statIdentifiers == null) return;

        float totalSlow = _slowAmount * _BuffPotency;

        // foreach sobre todos los id q se pusieron en el buffso
        foreach (var statID in _BuffSO.statIdentifiers)
        {
            if (statID != null && _StatMan != null)
            {
                StatModifierFloat _Mod = new StatModifierFloat();
                _Mod.ModifierValueFloat = totalSlow;
                _Mod.ModifierValue = totalSlow;
                _Mod.ModType = ModifierType.PercentMult;
                _Mod.Source = manager;
                _Mod.EffectName = _BuffSO.BuffID;

                _StatMan.UpdateFloatStatValue(statID, _Mod);

                if (!_appliedStats.Contains(statID))
                    _appliedStats.Add(statID);
            }
        }

        Debug.Log("se aplicaron los debufos de frozen a las stats");
    }

    public override void OnAddPotency(int ExtraPotency) 
    {
        _BuffPotency += ExtraPotency;
    }

    public override void OnExpire()
    {
        if (_StatMan != null)
        {
            // remuevo el mod de cada stat q toque
            foreach (var statID in _appliedStats)
            {
                _StatMan.RemoveFloatStatModifier(statID, _BuffSO.BuffID);
            }
            Debug.Log("removido el debuff de las stats");
        }
    }

    public override void OnUpdate(float deltaTime) { }
}
