using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Loot/Item")]
public class LootItem : ScriptableObject
{
    [Header("info item")]
    public string itemName;
    public GameObject prefab;

    [Header("loot stats")]
    public ItemRarity rarity;
    [Range(0f, 1f)] public float dropChance; // probabilidad xd

    [TextArea] public string description; //descripcion del item intente seguir la logica de lo q hizo pato mas o menos para q no hayan dudas
                                          // si otra persona necesita ver algo.
}
