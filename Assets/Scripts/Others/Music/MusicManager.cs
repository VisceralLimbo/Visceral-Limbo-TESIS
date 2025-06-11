using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio Sources")]
    public AudioSource sourceA;
    public AudioSource sourceB;

    [Header("Audio Clips")]
    public AudioClip explorationMusic;
    public AudioClip combatMusic;

    [Header("Settings")]
    public float fadeDuration = 2f;

    private AudioSource currentSource;
    private AudioSource nextSource;
    private bool isFading = false;

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

        currentSource.clip = explorationMusic;
        currentSource.volume = 0.244f;
        currentSource.Play();
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

    private System.Collections.IEnumerator FadeToClip(AudioClip newClip)
    {
        isFading = true;

        nextSource.clip = newClip;
        nextSource.volume = 0f;
        nextSource.Play();

        float time = 0f;

        while (time < fadeDuration)
        {
            float t = time / fadeDuration;
            currentSource.volume = Mathf.Lerp(0.244f, 0f, t);
            nextSource.volume = Mathf.Lerp(0f, 0.244f, t);
            time += Time.deltaTime;
            yield return null;
        }

        currentSource.Stop();
        currentSource.volume = 0.244f;

        
        var temp = currentSource;
        currentSource = nextSource;
        nextSource = temp;

        isFading = false;
    }
}

