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
        AddStack();
     
    }
    public override void AddStack()
    {
        if (ItemDefinitionSO.ItemStack == 0)
        {
            var StatManager = _Context.Stats;
            StatModifierFloat StatMod = new StatModifierFloat();
            StatMod.ModifierValueFloat = 25;
            StatMod.ModType = ModifierType.flat;
            StatMod.EffectName = "TestItemLogicHealthUP";
            StatMod.Source = this;
            StatManager.UpdateFloatStatValue("MaxHealth", StatMod);
            ItemDefinitionSO.ItemStack++;
        }
        else if(ItemDefinitionSO.ItemStack >= 1)
        {
            ItemDefinitionSO.ItemStack++;
            var StatManager = _Context.Stats;
            StatModifierFloat StatMod = new StatModifierFloat();
            StatMod.ModifierValueFloat = 25 * ItemDefinitionSO.ItemStack;
            StatMod.ModType = ModifierType.flat;
            StatMod.EffectName = "TestItemLogicHealthUP";
            StatMod.Source = this;
            StatManager.UpdateFloatStatValue("MaxHealth", StatMod);
        }

    }

    public override void Register(InventoryManager inventory,PlayerContext Context)
    {
        base.Register(inventory,Context);
    }

    public override void RemoveStack()
    {
        base.RemoveStack();
    }

    public override void Unregister()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        print(other);
        if (other.CompareTag("Player"))
        {
            if (other.transform.parent.TryGetComponent(out PlayerContext context))
            {
                _Context = context;
                Inventory = context.Inventory;
                Inventory.AddItemStack(this);
            }
            else
            {
                print("no inventory?");
            }
            print("found player");
        }
    }
}
