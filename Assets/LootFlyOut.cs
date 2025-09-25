using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootFlyOut : MonoBehaviour
{
    public float flyHeight = 1.5f;   // altura máxima de la parábola
    public float flyDistance = 1.5f; // distancia hacia adelante
    public float duration = 1f;      // cuánto tarda en salir
    public Vector3 finalScale = Vector3.one;

    private Vector3 startPos;
    private Vector3 endPos;
    private Vector3 controlPoint;
    private float timer;

    void Start()
    {
        transform.localScale = Vector3.one * 0.1f;

        startPos = transform.position;

        endPos = startPos + transform.forward * flyDistance;

        controlPoint = (startPos + endPos) / 2f + Vector3.up * flyHeight;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);

        Vector3 pos = Mathf.Pow(1 - t, 2) * startPos +
                      2 * (1 - t) * t * controlPoint +
                      Mathf.Pow(t, 2) * endPos;

        transform.position = pos;

        transform.localScale = Vector3.Lerp(Vector3.one * 0.1f, finalScale, t);

        transform.Rotate(Vector3.up * 180 * Time.deltaTime / 2);
    }
}
