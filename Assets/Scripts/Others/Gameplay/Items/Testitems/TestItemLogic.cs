using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestItemLogic : ItemLogic
{

    [SerializeField] float _MaxHP, _HealHP;
    //[SerializeField] protected InventoryManager Inventory;
    //[SerializeField] protected ItemDefinitionSO ItemDefinition;

    public override void OnDrop()
    {

    }

    public override void OnPickUp()
    {
        Inventory.AddItemStack(_ItemDefinition);
        Destroy(this.gameObject);

        // TO DO:
        // CHANGE THIS HACK!

        Health_Component _HP = Inventory.GetComponentInChildren<Health_Component>();

        if(_HP != null)
        {
            _HP.HealHP(_HealHP, false);
        }

    }
    public override void AddStack()
    {
        if (ItemStacks == 0)
        {
            var StatManager = _Context.Stats;
            StatModifierFloat StatMod = new StatModifierFloat();
            StatMod.ModifierValueFloat = _MaxHP;
            StatMod.ModType = ModifierType.flat;
            StatMod.EffectName = "TestItemLogicHealthUP";
            StatMod.Source = this;
            StatManager.UpdateFloatStatValue("MaxHealth", StatMod);
            ItemStacks++;
        }
        else if(ItemStacks >= 1)
        {
            ItemStacks++;
            var StatManager = _Context.Stats;
            StatModifierFloat StatMod = new StatModifierFloat();
            StatMod.ModifierValueFloat = _MaxHP * ItemStacks;
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
        if (IsInventoryMaster) return;

        print(other);
        if (other.CompareTag("Player"))
        {
            if (other.transform.parent.TryGetComponent(out PlayerContext context))
            {
                _Context = context;
                Inventory = context.Inventory;
                OnPickUp();
            }
            else
            {
                print("no inventory?");
            }
            print("found player");
        }
    }
}
