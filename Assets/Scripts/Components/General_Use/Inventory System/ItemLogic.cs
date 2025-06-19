using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemLogic : MonoBehaviour
{
    [SerializeField] protected InventoryManager Inventory;
    [SerializeField] protected ItemDefinitionSO _ItemDefinition;
    public ItemDefinitionSO ItemDefinitionSO { get { return _ItemDefinition; } }
    [SerializeField] protected PlayerContext _Context;

    
    [SerializeField] protected int ItemStacks;

    [SerializeField] protected bool IsInventoryMaster;

    public void Initialize(ItemDefinitionSO Definition)
    {
        _ItemDefinition = Definition;
    }

    /// <summary>
    /// funcion de pick up de items
    /// </summary>
    public abstract void OnPickUp();

    /// <summary>
    /// funcion de soltado de items
    /// </summary>
    public abstract void OnDrop();


    /// <summary>
    /// funcion para registrar el inventory manager.
    /// cualquier subscripcion de eventos tiene que ser integrada a esta funcion
    /// </summary>
    /// <param name="inventory"></param>
    public virtual void Register(InventoryManager inventory,PlayerContext context)
    {
        Inventory = inventory;
        _Context = context;
       
    }

    /// <summary>
    /// funcion para deregistrar el inventory manager.
    /// cualquier desubscripcion de eventos tiene que ser integrada a esta funcion
    /// </summary>
    /// <param name="inventory"></param>
    public virtual void Unregister()
    {
        


    }

    private void OnTriggerEnter(Collider other)
    {
        print(other.tag);
        if (other.CompareTag("Player"))
        {
            if(other.TryGetComponent(out Inventory))
            {
                Inventory.AddItemStack(ItemDefinitionSO);
                Destroy(this.gameObject);
            }
            print("found player");
        }
    }

    public virtual void AddStack()
    {
        ItemStacks++;
    }

    /// <summary>
    /// remover un stack de item.
    /// RECORDATORIO: esta funcion tambien remueve el item final si el
    /// stack es menor o igual a 0
    /// </summary>
    public virtual void RemoveStack()
    {
        ItemStacks--;
        ItemDefinitionSO.ItemStack--;
        if (ItemStacks <= 0)
        {
            Unregister();
        }
    }

    public virtual void SetInventoryMaster(bool isMaster)
    {
        IsInventoryMaster = isMaster;
    }
}
