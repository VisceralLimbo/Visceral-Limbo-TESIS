using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingBuffo : MonoBehaviour
{
    [Header("Floating")]
    public float floatAmplitude = 0.25f;
    public float floatFrequency = 2f;

    [Header("Rotation")]
    public Transform x2Child;  
    public float x2RotationSpeed = 90f;  

    private Vector3 startPos;
    private Vector3 x2InitialLocalPos;
    private float currentAngle = 0f;

    void Start()
    {
        startPos = transform.position;
        if (x2Child != null) x2InitialLocalPos = x2Child.localPosition;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        if (x2Child != null)
        {
            currentAngle += x2RotationSpeed * Time.deltaTime;
            currentAngle %= 360f;

            x2Child.localRotation = Quaternion.Euler(0f, currentAngle, 0f);
            x2Child.localPosition = x2InitialLocalPos;
        }
    }
}
