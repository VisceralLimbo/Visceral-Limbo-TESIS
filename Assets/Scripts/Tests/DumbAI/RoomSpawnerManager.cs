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
    public bool ManagerStopped { get { return StopThisManager; } }
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

    [Space]
    [Header("Environmental Effect")]
    [SerializeField] private bool _canHaveEnvironmentalEffect = true; // si no queremos q la sala tenga efectos desactivamos
    [SerializeField] private EnvironmentalEffect _currentEffect = EnvironmentalEffect.None;
    [SerializeField] private float _lowVisibilityLightIntensity = 0.1f; // intensidad luz
    private float _originalExtraLightIntensity;
    private List<float> _originalCombatLightIntensities = new List<float>(); //lista con la instensidad orignal de las lcues
    public ParticleSystem blackFog;

    public event System.Action OnCombatEnded;

    [Header("Particles Setup")]
    [SerializeField] private List<ParticleSystem> fireParticles;

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

        // guarda la intensidad de la lzu
        if (extraLight != null)
        {
            _originalExtraLightIntensity = extraLight.intensity;
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

        // cambio del enviroment
        if (_canHaveEnvironmentalEffect)
        {
            ChooseRandomEffect();
            ApplyEnvironmentalEffect(_currentEffect);
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
            if(roomTriggers.Count > 0)
            {
                foreach (var item in roomTriggers)
                {
                    item.SetSolidState(true);
                    item.SetCombatState(false);
                }

            }

            StopThisManager = true;

            //revierto el cambio del enviroment
            RevertEnvironmentalEffect(_currentEffect);

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
        SetLightsColor(lastWaveColor, 2f); // 2 segundos de transición
    }


    private void ChangeLightsToEndWave()
    {
        SetLightsColor(endWaveColor, 2f);

        if (extraLight != null)
            extraLight.gameObject.SetActive(true);
    }


    //script hecho por Patricio Malvasio Maddalena
    // uso de Any / all (Grupo 3)

    private void SetLightsColor(Color targetColor, float duration = 2f)
    {
        if (combatLights == null || combatLights.Count == 0) return;

        StopAllCoroutines(); // paramos cualquier transición previa
        StartCoroutine(TransitionLightsColor(targetColor, duration));
    }

    private IEnumerator TransitionLightsColor(Color targetColor, float duration)
    {
        float elapsed = 0f;
        List<Color> initialColors = new List<Color>();

        foreach (var light in combatLights)
        {
            if (light != null)
                initialColors.Add(light.color);
            else
                initialColors.Add(Color.white);
        }

        List<Color> initialParticleColors = new List<Color>();
        foreach (var ps in fireParticles)
        {
            if (ps != null)
                initialParticleColors.Add(ps.main.startColor.color);
            else
                initialParticleColors.Add(Color.white);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Luces
            for (int i = 0; i < combatLights.Count; i++)
            {
                if (combatLights[i] != null)
                    combatLights[i].color = Color.Lerp(initialColors[i], targetColor, t);
            }

            // Partículas
            for (int i = 0; i < fireParticles.Count; i++)
            {
                if (fireParticles[i] != null)
                {
                    var main = fireParticles[i].main;
                    main.startColor = Color.Lerp(initialParticleColors[i], targetColor, t);
                }
            }

            yield return null;
        }

        // asegurar que terminan en el color correcto
        for (int i = 0; i < combatLights.Count; i++)
        {
            if (combatLights[i] != null)
                combatLights[i].color = targetColor;
        }

        for (int i = 0; i < fireParticles.Count; i++)
        {
            if (fireParticles[i] != null)
            {
                var main = fireParticles[i].main;
                main.startColor = targetColor;
            }
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

    // aca irian todos los efectos que queremos intente el frozen pero pincho xd
    public enum EnvironmentalEffect
    {
        None, // normal
        //Frozen, // congelado (hay q ver, creo q necesitamos un timescale manager porque se usa en 30mil lados)
        LowVisibility, // poca visibilidad
    }

    //random para agarrar algun efecto
    private void ChooseRandomEffect()
    {
        // agarro los valores del enum
        var effects = System.Enum.GetValues(typeof(EnvironmentalEffect)).Cast<EnvironmentalEffect>().Where(e => e != EnvironmentalEffect.None).ToArray();

        //menos return
        if (effects.Length == 0)
        {
            _currentEffect = EnvironmentalEffect.None;
            return;
        }

        // 50% de probabilidad de q tenga efecto o no
        float probability = 0.5f;
        if (Random.value < probability)
        {
            // elige un efecto random
            _currentEffect = effects[Random.Range(0, effects.Length)];
            Debug.Log($"Efecto ambiental activado en la sala: {_currentEffect}");
        }
        else
        {
            _currentEffect = EnvironmentalEffect.None;
            Debug.Log("La sala no tiene efecto ambiental.");
        }
    }

    private void ApplyEnvironmentalEffect(EnvironmentalEffect effect)
    {
        Debug.Log($"Aplicando efecto ambiental: {effect}");

        switch (effect)
        {
            case EnvironmentalEffect.LowVisibility:
                // guardo y aplico las luces
                _originalCombatLightIntensities.Clear();
                foreach (var light in combatLights)
                {
                    if (light != null)
                    {
                        _originalCombatLightIntensities.Add(light.intensity); // guardo
                        light.intensity = _lowVisibilityLightIntensity;       // aplico la baja intensidad
                    }
                    if (blackFog !=null)
                    {
                        blackFog.Play();
                    }
                }
                // aplico a extralight
                if (extraLight != null)
                {
                    extraLight.intensity = _lowVisibilityLightIntensity;
                    extraLight.gameObject.SetActive(true);
                }
                break;

            case EnvironmentalEffect.None:
            default:
                break;
        }
    }

    private void RevertEnvironmentalEffect(EnvironmentalEffect effect)
    {
        switch (effect)
        {
            case EnvironmentalEffect.LowVisibility:
                // restauro
                for (int i = 0; i < combatLights.Count; i++)
                {
                    if (combatLights[i] != null && i < _originalCombatLightIntensities.Count)
                    {
                        combatLights[i].intensity = _originalCombatLightIntensities[i];
                    }
                    if (blackFog != null)
                    {
                        blackFog.Stop();
                    }
                }
                // restuaro extralight
                if (extraLight != null)
                {
                    extraLight.intensity = _originalExtraLightIntensity;
                }
                break;

            case EnvironmentalEffect.None:
            default:
                break;
        }
        Debug.Log($"Reverting effect: {effect} - Called unexpectedly!"); // test
        _currentEffect = EnvironmentalEffect.None;
    }
}
