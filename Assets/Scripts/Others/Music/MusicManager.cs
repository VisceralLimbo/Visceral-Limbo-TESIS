using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio Sources")]
    public AudioSource sourceA;
    public AudioSource sourceB;

    [Header("Audio Clips")]
    public AudioClip explorationMusic;
    public AudioClip combatMusic;
    public AudioClip menuMusic; // clip de la musica del menu

    [Header("Settings")]
    public float fadeDuration = 2f;

    private AudioSource currentSource;
    private AudioSource nextSource;
    private bool isFading = false;

    // escena menu
    private const string MENU_SCENE_NAME = "MainMenu";

    private void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeSources()
    {
        sourceA.loop = true;
        sourceB.loop = true;

        currentSource = sourceA;
        nextSource = sourceB;

        //currentSource.clip = explorationMusic;
        //currentSource.volume = 0.244f;
        //currentSource.Play();
    }

    public void PlayExplorationMusic()
    {
        if (currentSource.clip == explorationMusic || isFading)
            return;

        StartCoroutine(FadeToClip(explorationMusic));
    }

    public void PlayCombatMusic()
    {
        if (currentSource.clip == combatMusic || isFading)
            return;

        StartCoroutine(FadeToClip(combatMusic));
    }

    private IEnumerator FadeToClip(AudioClip newClip)
    {
        isFading = true;

        nextSource.clip = newClip;
        nextSource.volume = 0f;
        nextSource.Play();

        float time = 0f;

        while (time < fadeDuration)
        {
            float t = time / fadeDuration;
            // lerpo para el volumen
            currentSource.volume = Mathf.Lerp(0.244f, 0f, t);
            nextSource.volume = Mathf.Lerp(0f, 0.244f, t);
            time += Time.deltaTime;
            yield return null;
        }

        currentSource.Stop();
        currentSource.volume = 0.244f; // volumen q habia seteado

        var temp = currentSource;
        currentSource = nextSource;
        nextSource = temp;

        isFading = false;
    }

    private void OnEnable()
    {
        // suscribo a vento carga de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // cancelo
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // cambio de escena
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // paro corrutinas y la musica de las otras escenas porque esto tiene un dontdestroyonload
        if (isFading)
        {
            StopAllCoroutines();
            isFading = false;
        }

        // paro los dos audios
        sourceA.Stop();
        sourceB.Stop();

        // si estoy en menu suena menu music si no la de exploracion
        if (scene.name == MENU_SCENE_NAME)
        {
            PlayImmediate(menuMusic);
        }
        else
        {
            PlayImmediate(explorationMusic);
        }
    }

    // sin fade para cuando cambia la escena, si no queda raro xd
    void PlayImmediate(AudioClip clip)
    {
        if (clip == null) return;

        currentSource.clip = clip;
        currentSource.volume = 0.244f;
        currentSource.Play();

        nextSource.Stop();
        nextSource.clip = null;
    }
}

