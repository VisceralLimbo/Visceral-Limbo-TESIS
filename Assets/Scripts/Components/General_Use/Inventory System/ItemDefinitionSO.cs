using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(menuName = "Visceral_Limbo/Gameplay Data/Items/ItemDefinitionSO")]
public class ItemDefinitionSO : ScriptableObject
{
    [SerializeField] public string ItemID;
    [SerializeField] public string ItemName;
    [SerializeField] public float ItemStacks;

    [SerializeField] public Sprite ItemSprite;

    [SerializeField] public GameObject ItemPrefab;

    [SerializeField] public ItemLogic Logic; 

    /// <summary>
    /// añadimos un stack de item, RECORDATORIO:
    /// esta funcion tambien añade item si no estan en el inventario
    /// </summary>
    public void AddStack()
    {
        ItemStacks++;
    }

    /// <summary>
    /// remover un stack de item.
    /// RECORDATORIO: esta funcion tambien remueve el item final si el
    /// stack es menor o igual a 0
    /// </summary>
    public void RemoveStack()
    {
        ItemStacks--;
        if(ItemStacks <= 0)
        {
            Logic.Unregister();
        }
    }
}
