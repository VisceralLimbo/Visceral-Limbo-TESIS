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

    [Header("Scene Type")]
    [SerializeField] private bool tutorialScene;

    private float targetZoom;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        targetZoom = cam.orthographicSize;
    }

    void LateUpdate()
    {
        if (playerTransform == null)
            return;

        // si es tuto sin dungeongenerator
        if (tutorialScene)
        {
            FollowPlayer();
            HandleZoom();
            return;
        }

        // si es procedural con dungeongenerator
        if (DungeonGenerator.Instance != null &&
            DungeonGenerator.Instance.HasFinishedGeneration())
        {
            FollowPlayer();
            HandleZoom();
        }
    }

    void FollowPlayer()
    {
        Vector3 targetPosition = new Vector3(
            playerTransform.position.x,
            transform.position.y,
            playerTransform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * lerpSpeed
        );
    }

    void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0)
        {
            targetZoom -= scrollInput * zoomSpeed * 10f;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetZoom,
            Time.deltaTime * lerpSpeed
        );
    }
}
