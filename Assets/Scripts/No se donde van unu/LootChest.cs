using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLootTable", menuName = "Loot/Table")]
public class LootChest : ScriptableObject
{
    //array con los items que quieren que esten en el cofre
    public LootItem[] lootItems;

    //random dependiendo la chance q le dieron a cada item
    public LootItem GetRandomLoot()
    {
        if (lootItems == null || lootItems.Length == 0)
            return null;

        List<LootItem> validItems = new List<LootItem>();
        float total = 0f;

        foreach (LootItem item in lootItems)
        {
            if (item == null || item.prefab == null)
                continue;

            if (item.dropChance <= 0f ||
                float.IsNaN(item.dropChance) ||
                float.IsInfinity(item.dropChance))
                continue;

            validItems.Add(item);
            total += item.dropChance;
        }

        if (validItems.Count == 0 || total <= 0f)
            return null;

        float roll = Random.Range(0f, total);
        float cumulative = 0f;

        foreach (LootItem item in validItems)
        {
            cumulative += item.dropChance;

            if (roll < cumulative)
                return item;
        }

        return validItems[validItems.Count - 1];
    }
}
