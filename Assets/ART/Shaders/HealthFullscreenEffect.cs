using System.Collections;
using UnityEngine;

public class HealthFullscreenEffect : MonoBehaviour
{
    public static HealthFullscreenEffect Instance;

    [SerializeField] private Material fullscreenMat;
    [SerializeField] private float fadeDuration = 1.5f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        
        fullscreenMat.SetFloat("_VignetteIntensity", 0f);
    }

  

    public void PlayEffect(float intensity)
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(PlayAndFade(intensity));
    }

    private IEnumerator PlayAndFade(float startIntensity)
    {
        fullscreenMat.SetFloat("_VignetteIntensity", startIntensity);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float newValue = Mathf.Lerp(startIntensity, 0f, t / fadeDuration);
            fullscreenMat.SetFloat("_VignetteIntensity", newValue);
            yield return null;
        }

        fullscreenMat.SetFloat("_VignetteIntensity", 0f);
        currentRoutine = null;
    }
}

