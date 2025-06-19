using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private ItemDefinitionSO _ItemSpawn;
    


    void Start()
    {
        var ItemGO = Instantiate(_ItemSpawn.ItemDataPrefab, this.transform.position, Quaternion.identity);
        var Logic = ItemGO.GetComponent<ItemLogic>();
        Logic.Initialize(_ItemSpawn);
        
        

    }

  
}
