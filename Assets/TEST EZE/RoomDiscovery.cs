// RoomDiscovery.cs (Script NUEVO)

using UnityEngine;

public class RoomDiscovery : MonoBehaviour
{
    private DungeonPart part;
    private bool isDiscovered = false;

    private void Start()
    {
        // Obtiene la referencia a la información de la pieza
        // Ahora busca el componente en el objeto padre (DungeonPart)
        part = GetComponentInParent<DungeonPart>();

        if (part == null)
        {
            Debug.LogError("RoomDiscovery no encontró DungeonPart en el objeto padre.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (part != null && MinimapManager.Instance != null)
            {
                // *** Descubrimos la parte (Ahora le da color, no activa el GameObject) ***
                if (!isDiscovered)
                {
                    MinimapManager.Instance.DiscoverPart(part); // <--- Llama a la nueva lógica de color
                    isDiscovered = true;
                }

                // EL ANCLAJE OCURRE SIEMPRE QUE EL JUGADOR ESTÉ DENTRO
                // Esta llamada ya no tiene efecto de movimiento, pero asegura que currentTrackedRoom se actualice
                MinimapManager.Instance.StartRoomTracking(part);
            }
        }
    }
}