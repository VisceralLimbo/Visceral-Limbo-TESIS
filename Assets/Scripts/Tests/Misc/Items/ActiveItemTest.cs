using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveItemTest : ItemLogic, I_ItemActiveItem
{

    [SerializeField] float timer;

    public override void OnDrop()
    {
        
    }

    public override void OnPickUp()
    {
        print("Got picked up");
        Inventory.AddItemStack(_ItemDefinition);
        Destroy(this.gameObject);
       
    }

    float pulse;
    public void UpdateActiveItem(InventoryManager Manager, StatsManager StatManager)
    {
        pulse++;
        if(pulse > timer)
        {
            pulse = 0;
        }
    }

    public override void AddStack()
    {
        ItemStacks++;
        timer = timer - ItemStacks / 2;
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
