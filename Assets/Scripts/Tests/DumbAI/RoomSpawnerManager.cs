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
    [SerializeField] Transform _spawnPointReward;
    [SerializeField] SoundData _spawnSound,_rewardSound;
    [SerializeField] private Animator _animatorObjective;
    [SerializeField] private GameObject _reward;

    [Space]
    [Header("Variables")]
    [SerializeField] bool MinionsAlive;
    [SerializeField] bool SpawnersSpent;
    [SerializeField] bool StopThisManager;
    [SerializeField] bool _ShouldOffsetSpawnTime;
    [SerializeField] float _OffsetSpawnTime;
    [SerializeField] private float _StartingCombatScore;
    [SerializeField] private float _AddExtraRequiredScore;

    [SerializeField] private List<FireballTrap> trapsInThisRoom;

    public event System.Action OnCombatEnded;

    private void Start()
    {
        if(Spawners.Length <= 0)
        {
            Spawners = GetComponentsInChildren<Spawner>();
        }

        if (trapsInThisRoom.Count == 0)
        {
            trapsInThisRoom = GetComponentsInChildren<FireballTrap>().ToList();
        }
    }


    public void AssignPlayerContext(PlayerContext playerCont)
    {
        playerContext = playerCont;
        _StartingCombatScore = ScoreManager.Instance.GetPlayerScore;

        ScoreManager.Instance.RegisterMilestone(_StartingCombatScore + _AddExtraRequiredScore);
    }

    public void AssignRoomEnters(RoomEnterTrigger trigger)
    {
        roomTriggers.Add(trigger);
    }


    public void StartRoomCombat()
    {
        _animatorObjective.SetTrigger("StartCombat");

        foreach (var trap in trapsInThisRoom)
        {
            trap.ActivateTrap();
        }

        //reseteamos el TEXTO de puntuacion, el valor de la misma sigue siendo igual
        ScoreManager.Instance.ResetScoreText();

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

            SlowMotion.Stop(1f, 0.35f, true); //el segundo valor cambia el pitch de la musica y sonidos
            FindObjectOfType<SlowMotionController>()?.ApplyEffect(1f, 0.6f); //intensidad es el primer numero, el otro es la duracion

          

            //comenzar la coroutina de spawneo
            StartCoroutine(SpawnCoroutine());

            foreach (var item in roomTriggers)
            {
                if (item == null) continue;
                item.SetSolidState(false);
                item.SetCombatState(true);
            }
        }

        // no hay minions activos y los spawners estan vacios
        // finalizar combate
        if (!MinionsAlive && SpawnersSpent)
        {
            foreach (var item in roomTriggers)
            {
               


                item.SetSolidState(true);
                item.SetCombatState(false);


            }

            StopThisManager = true;
            foreach (var trap in trapsInThisRoom)
            {
                trap.DeactivateTrap();
            }
            audioSource.Play();

            _animatorObjective.SetTrigger("EndCombat");

            // la sala tiene reward y la puntuacion final del jugador
            // es mayor a la requerida X sala


            if (ScoreManager.Instance.GetPlayerScore >= (_StartingCombatScore + _AddExtraRequiredScore))
            {
                Instantiate(_reward, _spawnPointReward.transform.position, _spawnPointReward.transform.rotation);
                BloodEchoesManager.AddBloodEchoes(500);
                print(BloodEchoesManager.BloodEchoes);

                SoundManager.Instance.CreateSound().
                    WithSoundData(_rewardSound).
                    WithRandomPitch(true).
                    WithPosition(_spawnPointReward.position).
                    WithSpatialBlend(1).play();
            }
            DialogueManager.instance.StartDialogue(_finishCombatDialogue);

            
            MusicManager.Instance?.PlayExplorationMusic();

            OnCombatEnded?.Invoke();
        }
    }

    //script hecho por Patricio Malvasio Maddalena
    // uso de Any / all (Grupo 3)


    //coroutina de spawn
    IEnumerator SpawnCoroutine()
    {
        foreach(var item in Spawners) 
        {
            item.SetPlayerIndex(playerContext);

            item.SpawnEnemy();

            SoundManager.Instance.CreateSound().
                WithSoundData(_spawnSound).
                WithPosition(item.transform.position).
                WithRandomPitch(true).
                WithSpatialBlend(1)
               .play();
               

            if (_ShouldOffsetSpawnTime)
            {
                yield return new WaitForSeconds(_OffsetSpawnTime);
            }

        }
    }
}
