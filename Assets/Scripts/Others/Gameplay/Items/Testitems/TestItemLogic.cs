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

    public override void OnPickUp() // ser agarrado
    {
        Inventory.AddItemStack(_ItemDefinition);
        Destroy(this.gameObject);

        // TO DO:
        // CHANGE THIS HACK!
        // este hack es para curar de manera directa al player

        Health_Component _HP = Inventory.GetComponentInChildren<Health_Component>();

        if(_HP != null) 
        {
            _HP.HealHP(_HealHP, false);
        }

    }
    public override void AddStack()
    {
        if (ItemStacks == 0) // nuevo stack
        {
            var StatManager = _Context.Stats; // stat manager
            StatModifierFloat StatMod = new StatModifierFloat(); // creamos un nuevo modificador de float
            StatMod.ModifierValueFloat = _MaxHP; // seteamos el valor del modificador
            StatMod.ModType = ModifierType.flat; // seteamos el tipo de suma del modificador
            StatMod.EffectName = "TestItemLogicHealthUP"; // le damos un nombre al modificador (util para saber que esta afectand)
            StatMod.Source = this; // el origen del modificador 
            StatManager.UpdateFloatStatValue("MaxHealth", StatMod); // cambiamos el valor de la estadistica guardada en el statmanager
            ItemStacks++; // sumamos un stack
        }
        else if(ItemStacks >= 1) // sumar stat
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
