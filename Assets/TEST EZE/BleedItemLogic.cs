using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BleedItemLogic : ItemLogic
{
    // danio base por tick
    [SerializeField] private float baseBleedDamage = 2f;
    // duracion del sangrado
    [SerializeField] private float bleedDuration = 5f;
    // tickrate
    [SerializeField] private float bleedTickRate = 1f;

    // refe de la espada
    private SwordTest _Sword;

    private void OnTriggerEnter(Collider other)
    {
        if (IsInventoryMaster) return;

        if (other.CompareTag("Player"))
        {
            if (other.transform.parent.TryGetComponent(out PlayerContext context))
            {
                _Context = context;
                Inventory = context.Inventory; 
                OnPickUp();
            }
        }
    }

    public override void Register(InventoryManager inventory, PlayerContext context)
    {
        base.Register(inventory, context);
        //agarro el swordtest
        _Sword = _Context.GetComponentInChildren<SwordTest>();
    }

    public override void Unregister()
    {
        base.Unregister();
    }

    public override void OnPickUp()
    {
        Inventory.AddItemStack(_ItemDefinition);
        Destroy(this.gameObject);
    }

    public override void OnDrop()
    {
    }

    public override void AddStack()
    {
        if (ItemStacks == 0)
        {
            var statManager = _Context.Stats;

            StatModifierFloat statMod = new StatModifierFloat // stat modificador de danio de sangrado
            {
                ModifierValueFloat = baseBleedDamage,
                ModType = ModifierType.flat,
                EffectName = "BleedItemLogic",
                Source = this
            };
            statManager.UpdateFloatStatValue("Bleed", statMod);
            ItemStacks++;
        }
        else if (ItemStacks >= 1)
        {
            ItemStacks++;
            var statManager = _Context.Stats;
            StatModifierFloat statMod = new StatModifierFloat // stat modificador de danio de sangrado
            {
                ModifierValueFloat = baseBleedDamage * ItemStacks,
                ModType = ModifierType.flat,
                EffectName = "BleedItemLogic",
                Source = this
            };
            statManager.UpdateFloatStatValue("Bleed", statMod);
        }
    }

    public override void RemoveStack()
    {
        base.RemoveStack();
    }

    // func para obtener los valores del sangrado de swordtest
    public float GetBleedDamage() => baseBleedDamage * ItemStacks;
    public float GetBleedDuration() => bleedDuration;
    public float GetBleedTickRate() => bleedTickRate;
}
