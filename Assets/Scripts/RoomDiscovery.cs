using UnityEngine;

public class RoomDiscovery : MonoBehaviour
{
    [Header("Visual exclusiva del Minimapa")]
    [Tooltip("Arrastra aqui la copia visual de la sala que tiene la layer MinimapOnly")]
    [SerializeField] private GameObject minimapVisual;

    private bool isDiscovered = false;

    private void Awake()
    {
        // Al generarse el mapa, arranca invisible para el minimapa
        if (minimapVisual != null)
        {
            minimapVisual.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDiscovered || !other.CompareTag("Player")) return;

        isDiscovered = true;

        // Se enciende y queda visible para siempre
        if (minimapVisual != null)
        {
            minimapVisual.SetActive(true);
        }

        // Destruye el script para no gastar recursos
        Destroy(this);
    }
}