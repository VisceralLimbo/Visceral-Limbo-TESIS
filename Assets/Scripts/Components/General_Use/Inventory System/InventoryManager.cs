using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public List<ItemDefinitionSO> Inventory { get => _Inventory; }

    [SerializeField] private List<ItemDefinitionSO> _Inventory;

    [SerializeField] StatsManager _StatsManager;

    [SerializeField] PlayerContext _Context;

    /// <summary>
    /// funcion para añadir un item / un stack
    /// RECORDATORIO: esta funcion maneja tanto añadir items nuevos
    /// como sumar stacks de items
    /// </summary>
    /// <param name="item"> la referencia del item a sumar</param>
    public void AddItemStack(ItemDefinitionSO item)
    {
        if (_Inventory.Contains(item))
        {
            _Inventory.Find(x => x.ItemID == item.ItemID).AddStack();
        }
        else
        {
            _Inventory.Add(item);
        }

        item.Logic.Register(this,_Context, item);
        item.Logic.OnPickUp();
    }

    /// <summary>
    /// funcion para restar un stack de un item del inventario
    /// RECORDATORIO: USAR ESTA FUNCION CADA VEZ QUE SE QUIERA RESTAR UN ITEM
    /// INCLUSO REMOVER, NO USAR REMOVEITEM A MENOS QUE ESTES SEGURO
    /// </summary>
    /// <param name="item">Item a restar un stack</param>
    public void LoseItemStack(ItemDefinitionSO item)
    {
        if (!_Inventory.Contains(item))
        {
            Debug.LogError("[Visceral Error] Inventory manager: item removido"
                 + "no existe en el inventario: " + item.name + " ID: " + item.ItemID);
            return;
        }
         _Inventory.Find(x=>x.ItemID == item.ItemID).RemoveStack();
        item.Logic.OnDrop();
    }

    /// <summary>
    /// Funcion para remover un item del inventario.
    /// RECORDATORIO: ESTA FUNCION REMUEVE EL ITEM DE LA LISTA ENTERA
    /// NO ES PARA RESTAR STACKS, ES PARA LIMPIAR LA REFERENCIA ENTERA
    /// USAR CON DELICADEZA
    /// </summary>
    /// <param name="item">item a remover del inventario</param>
    public void RemoveItem(ItemDefinitionSO item)
    {
        if (!_Inventory.Contains(item))
        {
            Debug.LogError("[Visceral Error] Inventory manager: item removido"
            + "no existe en el inventario: " + item.name + " ID: " + item.ItemID);
            return;
        }
        _Inventory.Remove(item);
    }
}
