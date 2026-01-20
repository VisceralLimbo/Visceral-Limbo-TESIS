using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEffect", menuName = "Visceral_Limbo/Effects/NewEffect")]
public class EnviromentalRoomEffectSO : RoomEffectSO
{
    //tengo dos asi q o es bufo/debufo o visual, aca deberia agrandar esto para otros efectos
    public enum EffectCategory { BuffPlayer, VisualChange}
    public EffectCategory category;

    [Header("Buff Data")]
    public BuffSO buffToApply;
    public int potency = 1;

    [Header("Visual Data")]
    public float targetIntensity = 0.1f;

    private List<float> _oldIntensities = new List<float>();

    public override void ApplyEffect(RoomSpawnerManager manager)
    {
        // logica del frozen
        if (category == EffectCategory.BuffPlayer)
        {
            if (manager.playerContext != null && buffToApply != null)
            {
                manager.playerContext.BuffManager.AddNewBuff(buffToApply.BuffID, buffToApply, potency);
            }
        }

        // logica del lowvisibilty
        if (category == EffectCategory.VisualChange)
        {
            _oldIntensities.Clear();
            var lights = manager.GetCombatLights();
            foreach (var l in lights)
            {
                if (l != null)
                {
                    _oldIntensities.Add(l.intensity);
                    l.intensity = targetIntensity;
                }
            }

            if (manager.blackFog != null)
            {
                manager.blackFog.Play();
            }
        }
    }

    public override void RemoveEffect(RoomSpawnerManager manager)
    {
        if (category == EffectCategory.BuffPlayer)
        {
            if (manager.playerContext != null && buffToApply != null)
                manager.playerContext.BuffManager.ForceExpirationBuff(buffToApply.BuffID);
        }

        if (category == EffectCategory.VisualChange)
        {
            var lights = manager.GetCombatLights();
            for (int i = 0; i < lights.Count; i++)
            {
                if (i < _oldIntensities.Count && lights[i] != null)
                    lights[i].intensity = _oldIntensities[i];
            }

            if (manager.blackFog != null)
                manager.blackFog.Stop();
        }
    }
}