using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum ShakeType
{
    AllDirections,
    Vertical,
    Horizontal
}

public class CameraShake : MonoBehaviour
{
    public Transform shakePivot;

    public static CameraShake instance;


    private void Awake()
    {
        instance = this;
    }


    public void ShakeCamera(float duration, float shakeSize, ShakeType shakeType = ShakeType.AllDirections)
    {
        StartCoroutine(Shake(duration, shakeSize, shakeType));
    }

    public IEnumerator Shake(float duration, float shakeSize, ShakeType shakeType)
    {
        float elapsed = 0f;
        Vector3 originalPosition = shakePivot.localPosition;


        Vector3 target = originalPosition;
        float changeInterval = 0.05f; // cada 0.05s cambia el destino
        float timer = 0f;



        while (elapsed < duration)
        {
            timer += Time.deltaTime;

            if (timer >= changeInterval)
            {
                float x = 0f, y = 0f;

                switch (shakeType)
                {
                    case ShakeType.AllDirections:
                        x = Random.Range(-1f, 1f) * shakeSize;
                        y = Random.Range(-1f, 1f) * shakeSize;
                        break;
                    case ShakeType.Vertical:
                        y = Random.Range(-1f, 1f) * shakeSize;
                        break;
                    case ShakeType.Horizontal:
                        x = Random.Range(-1f, 1f) * shakeSize;
                        break;
                }

                target = new Vector3(x, y, originalPosition.z);
                timer = 0f;
            }

            // interpolación suave hacia el target
            shakePivot.localPosition = Vector3.Lerp(shakePivot.localPosition, target, Time.deltaTime * 10f);

            elapsed += Time.deltaTime;
            yield return null;
        }


        while (Vector3.Distance(originalPosition, shakePivot.localPosition) > 0.01f)
        {
            shakePivot.localPosition = Vector3.Lerp(shakePivot.localPosition, originalPosition, Time.deltaTime * 5f);
            yield return null;
        }

        shakePivot.localPosition = originalPosition;
    }
}

//para darle la intencidad cree la clase estatica q era lo me habia pedido pato
public static class CameraShakeIntensity
{
    public static float currentIntensity = 0.5f; // defau 0.5
}

