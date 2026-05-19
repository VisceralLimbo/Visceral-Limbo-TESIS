using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "NewEffect", menuName = "Visceral_Limbo/Effects/NewEffect")]
public class EnviromentalRoomEffectSO : RoomEffectSO
{
    [Flags]
    public enum EffectCategory
    {
        None = 0,
        BuffPlayer = 1,
        VisualChange = 2
    }

    public EffectCategory category;

    // tipo especifico del efecto
    public RoomSpawnerManager.EnvironmentalEffect effectType;

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
        // =========================
        // FROZEN (BUFF / DEBUFF)

        if ((category & EffectCategory.BuffPlayer) != 0)
        {
            // player
            if (manager.playerContext != null)
            {
                ApplyToSingleTarget(manager.playerContext.BuffManager);
            }

            // enemigos
            var enemyBuffs = manager.GetActiveEnemyBuffManagers();

            foreach (var bManager in enemyBuffs)
            {
                ApplyToSingleTarget(bManager);
            }
        }

        // =========================
        // VISUAL EFFECTS

        if ((category & EffectCategory.VisualChange) != 0)
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

            // =========================
            // LOW VISIBILITY

            if (effectType == RoomSpawnerManager.EnvironmentalEffect.LowVisibility)
            {
                if (manager.blackFog != null)
                {
                    manager.blackFog.Play();
                }

                HealthFullscreenEffect.Instance.FadeDarkness(0.8f, 1.5f);

                HealthFullscreenEffect.Instance.FadeBlackAndWhite(1f, 1f);
            }

            // =========================
            // FROZEN VISUALS

            if (effectType == RoomSpawnerManager.EnvironmentalEffect.Frozen)
            {
                HealthFullscreenEffect.Instance.FadeFreeze(1f, 1.5f);

                if (manager.snowParticles != null)
                {
                    manager.snowParticles.Play();
                }
            }
        }
    }

    public override void RemoveEffect(RoomSpawnerManager manager)
    {
        // =========================
        // REMOVE BUFFS

        if ((category & EffectCategory.BuffPlayer) != 0)
        {
            // player
            if (manager.playerContext != null && buffToApply != null)
            {
                manager.playerContext.BuffManager
                    .ForceExpirationBuff(buffToApply.BuffID);
            }

            // enemigos
            var enemyBuffs = manager.GetActiveEnemyBuffManagers();

            foreach (var buffManager in enemyBuffs)
            {
                buffManager.ForceExpirationBuff(buffToApply.BuffID);
            }
        }

        // =========================
        // REMOVE VISUALS

        if ((category & EffectCategory.VisualChange) != 0)
        {
            var lights = manager.GetCombatLights();

            for (int i = 0; i < lights.Count; i++)
            {
                if (i < _oldIntensities.Count && lights[i] != null)
                {
                    lights[i].intensity = _oldIntensities[i];
                }
            }

            // =========================
            // LOW VISIBILITY REMOVE

            if (effectType == RoomSpawnerManager.EnvironmentalEffect.LowVisibility)
            {
                if (manager.blackFog != null)
                {
                    manager.blackFog.Stop();
                }

                HealthFullscreenEffect.Instance.FadeDarkness(0f, 1.5f);

                HealthFullscreenEffect.Instance.FadeBlackAndWhite(0f, 1f);
            }

            // =========================
            // FROZEN REMOVE

            if (effectType == RoomSpawnerManager.EnvironmentalEffect.Frozen)
            {
                HealthFullscreenEffect.Instance.FadeFreeze(0f, 1.5f);

                if (manager.snowParticles != null)
                {
                    manager.snowParticles.Stop();
                }
            }
        }
    }
}