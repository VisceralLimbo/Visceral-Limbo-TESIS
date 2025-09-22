using System.Collections;
using UnityEngine;

public class DialogueShaderController : MonoBehaviour
{
    [Header("Shader Settings")]
    [SerializeField] private Material dialogMaterial;
    [SerializeField] private string vignetteProperty = "_VignetteIntensity";
    [SerializeField] private float transitionDuration = 1f;

    private Coroutine transitionCoroutine;
    private int vignetteID;

    private void Awake()
    {
        if (dialogMaterial == null)
        {
            Debug.LogError("DialogueShaderController: no hay material asignado");
            return;
        }

        vignetteID = Shader.PropertyToID(vignetteProperty);


        dialogMaterial.SetFloat(vignetteID, 0f);
    }

    public void PlayEffect()
    {
        StartTransition(1f); // fade in hasta 1
    }

    public void StopEffect()
    {
        StartTransition(0f); // fade out hasta 0
    }

    private void StartTransition(float targetValue)
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(SetShaderFloatOverTime(targetValue));
    }

    private IEnumerator SetShaderFloatOverTime(float targetValue)
    {
        if (dialogMaterial == null) yield break;

        float startValue = dialogMaterial.GetFloat(vignetteID);
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            float newValue = Mathf.Lerp(startValue, targetValue, t);

            dialogMaterial.SetFloat(vignetteID, newValue);

            yield return null;
        }

        dialogMaterial.SetFloat(vignetteID, targetValue);

        transitionCoroutine = null;
    }
}

