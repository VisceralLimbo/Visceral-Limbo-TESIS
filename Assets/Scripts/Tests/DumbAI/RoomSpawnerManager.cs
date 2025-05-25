using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoomSpawnerManager : MonoBehaviour
{
   public PlayerContext playerContext { get; private set; }
    [SerializeField] private Spawner[] Spawners;
    [SerializeField] private List<RoomEnterTrigger> roomTriggers;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private DialogueData _finishCombatDialogue;

    [SerializeField] bool MinionsAlive, SpawnersSpent, StopThisManager;

    private void Start()
    {
        if(Spawners.Length <= 0)
        {
            Spawners = GetComponentsInChildren<Spawner>();
        }
    }


    public void AssignPlayerContext(PlayerContext playerCont)
    {
        playerContext = playerCont;
    }

    public void AssignRoomEnters(RoomEnterTrigger trigger)
    {
        roomTriggers.Add(trigger);
    }

    public void NotifyMinionDeath()
    {
        if (StopThisManager) return;

        MinionsAlive = Spawners.Any(x => x.HasMinion);
        SpawnersSpent = Spawners.All(x => x.IsSpent);
        if(!MinionsAlive && !SpawnersSpent)
        {
            foreach(var Item in Spawners)
            {
                Item.SetPlayerIndex(playerContext);
                Item.SpawnEnemy();
            }
            foreach(var item in roomTriggers)
            {
                if (item == null) continue;
                item?.SetSolidState(true);
            }
        }

        if(!MinionsAlive && SpawnersSpent)
        {
            
            foreach (var item in roomTriggers)
            {
                item.SetSolidState(false);
            }
            StopThisManager = true;
            audioSource.Play();
            DialogueManager.instance.StartDialogue(_finishCombatDialogue);
        }


    }

    //script hecho por Patricio Malvasio Maddalena
    // uso de Any / all (Grupo 3)

}
