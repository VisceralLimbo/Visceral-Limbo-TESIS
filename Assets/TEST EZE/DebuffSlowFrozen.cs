using UnityEngine;

public class DebuffFrozen : BuffBehavior
{
    [SerializeField] StatIdentifier _StatID;
    [SerializeField] float _slowAmount = -0.5f; // la cantidad de slow, ta en un 50% ahroa para q se note pero lo podemos ajustar obvio xd

    public override void OnApply(BuffManager manager, StatsManager StatMan)
    {
        _buffManager = manager;
        _StatMan = StatMan;

        if(_BuffSO.statIdentifiers.Count > 0)
        {
            _StatID = _BuffSO.statIdentifiers[0];
        }

        StatModifierFloat _Mod = new StatModifierFloat();
        float totalSlow = _slowAmount * _BuffPotency;

        _Mod.ModifierValueFloat = totalSlow;
        _Mod.ModifierValue = totalSlow;
        _Mod.ModType = ModifierType.percentAdd;
        _Mod.Source = manager;
        _Mod.EffectName = _BuffSO.BuffID; // USO EL ID DEL SO NA BRONCA MEDIA HORA PARA ESTO

        _StatMan.UpdateFloatStatValue(_StatID, _Mod);
        //si no le ponia colorcito no lo veia con la cantidad de cosas en consola q hay xd
        Debug.Log("<Color=blue>SE APLICO EL DEBUFF INCREIBLE</Color>");
    }

    public override void OnAddPotency(int ExtraPotency) 
    {
        _BuffPotency += ExtraPotency;
    }

    public override void OnExpire()
    {
        if (_StatMan != null)
        {
            _StatMan.RemoveFloatStatModifier(_StatID, _BuffSO.BuffID);
            Debug.Log("REMUEVO EL DEBUFO");
        }
    }

    public override void OnUpdate(float deltaTime) { }
}
