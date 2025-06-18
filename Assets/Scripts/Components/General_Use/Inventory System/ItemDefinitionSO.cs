using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(menuName = "Visceral_Limbo/Gameplay Data/Items/ItemDefinitionSO")]
public class ItemDefinitionSO : ScriptableObject
{
    [SerializeField] public string ItemID;
    [SerializeField] public string ItemName;
    [SerializeField] protected int _InitialItemStack;
    public int ItemStack;

    [SerializeField] public Sprite ItemSprite;

    [SerializeField] public GameObject ItemDataPrefab;

    /// <summary>
    /// añadimos un stack de item, RECORDATORIO:
    /// esta funcion tambien añade item si no estan en el inventario
    /// </summary>

    public void Awake()
    {
        ItemStack = 0;
    }

    private void OnEnable()
    {
        ItemStack = _InitialItemStack;
    }

}
