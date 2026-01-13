using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

/// <summary>
/// Este script servirá para pasar contexto basico del usuario, elementos que sepamos que van a ser utiles o muy pesados de andar obteniendo
/// </summary>
public class PlayerContext : Visceral_Script
{

    public StatsManager Stats;
    public InventoryManager Inventory;

    [Tooltip("El manager de buffos")]
    public BuffManager BuffManager;

    /// <summary>
    /// El GameObject general del usuario
    /// </summary>
    public GameObject PlayerGameObject;
    /// <summary>
    /// El transform del usuario
    /// </summary>
    public Transform PlayerTransform;

    /// <summary>
    /// El Kinematic Character Motor del usuario
    /// </summary>
    public KinematicCharacterMotor KCCMotor;

    public IKnockback knockback;

    public override void VS_Initialize()
    {
        //PlayerGameObject = gameObject;
        //PlayerTransform= GetComponent<Transform>();

        knockback = this.transform.root.GetComponentInChildren<IKnockback>();

        // por las dudas de q sea null
        if (Stats != null && faction == FactionID.Player)
        {
            ApplyMetaUpgrades();
        }

    }

    /// <summary>
    /// La ID de la faccion que este personaje pertenece
    /// </summary>
    public FactionID faction;

    public float EnemyValueScore;

    // aplico los upgrades q compre
    public void ApplyMetaUpgrades()
    {
        if (MetaProgressionManager.Instance == null)
        {
            Debug.LogError("no puse MetaProgressionManager xd no anda");
            return;
        }

        // metassource seria el source this
        object metaSource = this;

        // mejora de vida maxima
        if (MetaProgressionManager.Instance.IsUpgradeBought(MetaProgressionManager.KEY_HEALTH_UP_BOUGHT))
        {
            StatModifierFloat healthMod = new StatModifierFloat
            {
                ModifierValueFloat = MetaProgressionManager.PERM_MAX_HEALTH_BOOST, // la mejora es de +10 vida
                ModType = ModifierType.flat,
                EffectName = "MetaHealthUpgrade_Permanent",
                Source = metaSource
            };
            Stats.UpdateFloatStatValue("MaxHealth", healthMod);
        }

        // mejora daño base
        if (MetaProgressionManager.Instance.IsUpgradeBought(MetaProgressionManager.KEY_DAMAGE_UP_BOUGHT))
        {
            StatModifierFloat damageMod = new StatModifierFloat
            {
                ModifierValueFloat = MetaProgressionManager.PERM_BASE_DAMAGE_BOOST, // la mejora es de +2 dmg
                ModType = ModifierType.flat,
                EffectName = "MetaFlatDamageUpgrade_Permanent",
                Source = metaSource
            };
            Stats.UpdateFloatStatValue("BaseDamageFlatBoost", damageMod);
        }

        // chequeo
        float finalHealth = Stats.GetFloatStatValue("MaxHealth");
        Debug.Log($"[META CHECK] Vida Máxima Final: {finalHealth}");
        float finalDamage = Stats.GetFloatStatValue("BaseDamageFlatBoost");
        Debug.Log($"[META CHECK] Daño Base Final: {finalDamage}");
    }
}

public enum FactionID
{
    Player,
    LimboMonster1,
    LimboMonster2,
    LimboEntity,
    LimboTrap,
}
