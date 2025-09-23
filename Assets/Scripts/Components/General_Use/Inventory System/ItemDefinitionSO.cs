using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(menuName = "Visceral_Limbo/Gameplay Data/Items/ItemDefinitionSO")]
public class ItemDefinitionSO : ScriptableObject
{
    [SerializeField] public string ItemID;
    [SerializeField] public string ItemName;
    //descripcion que quieren que se muestre ingame
    [SerializeField] public string ItemDescription;
    [SerializeField] protected int _InitialItemStack;

    [Header("item stack bufos")]
    [SerializeField] public string buff; // ej "velocidad", "vida", "danio" (no uso la enie por miedo me rompio un proyecto una vez :c)
    [SerializeField] public float buffvalueperstack; // valor por stack (ej vida 1stack = 20, velocidad 1stack = 2,etc)

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
