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

                //  ANCLAJE: SIEMPRE debe actualizarse al entrar en una nueva sala
                MinimapManager.Instance.StartRoomTracking(part);

                // ELIMINA LA VARIABLE isDiscovered DE AQUÍ ABAJO (si la tenías)
            }

            Debug.Log("ENTRANDO a sala: " + part.MapCoords + " | Nombre: " + gameObject.name);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Solo si detectamos que el jugador está en la sala pero el minimapa no lo sabe.
        if (other.CompareTag("Player") && MinimapManager.Instance != null && part != null)
        {
            // Forzar el anclaje y la revelación si el jugador está aquí.
            // Esto soluciona si el OnTriggerEnter se perdió.
            MinimapManager.Instance.StartRoomTracking(part);

            if (!isDiscovered)
            {
                MinimapManager.Instance.DiscoverPart(part);
                isDiscovered = true;
            }

            Debug.Log("Stay: Rastreo forzado a sala: " + part.MapCoords);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // VITAL: Si salimos de una sala, y el MinimapManager sigue rastreando esta sala,
            // (porque el OnTriggerEnter de la siguiente sala falló)
            // forzamos la actualización de rastreo a la siguiente sala.

            // Por seguridad, no hacemos nada al salir por ahora, para evitar que el jugador salte al vacío.

            // La detección debe ocurrir en OnTriggerEnter/Stay de la *nueva* sala.

            // *Recomendación:* Simplemente dejamos que OnTriggerStay fuerce la actualización
            // ya que es el método más fiable en entornos de KCC/física complejos. 

            // Asegúrate de que el collider que estás usando para la detección del minimapa
            // sea ligeramente más pequeño que la sala visual, para que el "Stay" no se solape demasiado.

            // Para solucionar el problema de que el mapa no se actualiza, nos enfocaremos en:
            // 1. Asegurar que el trigger de la nueva sala sea detectado.
            // 2. Usar un retraso de 1 frame si el trigger se pierde.
            Debug.LogWarning("SALIENDO de sala: " + part.MapCoords + " | Nombre: " + gameObject.name);
        }
    }
}