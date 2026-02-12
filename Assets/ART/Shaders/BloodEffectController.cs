using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class BloodEffectController : MonoBehaviour
{
    [Header("Shader Settings")]
    [SerializeField] private Material bloodMaterial;
    [SerializeField] private string voronoiProperty = "_VoronoiPower";
    [SerializeField] private float transitionDuration = 1f;

    private Coroutine transitionCoroutine;
    private int voronoiID;

    [Header("Post Processing")]
    [SerializeField] private Volume globalVolume;

    private Bloom bloom;
    private Coroutine bloomCoroutine;

    private void Awake()
    {
        if (bloodMaterial == null)
        {
            Debug.LogError("BloodEffectController: no hay material asignado");
            return;
        }

        voronoiID = Shader.PropertyToID(voronoiProperty);

        if (!bloodMaterial.HasProperty(voronoiID))
            Debug.LogWarning($"BloodEffectController: propiedad {voronoiProperty} no encontrada en {bloodMaterial.name}");

        if (globalVolume != null && globalVolume.profile.TryGet(out bloom))
        {
            bloom.clamp.overrideState = true;
        }
        else
        {
            Debug.LogWarning("No se encontró Bloom en el Global Volume");
        }
    }

   
    public void PlayEffect()
    {
        StartTransition(-0.6f); // mínimo del slider
        StartBloomTransition(0f); // Clamp 1 a 0
    }

    
    public void StopEffect()
    {
        StartTransition(0f); // máximo del slider
        StartBloomTransition(1f); // Clamp 0 a 1
    }

    private void StartTransition(float targetValue)
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(SetShaderFloatOverTime(targetValue));
    }

    private void StartBloomTransition(float targetValue)
    {
        if (bloom == null) return;

        if (bloomCoroutine != null)
            StopCoroutine(bloomCoroutine);

        bloomCoroutine = StartCoroutine(SetBloomClampOverTime(targetValue));
    }


    private IEnumerator SetShaderFloatOverTime(float targetValue)
    {
        if (bloodMaterial == null) yield break;

        float startValue = bloodMaterial.GetFloat(voronoiID);
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            float newValue = Mathf.Lerp(startValue, targetValue, t);

            bloodMaterial.SetFloat(voronoiID, newValue);

            yield return null;
        }

        bloodMaterial.SetFloat(voronoiID, targetValue);

        transitionCoroutine = null;
    }

    private IEnumerator SetBloomClampOverTime(float targetValue)
    {
        float startValue = bloom.clamp.value;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            float newValue = Mathf.Lerp(startValue, targetValue, t);

            bloom.clamp.value = newValue;

            yield return null;
        }

        bloom.clamp.value = targetValue;
        bloomCoroutine = null;
    }
}

