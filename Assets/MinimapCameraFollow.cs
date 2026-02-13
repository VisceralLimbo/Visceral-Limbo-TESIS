using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    private Camera cam;

    [Header("player transform")]
    [SerializeField] private Transform playerTransform;

    [Header("zoom settings")]
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minZoom = 10f;
    [SerializeField] private float maxZoom = 150f;
    [SerializeField] private float lerpSpeed = 5f;

    private float targetZoom;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        targetZoom = cam.orthographicSize;
    }

    void LateUpdate()
    {
        // sigo si player esta y si la generacion termino
        if (playerTransform != null && DungeonGenerator.Instance.HasFinishedGeneration())
        {
            FollowPlayer();
            HandleZoom();
        }
    }

    void FollowPlayer()
    {
        // camara sobre el player
        Vector3 targetPosition = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);

        // lerp para suavizar
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * lerpSpeed);
    }

    void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0)
        {
            targetZoom -= scrollInput * zoomSpeed * 10f;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        // aplico zoom
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * lerpSpeed);
    }
}
