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

        float total = 0;
        foreach (var item in lootItems)
            total += item.dropChance;

        float roll = Random.value * total;
        float cumulative = 0;

        foreach (var item in lootItems)
        {
            cumulative += item.dropChance;
            if (roll <= cumulative)
                return item;
        }

        return null;
    }
}
