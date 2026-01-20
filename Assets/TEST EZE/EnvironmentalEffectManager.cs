using UnityEngine;
using System.Collections.Generic;

public class EnvironmentalEffectManager : MonoBehaviour
{
    //manager de los efectos para las salas
    private RoomSpawnerManager _roomManager;

    [System.Serializable]
    public struct EffectMapping
    {
        public RoomSpawnerManager.EnvironmentalEffect type;
        public EnviromentalRoomEffectSO effectData;
    }

    public List<EffectMapping> availableEffects;
    private EnviromentalRoomEffectSO _activeEffect;

    private void Awake() => _roomManager = GetComponent<RoomSpawnerManager>();

    //manejo cuando empieza y termina el combate para saber cuando inicia y termina el efecto en la sala
    private void OnEnable()
    {
        _roomManager.OnCombatStart += HandleStart;
        _roomManager.OnCombatEnded += HandleEnd;
    }

    private void OnDisable()
    {
        _roomManager.OnCombatStart -= HandleStart;
        _roomManager.OnCombatEnded -= HandleEnd;
    }

    private void HandleStart()
    {
        var current = _roomManager.GetEffect();
        var mapping = availableEffects.Find(x => x.type == current);
        if (mapping.effectData != null)
        {
            _activeEffect = mapping.effectData;
            _activeEffect.ApplyEffect(_roomManager);
        }
    }

    private void HandleEnd()
    {
        if (_activeEffect != null)
        {
            _activeEffect.RemoveEffect(_roomManager);
            _activeEffect = null;
        }
    }
}
