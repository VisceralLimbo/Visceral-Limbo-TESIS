using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemLogic : MonoBehaviour
{
    [SerializeField] protected InventoryManager Inventory;
    [SerializeField] protected ItemDefinitionSO ItemDefinition;
    [SerializeField] protected PlayerContext _Context;

    private void Start()
    {
        ItemDefinition = GetComponentInParent<ItemDefinitionSO>();
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
    public virtual void Register(InventoryManager inventory,PlayerContext context,ItemDefinitionSO definitionSO = null)
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
        if (other.CompareTag("Player"))
        {
            if(other.TryGetComponent<InventoryManager>(out Inventory))
            {
                Inventory.AddItemStack(ItemDefinition);
            }
        }
    }

}
