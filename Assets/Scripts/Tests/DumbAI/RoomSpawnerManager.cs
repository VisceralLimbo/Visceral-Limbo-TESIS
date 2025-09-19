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
    [SerializeField] private DialogueData _finishCombatDialogue;
    [SerializeField] Transform _spawnPointReward;
    [SerializeField] SoundData _spawnSound,_rewardSound,_FinishWaveSound;
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
    [SerializeField] private int _BloodEchoesReward;

    [SerializeField] private List<FireballTrap> trapsInThisRoom;

    [Header("Lights Setup")]
    [SerializeField] private List<Light> combatLights; 
    [SerializeField] private Color lastWaveColor = Color.red; 
    [SerializeField] private Color endWaveColor = Color.blue;
    [SerializeField] private bool lightsChanged = false;

    [SerializeField] private Light extraLight;


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

        lightsChanged = false;

        foreach (var trap in trapsInThisRoom)
        {
            trap.ActivateTrap();
        }

        //reseteamos el TEXTO de puntuacion, el valor de la misma sigue siendo igual
        ScoreManager.Instance.ResetScoreText();

    }

    public void NotifyMinionDeath()
    {
        if (StopThisManager) return;

        MinionsAlive = Spawners.Any(x => x.HasMinion);
        SpawnersSpent = Spawners.All(x => x.IsSpent);

        // no hay minions vivos pero todavía quedan spawners = spawnear otra oleada
        if (!MinionsAlive && !SpawnersSpent)
        {
            bool nextWaveWillBeLast = Spawners.All(s => s.RemainingSpawns <= 1);


            if (nextWaveWillBeLast && !lightsChanged)
            {
                ChangeLightsToLastWave();
                lightsChanged = true;
            }

            MusicManager.Instance?.PlayCombatMusic();

            SlowMotion.Stop(1f, 0.35f, true);
            FindObjectOfType<SlowMotionController>()?.ApplyEffect(1f, 0.6f);

            StartCoroutine(SpawnCoroutine());

            foreach (var item in roomTriggers)
            {
                if (item == null) continue;
                item.SetSolidState(false);
                item.SetCombatState(true);
            }
        }

        // no hay minions y no quedan spawners = combate terminado
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

            //audioSource.Play();
            SoundManager.Instance.CreateSound().WithSoundData(_FinishWaveSound).play();


            ChangeLightsToEndWave();

            if (ScoreManager.Instance.GetPlayerScore >= (_StartingCombatScore + _AddExtraRequiredScore))
            {
                if(_reward != null)
                {
                    Instantiate(_reward, _spawnPointReward.transform.position, _spawnPointReward.transform.rotation);
                }

                BloodEchoesManager.AddBloodEchoes(_BloodEchoesReward);

                SoundManager.Instance.CreateSound()
                    .WithSoundData(_rewardSound)
                    .WithRandomPitch(true)
                    .WithPosition(_spawnPointReward.position)
                    .WithSpatialBlend(1).play();
            }
            DialogueManager.instance.StartDialogue(_finishCombatDialogue);

            MusicManager.Instance?.PlayExplorationMusic();
            OnCombatEnded?.Invoke();
        }

    }


    private void ChangeLightsToLastWave()
    {
        SetLightsColor(lastWaveColor);

    }


    private void ChangeLightsToEndWave()
    {
        SetLightsColor(endWaveColor);

        if (extraLight != null)
            extraLight.gameObject.SetActive(true);
    }
    //script hecho por Patricio Malvasio Maddalena
    // uso de Any / all (Grupo 3)

    private void SetLightsColor(Color c)
    {
        if (combatLights == null) return;
        foreach (var light in combatLights)
        {
            if (light != null)
                light.color = c;
        }
    }

    IEnumerator SpawnCoroutine()
    {
        foreach (var item in Spawners)
        {
         
            item.SpawnEnemy();

            SoundManager.Instance.CreateSound()
                .WithSoundData(_spawnSound)
                .WithPosition(item.transform.position)
                .WithRandomPitch(true)
                .WithSpatialBlend(1)
                .play();

            if (_ShouldOffsetSpawnTime)
            {
                yield return new WaitForSeconds(_OffsetSpawnTime);
            }
        }
    }

}
