using System.Collections;
using UnityEngine;

public class BloodEffectController : MonoBehaviour
{
    [Header("Shader Settings")]
    [SerializeField] private Material bloodMaterial;
    [SerializeField] private string voronoiProperty = "_VoronoiPower";
    [SerializeField] private float transitionDuration = 1f;

    private Coroutine transitionCoroutine;
    private int voronoiID;

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
    }

   
    public void PlayEffect()
    {
        StartTransition(-0.6f); // mínimo del slider
    }

    
    public void StopEffect()
    {
        StartTransition(0f); // máximo del slider
    }

    private void StartTransition(float targetValue)
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(SetShaderFloatOverTime(targetValue));
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
}

