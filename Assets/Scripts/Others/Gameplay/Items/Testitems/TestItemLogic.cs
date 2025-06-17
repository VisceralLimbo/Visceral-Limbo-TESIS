using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestItemLogic : ItemLogic
{
    //[SerializeField] protected InventoryManager Inventory;
    //[SerializeField] protected ItemDefinitionSO ItemDefinition;

    public override void OnDrop()
    {
        
    }

    public override void OnPickUp()
    {
        var StatManager = _Context.Stats;
        StatModifierFloat StatMod = new StatModifierFloat();
        StatMod.ModifierValueFloat = 25 * ItemDefinition.ItemStacks;
        StatMod.ModType = ModifierType.flat;

        StatManager.UpdateFloatStatValue("MaxHealth", StatMod);

    }

    public override void Register(InventoryManager inventory,PlayerContext Context, ItemDefinitionSO definitionSO = null)
    {
        base.Register(inventory,Context, definitionSO);
    }

    public override void Unregister()
    {
        base.Unregister();
    }
}
