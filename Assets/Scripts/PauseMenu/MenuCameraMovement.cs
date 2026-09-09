using UnityEngine;

public class MenuCameraMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float mov = 0.15f;
    [SerializeField] private float velocity = 0.15f;

    [Header("Rotacion")]
    [SerializeField] private float rotation = 1.5f;

    private Vector3 posini;
    private Quaternion rotationini;

    private float seedX;
    private float seedY;
    private float seedRot;

    void Start()
    {
        posini = transform.position;
        rotationini = transform.rotation;

        seedX = Random.Range(0f, 100f);
        seedY = Random.Range(0f, 100f);
        seedRot = Random.Range(0f, 100f);
    }

    void Update()
    {
        // movimiento de la cam
        float x = (Mathf.PerlinNoise(seedX, Time.time * velocity) - 0.5f) * 2f;
        float y = (Mathf.PerlinNoise(seedY, Time.time * velocity) - 0.5f) * 2f;

        transform.position = posini + new Vector3(x * mov, y * mov, 0f);

        // rotacionn
        float rot = (Mathf.PerlinNoise(seedRot, Time.time * velocity) - 0.5f) * 2f;

        transform.rotation = rotationini * Quaternion.Euler(rot * rotation, rot * rotation, rot * rotation * 0.3f);
    }
}