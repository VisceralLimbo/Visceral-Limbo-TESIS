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
    [SerializeField] GameObject _reward;
    [SerializeField] Transform _spawnPointReward;

    [SerializeField] bool MinionsAlive, SpawnersSpent, StopThisManager;

   
    public event System.Action OnCombatEnded;

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

        
        if (!MinionsAlive && !SpawnersSpent)
        {
            
            MusicManager.Instance?.PlayCombatMusic();

            foreach (var item in Spawners)
            {
                item.SetPlayerIndex(playerContext);
                item.SpawnEnemy();
            }

            foreach (var item in roomTriggers)
            {
                if (item == null) continue;
                item.SetSolidState(true); 
            }
        }

        
        if (!MinionsAlive && SpawnersSpent)
        {
            foreach (var item in roomTriggers)
            {
                item.SetSolidState(false); 
            }

            StopThisManager = true;
            audioSource.Play();
            if(_reward != null)
            {
                Instantiate(_reward, _spawnPointReward.transform.position, Quaternion.identity);
            }
            DialogueManager.instance.StartDialogue(_finishCombatDialogue);

            
            MusicManager.Instance?.PlayExplorationMusic();

            OnCombatEnded?.Invoke();
        }
    }

    //script hecho por Patricio Malvasio Maddalena
    // uso de Any / all (Grupo 3)

}
