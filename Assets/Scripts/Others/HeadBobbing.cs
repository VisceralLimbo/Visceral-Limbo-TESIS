using UnityEngine;

public class HeadBobbing : MonoBehaviour
{
    [Header("References")]
    public Player_Movement player; // Referencia al script de movimiento
    public Transform cameraTransform; // Transform de la cámara

    [Header("Bobbing Settings")]
    public float frequency = 10f;
    public float amplitude = 0.05f;
    public float lerpSpeed = 5f;

    private float timer;
    private Vector3 initialLocalPos;

    void Start()
    {
        if (cameraTransform == null) cameraTransform = transform;
        initialLocalPos = cameraTransform.localPosition;
    }

    void Update()
    {
        Vector3 velocity = player.KKCMotor.BaseVelocity;
        bool isMoving = velocity.magnitude > 0.1f;
        bool isGrounded = player.CurrentState.Grounded;

        if (isMoving && isGrounded)
        {
            timer += Time.deltaTime * frequency;
            float bobOffset = Mathf.Sin(timer) * amplitude;

            Vector3 newPos = new Vector3(
                initialLocalPos.x,
                initialLocalPos.y + bobOffset,
                initialLocalPos.z
            );

            cameraTransform.localPosition = newPos;
        }
        else
        {
            timer = 0f;
            cameraTransform.localPosition = Vector3.Lerp(
                cameraTransform.localPosition,
                initialLocalPos,
                Time.deltaTime * lerpSpeed
            );
        }
    }
}

