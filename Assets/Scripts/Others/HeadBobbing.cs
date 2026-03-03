using UnityEngine;

public class HeadBobbing : MonoBehaviour
{
    [Header("References")]
    public Player_Movement player;
    public Transform cameraTransform;

    [Header("Settings")]
    public float bobSpeed = 6f;
    public float bobMagnitude = 0.04f;
    public float sideBobMagnitude = 0.02f;
    public float rotMagnitude = 1.2f;
    public float smoothTime = 6f;

    private Vector3 initialPos;
    private Vector3 velocity = Vector3.zero;
    private float bobTimer = 0f;

    void Start()
    {
        if (cameraTransform == null) cameraTransform = transform;
        initialPos = cameraTransform.localPosition;
    }

    void Update()
    {
        Vector3 vel = player.KKCMotor.BaseVelocity;
        bool isMoving = vel.magnitude > 0.1f && player.CurrentState.Grounded;

        
            
            bobTimer += Time.deltaTime * (bobSpeed + vel.magnitude * 0.2f);

            
            float upDown = (Mathf.PerlinNoise(bobTimer, 0) - 0.5f) * 2f;
            float leftRight = (Mathf.PerlinNoise(0, bobTimer) - 0.5f) * 2f;

            
            Vector3 targetPos = initialPos +
                Vector3.up * upDown * bobMagnitude +
                Vector3.right * leftRight * sideBobMagnitude;

            cameraTransform.localPosition = Vector3.SmoothDamp(
                cameraTransform.localPosition,
                targetPos,
                ref velocity,
                1f / smoothTime
            );

            
            float tiltX = Mathf.Sin(bobTimer * 0.7f) * rotMagnitude;
            float tiltZ = Mathf.Sin(bobTimer * 0.5f) * rotMagnitude * 0.5f;

            Quaternion targetRot = Quaternion.Euler(tiltX, 0f, tiltZ);

            cameraTransform.localRotation = Quaternion.Slerp(
                cameraTransform.localRotation,
                targetRot,
                Time.deltaTime * smoothTime
            );
        
    }
}

