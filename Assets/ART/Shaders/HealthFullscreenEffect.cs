using System.Collections;
using UnityEngine;

public class HealthFullscreenEffect : MonoBehaviour
{
    public static HealthFullscreenEffect Instance;

    [Header("Health Effect")]
    [SerializeField] private Material fullscreenMat;
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Velocity Effect")]
    [SerializeField] private Material windEffectMat;
    private const string SHADER_ALPHA_PROPERTY = "_Alpha"; //juego con el alpha apagano y prendiendo para movimiento

    [Header("Darkness Effect")]
    [SerializeField] private Material darknessMat;
    private Coroutine darknessRoutine;

    [Header("Black and White Effect")]
    [SerializeField] private Material blackAndWhiteMat;
    private Coroutine blackAndWhiteRoutine;

    [Header("Freeze Effect")]
    [SerializeField] private Material congelationMat;

    private Coroutine freezeRoutine;

    private Coroutine currentRoutine;

    // func para q la llame speedlogic, juego con el valor del _Alpha 0 apagdo 1 max
    public void SetWindAlpha(float alpha)
    {
        if (windEffectMat != null && windEffectMat.HasProperty(SHADER_ALPHA_PROPERTY))
        {
            windEffectMat.SetFloat(SHADER_ALPHA_PROPERTY, alpha);
        }
    }

    public void FadeDarkness(float targetAlpha, float fadeSpeed)
    {
        if (darknessRoutine != null)
            StopCoroutine(darknessRoutine);

        darknessRoutine = StartCoroutine(FadeDarknessRoutine(targetAlpha, fadeSpeed));
    }

    public void FadeBlackAndWhite(float targetIntensity, float fadeSpeed)
    {
        if (blackAndWhiteRoutine != null)
            StopCoroutine(blackAndWhiteRoutine);

        blackAndWhiteRoutine = StartCoroutine(
            FadeBlackAndWhiteRoutine(targetIntensity, fadeSpeed));
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);


        fullscreenMat.SetFloat("_VignetteIntensity", 0f);
        darknessMat.SetFloat("_Alpha", 0f);
        blackAndWhiteMat.SetFloat("_Intensity", 0f);
        congelationMat.SetFloat("_FreezeAmount", 0f);
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

    private IEnumerator FadeDarknessRoutine(float targetAlpha, float fadeSpeed)
    {
        float currentAlpha = darknessMat.GetFloat("_Alpha");

        while (Mathf.Abs(currentAlpha - targetAlpha) > 0.01f)
        {
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

            darknessMat.SetFloat("_Alpha", currentAlpha);

            yield return null;
        }

        darknessMat.SetFloat("_Alpha", targetAlpha);

        darknessRoutine = null;
    }

    private IEnumerator FadeBlackAndWhiteRoutine(float targetIntensity, float fadeSpeed)
    {
        float currentIntensity = blackAndWhiteMat.GetFloat("_Intensity");

        while (Mathf.Abs(currentIntensity - targetIntensity) > 0.01f)
        {
            currentIntensity = Mathf.Lerp(
                currentIntensity,
                targetIntensity,
                Time.deltaTime * fadeSpeed);

            blackAndWhiteMat.SetFloat("_Intensity", currentIntensity);

            yield return null;
        }

        blackAndWhiteMat.SetFloat("_Intensity", targetIntensity);

        blackAndWhiteRoutine = null;
    }

    public void FadeFreeze(float targetAmount, float speed)
    {
        if (freezeRoutine != null)
            StopCoroutine(freezeRoutine);

        freezeRoutine = StartCoroutine(
            FadeFreezeRoutine(targetAmount, speed));
    }

    private IEnumerator FadeFreezeRoutine(float targetAmount, float speed)
    {
        float current = congelationMat.GetFloat("_FreezeAmount");

        while (Mathf.Abs(current - targetAmount) > 0.01f)
        {
            current = Mathf.Lerp(current, targetAmount, Time.deltaTime * speed);

            congelationMat.SetFloat("_FreezeAmount", current);

            yield return null;
        }

        congelationMat.SetFloat("_FreezeAmount", targetAmount);

        freezeRoutine = null;
    }

    public void ResetAllEffects()
    {
        // darkness
        if (darknessRoutine != null)
            StopCoroutine(darknessRoutine);

        darknessMat.SetFloat("_Alpha", 0f);

        // black and white
        if (blackAndWhiteRoutine != null)
            StopCoroutine(blackAndWhiteRoutine);

        blackAndWhiteMat.SetFloat("_Intensity", 0f);

        // freeze
        if (freezeRoutine != null)
            StopCoroutine(freezeRoutine);

        congelationMat.SetFloat("_FreezeAmount", 0f);

        // wind
        if (windEffectMat != null)
        {
            windEffectMat.SetFloat("_Alpha", 0f);
        }

        // damage vignette
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        fullscreenMat.SetFloat("_VignetteIntensity", 0f);
    }
}

