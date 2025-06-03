using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public Transform playerCam;

    public static CameraShake instance;


    private void Awake()
    {
        instance = this;
    }


    public void ShakeCamera(float duration, float shakeSize)
    {
        StartCoroutine(Shake(duration, shakeSize));
    }

    public IEnumerator Shake(float duration, float shakeSize)
    {
        float elapsedTime = 0f;

        Vector3 originalPosition = playerCam.localPosition;

        while(elapsedTime < duration)
        {
            float x = Random.Range(-1f, 1f) * shakeSize;
            float y = Random.Range(-1f, 1f) * shakeSize;

            Vector3 pos = new Vector3(x, y, originalPosition.z);
            playerCam.localPosition = Vector3.Lerp(playerCam.localPosition, pos, Time.deltaTime * 5f);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        while (Vector3.Distance(originalPosition, playerCam.localPosition) > 0.01f)
        {

            playerCam.localPosition = Vector3.Lerp(playerCam.localPosition, originalPosition, Time.deltaTime * 2f);

            yield return null;

        }

        playerCam.localPosition = originalPosition; 
    }
}
