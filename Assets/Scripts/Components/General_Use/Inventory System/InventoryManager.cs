using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public List<ItemLogic> Inventory = new List<ItemLogic>();

    [SerializeField] private Dictionary<ItemDefinitionSO,ItemLogic> _Inventory = new Dictionary<ItemDefinitionSO, ItemLogic>();

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

        /*
        if (_Inventory.Contains(item))
        {
           var Item =  _Inventory.Find(x => x == item);
            
        }
        else
        {
            _Inventory.Add(item);
            item.Register(this, _Context);
        }
        item.OnPickUp();
        */

        //nuevo item
        if (!_Inventory.ContainsKey(item))
        {
            //instanciado de items
            var newItem = Instantiate(item.ItemDataPrefab,this.transform.position,Quaternion.identity,this.transform);
            var NewComponentItem=  newItem.GetComponent<ItemLogic>();
            NewComponentItem.Register(this, _Context);

            //seteo de variables
            NewComponentItem.SetInventoryMaster(true);
            NewComponentItem.AddStack();
            
            _Inventory[item] = NewComponentItem;
            print("nuevo item adquirido, seteando variables!");

        }
        else
        {
            _Inventory[item].AddStack();
            print("Stack obtenido!");
        }



    }

    /// <summary>
    /// funcion para restar un stack de un item del inventario
    /// RECORDATORIO: USAR ESTA FUNCION CADA VEZ QUE SE QUIERA RESTAR UN ITEM
    /// INCLUSO REMOVER, NO USAR REMOVEITEM A MENOS QUE ESTES SEGURO
    /// </summary>
    /// <param name="item">Item a restar un stack</param>
    public void LoseItemStack(ItemDefinitionSO item)
    {
        /*if (!_Inventory.Contains(item))
        {
            Debug.LogError("[Visceral Error] Inventory manager: item removido"
                 + "no existe en el inventario: " + item.ItemDefinitionSO.ItemName + " ID: " + item.ItemDefinitionSO.ItemID);
            return;
        }
         _Inventory.Find(x=>x== item).RemoveStack();
        item.OnDrop();
        */
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
        /*if (!_Inventory.Contains(item))
        {
            Debug.LogError("[Visceral Error] Inventory manager: item removido"
            + "no existe en el inventario: " + item.name + " ID: " + item.ItemDefinitionSO.ItemID);
            return;
        }
        _Inventory.Remove(item);

        */
    }
}
