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

    public void ApplyToSingleTarget(BuffManager target)
    {
        if (buffToApply != null)
        {
            target.AddNewBuff(buffToApply.BuffID, buffToApply, potency);
        }
    }

    public override void ApplyEffect(RoomSpawnerManager manager)
    {
        // logica del frozen
        if (category == EffectCategory.BuffPlayer)
        {
            // aplico al player
            if (manager.playerContext != null)
                ApplyToSingleTarget(manager.playerContext.BuffManager);

            // aplico a enemigos
            var enemyBuffs = manager.GetActiveEnemyBuffManagers();
            foreach (var bManager in enemyBuffs)
            {
                ApplyToSingleTarget(bManager);
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
            // remuevo del palyer
            if (manager.playerContext != null && buffToApply != null)
                manager.playerContext.BuffManager.ForceExpirationBuff(buffToApply.BuffID);

            // remuevo de los enemigos
            var enemyBuffs = manager.GetActiveEnemyBuffManagers();
            foreach (var buffManager in enemyBuffs)
            {
                buffManager.ForceExpirationBuff(buffToApply.BuffID);
            }
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