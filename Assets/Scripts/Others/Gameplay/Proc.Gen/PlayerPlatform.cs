using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlatform : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] DungeonGenerator Dungeon;
    void Start()
    {
        Dungeon.OnSuccessfulGeneration += gameLoaded;
    }


    private void gameLoaded()
    {
        Dungeon.OnSuccessfulGeneration -= gameLoaded;
        Destroy(this.gameObject);
    }
}
