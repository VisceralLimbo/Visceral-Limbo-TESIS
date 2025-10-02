using System.Collections;
using UnityEngine;

public class HealthFullscreenEffect : MonoBehaviour
{
    public static HealthFullscreenEffect Instance;

    [Header("efecto de curacion")]
    [SerializeField] private Material fullscreenMat;
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("efecto de velocidad)")]
    [SerializeField] private Material windEffectMat;
    private const string SHADER_ALPHA_PROPERTY = "_Alpha"; //juego con el alpha apagano y prendiendo para movimiento

    private Coroutine currentRoutine;

    // func para q la llame speedlogic, juego con el valor del _Alpha 0 apagdo 1 max
    public void SetWindAlpha(float alpha)
    {
        if (windEffectMat != null && windEffectMat.HasProperty(SHADER_ALPHA_PROPERTY))
        {
            windEffectMat.SetFloat(SHADER_ALPHA_PROPERTY, alpha);
        }
    }

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

