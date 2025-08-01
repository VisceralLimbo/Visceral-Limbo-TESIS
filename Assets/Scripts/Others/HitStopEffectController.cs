using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HitStopEffectController : MonoBehaviour
{
    public Volume volume;

    private ChromaticAberration chromatic;

    private void Awake()
    {
        volume.profile.TryGet(out chromatic);

    }

    public void ApplyEffect(float chromaIntensity, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(EffectRoutine(chromaIntensity, duration));
    }

    private System.Collections.IEnumerator EffectRoutine(float chromaIntensity, float duration)
    {
        if (chromatic != null)
            chromatic.intensity.value = chromaIntensity;


        yield return new WaitForSecondsRealtime(duration);

        if (chromatic != null)
            chromatic.intensity.value = 0f;

    }
}

