using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InventoryManager : MonoBehaviour
{
    public List<ItemLogic> Inventory = new List<ItemLogic>();

    [SerializeField] private Dictionary<ItemDefinitionSO,ItemLogic> _Inventory = new Dictionary<ItemDefinitionSO, ItemLogic>();

    [SerializeField] private List<I_ItemActiveItem> _ActiveItems = new List<I_ItemActiveItem>();

    [SerializeField] private List<I_OnHitItem> _OnHitItems= new List<I_OnHitItem>();

    [SerializeField] StatsManager _StatsManager;

    [SerializeField] PlayerContext _Context;

    public UnityAction<ItemDefinitionSO> ItemPickUp;

    private void Start()
    {
        // Si venimos de un nivel anterior y hay items guardados...
        if (GameManager.Instance != null && GameManager.Instance.savedInventory.Count > 0)
        {
            foreach (var savedItem in GameManager.Instance.savedInventory)
            {
                // Añadimos el item tantas veces como stacks teniamos
                for (int i = 0; i < savedItem.stacks; i++)
                {
                    AddItemStack(savedItem.definition);
                }
            }
            GameManager.Instance.savedInventory.Clear(); 
        }
    }

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

            if(NewComponentItem is I_ItemActiveItem ActiveItem && ActiveItem != null)
            {
                print("inventory: thats an active item!");
                _ActiveItems.Add(ActiveItem);
            }
            
            if(NewComponentItem is I_OnHitItem OnHitItem && OnHitItem != null)
            {
                _OnHitItems.Add(OnHitItem);
            }

        }
        else
        {
            _Inventory[item].AddStack();
            print("Stack obtenido!");
        }

        ItemPickUp?.Invoke(item);

    }
    public int GetItemStack(ItemDefinitionSO Definition)
    {
        if (_Inventory.ContainsKey(Definition))
        {
            return _Inventory[Definition].Stacks;
        }
        else
        {
            return 1;
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



    public void Update()
    {
        if (_ActiveItems.Count == 0)
        {
            return;
        }

        foreach (var item in _ActiveItems)
        {
            item.UpdateActiveItem(this, _StatsManager);
        }
    }


    public Dictionary<ItemDefinitionSO, ItemLogic> GetInventoryItems()
    {
        return _Inventory;

    }


    public void ProcOnHitEffects(PlayerContext Context,DamageScore DMS, Health_Component VictimHP)
    {
        if(_OnHitItems.Count > 0)
        {
            foreach(I_OnHitItem item in _OnHitItems)
            {
                item.OnProcEffect(Context,DMS,VictimHP);
            }
        }
    }
}
