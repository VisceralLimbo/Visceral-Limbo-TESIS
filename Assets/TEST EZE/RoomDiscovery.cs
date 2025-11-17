// RoomDiscovery.cs (Script NUEVO)

using UnityEngine;

public class RoomDiscovery : MonoBehaviour
{
    private DungeonPart part;
    private bool isDiscovered = false;

    private void Start()
    {
        // Obtiene la referencia a la información de la pieza
        part = GetComponent<DungeonPart>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Solo si el jugador entra
        if (other.CompareTag("Player"))
        {
            if (part != null && MinimapManager.Instance != null)
            {
                // La visibilidad solo se activa la primera vez
                if (!isDiscovered)
                {
                    MinimapManager.Instance.DiscoverPart(part);
                    isDiscovered = true;
                }

                // EL ANCLAJE OCURRE SIEMPRE QUE EL JUGADOR ESTÉ DENTRO
                // (Esto corrige la desincronización al volver a entrar)
                MinimapManager.Instance.StartRoomTracking(part);
            }
        }
    }
}