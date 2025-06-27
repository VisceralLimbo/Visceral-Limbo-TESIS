using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoomSpawnerManager : MonoBehaviour
{
    public PlayerContext playerContext { get; private set; }
    [Header("References")]
    [SerializeField] private Spawner[] Spawners;
    [SerializeField] private List<RoomEnterTrigger> roomTriggers;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private DialogueData _finishCombatDialogue;
    [SerializeField] GameObject _reward;
    [SerializeField] Transform _spawnPointReward;


    [Space]
    [Header("Variables")]

    [SerializeField] bool MinionsAlive, SpawnersSpent, StopThisManager;
    [Space]
    [SerializeField] private float _StartingCombatScore;
    [SerializeField] private float _AddExtraRequiredScore;


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
        _StartingCombatScore = ScoreManager.Instance.GetPlayerScore;
    }

    public void AssignRoomEnters(RoomEnterTrigger trigger)
    {
        roomTriggers.Add(trigger);
    }

    //evento de que murio un minion
    public void NotifyMinionDeath()
    {
        if (StopThisManager) return; // manager apagado

        MinionsAlive = Spawners.Any(x => x.HasMinion); //chequeamos si los spawners tienen minions vivos
        SpawnersSpent = Spawners.All(x => x.IsSpent);

        if (!MinionsAlive && !SpawnersSpent)
        {
            //no hay minions vivos pero tampoco hay spawners vacios =
            //spawnear otra oleada
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

        // no hay minions activos y los spawners estan vacios
        // finalizar combate
        if (!MinionsAlive && SpawnersSpent)
        {
            foreach (var item in roomTriggers)
            {
                item.SetSolidState(false); 
            }

            StopThisManager = true;
            audioSource.Play();

            // la sala tiene reward y la puntuacion final del jugador
            // es mayor a la requerida X sala
            if(_reward != null && ScoreManager.Instance.GetPlayerScore >= (_StartingCombatScore + _AddExtraRequiredScore))
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
