using UnityEngine;

public class RoomDiscovery : MonoBehaviour
{
    [Header("solo visual del minimapa (padre)")]
    [SerializeField] private GameObject minimapVisual;

    private bool isDiscovered = false;

    private void Awake()
    {
        // empieza todo apagado
        if (minimapVisual != null)
        {
            minimapVisual.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDiscovered || !other.CompareTag("Player")) return;

        isDiscovered = true;

        // se prende y queda visible
        if (minimapVisual != null)
        {
            minimapVisual.SetActive(true);
        }

        Destroy(this);
    }
}
