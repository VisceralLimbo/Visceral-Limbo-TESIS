using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI; // Necesario para componentes de UI (Image, RectTransform)

public class MinimapManager : MonoBehaviour
{
    // Patrón Singleton para acceso fácil
    public static MinimapManager Instance;

    // --- CONFIGURACIÓN EN EL INSPECTOR ---

    [Header("Referencias de UI")]
    [SerializeField] private RectTransform MapContainer;
    [SerializeField] private float mapScale = 100f; // Usaremos 100f

    [Header("Prefabs de Iconos (Imágenes de UI)")]
    // Arrastra aquí los prefabs de las imágenes/sprites de tu minimapa
    [SerializeField] private GameObject RegularRoomPrefab;
    [SerializeField] private GameObject HallwayPrefab;
    [SerializeField] private GameObject EntranceRoomPrefab;
    [SerializeField] private GameObject SpecialRoomPrefab;
    [SerializeField] private GameObject BossRoomPrefab;
    [SerializeField] private GameObject PlayerIconPrefab;
    private DungeonPart currentTrackedRoom = null;
    private GameObject playerIcon; // <-- Debe existir

    [Header("Control de UI")]
    [SerializeField] private GameObject MinimapPanel;
    private bool isMapVisible = false;

    private Vector3 mapWorldOrigin = Vector3.zero;

    // --- DATOS INTERNOS ---
    private Dictionary<DungeonPart, GameObject> mapIcons = new Dictionary<DungeonPart, GameObject>();
    private GameObject cachedPlayer; // Referencia al objeto del jugador
                                     // Diccionario para almacenar el color original de cada ícono de pieza
    private Dictionary<GameObject, UnityEngine.Color> originalIconColors = new Dictionary<GameObject, UnityEngine.Color>();

    [Header("Iconos de Conexión")]
    [SerializeField] private GameObject ConnectionIconPrefab; // Prefab de UI (una línea o cuadrado pequeño)
    [SerializeField] private float ConnectionLineLength = 20f; // Longitud de la línea de conexión (ej. 20 unidades)
    [SerializeField] private float ConnectionLineWidth = 5f; // Grosor de la línea
    [SerializeField] private UnityEngine.Color ConnectedColor = UnityEngine.Color.yellow; // Color de la conexión

    private Dictionary<DungeonPart, List<GameObject>> partConnections = new Dictionary<DungeonPart, List<GameObject>>();

    // --- MÉTODOS DE INICIO ---

    // Mover la lógica de ocultación inicial al Awake()
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (MinimapPanel != null)
        {
            // 1. Aseguramos que el panel visible (el marco/padre del mapa) esté inactivo
            MinimapPanel.SetActive(false);
        }

        if (MapContainer != null)
        {
            // 2. Por si acaso, apagamos el contenedor de los íconos también
            MapContainer.gameObject.SetActive(false);
        }

        // 3. Establecemos el estado lógico inicial
        isMapVisible = false;
    }

    private void Start()
    {
        // BUSCAMOS AL JUGADOR UNA SOLA VEZ
        cachedPlayer = GameObject.FindGameObjectWithTag("Player");

        // Instanciamos el icono.
        // parentTransform AHORA DEBE SER EL MAPCONTAINER para que el ícono se mueva con las coordenadas de la cuadrícula.
        Transform parentTransform = MapContainer.transform;

        if (cachedPlayer != null && playerIcon == null && PlayerIconPrefab != null && parentTransform != null)
        {
            // Instancia como hijo del CONTENEDOR DEL MAPA (MapContainer)
            playerIcon = Instantiate(PlayerIconPrefab, parentTransform);
            playerIcon.SetActive(true);

            // El Player Icon ahora empieza en (0, 0) y su posición será actualizada por MovePlayerIconToRoomCenter.
            RectTransform rt = playerIcon.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.one * 0.5f;
            rt.anchorMax = Vector2.one * 0.5f;
            rt.pivot = Vector2.one * 0.5f;
            rt.anchoredPosition = Vector2.zero; // Comienza en 0,0 (sala de entrada)

            playerIcon.transform.SetAsLastSibling();
        }

        // *** NUEVO: ENLACE DEL EVENTO DE GENERACIÓN ***
        if (DungeonGenerator.Instance != null)
        {
            DungeonGenerator.Instance.OnSuccessfulGeneration += CenterMapInitial;
        }
    }

    void Update()
    {
        // --- CONTROL DE TECLADO (TAB) ---
        // Usamos GetKey() para mantenerlo abierto mientras se presiona.
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Abrir el mapa (si está cerrado)
            SetMapVisibility(true);
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            // Cerrar el mapa (al soltar la tecla)
            SetMapVisibility(false);
        }

        // --- LÓGICA DE ROTACIÓN DEL PLAYER ICON (SOLO SI ES VISIBLE) ---
        if (cachedPlayer != null && playerIcon != null && isMapVisible)
        {
            float playerYRotation = cachedPlayer.transform.rotation.eulerAngles.y;

            RectTransform playerIconRect = playerIcon.GetComponent<RectTransform>();

            // Aplicar la rotación del jugador
            playerIconRect.localRotation = Quaternion.Slerp(
                playerIconRect.localRotation,
                Quaternion.Euler(0, 0, -playerYRotation),
                Time.deltaTime * 10f
            );
        }
    }

    // --- REGISTRO DE PIEZAS (Llamado desde DungeonGenerator) ---

    /// <summary>
    /// Crea y posiciona el icono de UI para una nueva DungeonPart.
    /// </summary>
    public void RegisterNewPart(DungeonPart newPart)
    {
        // 1. Selecciona el prefab de UI correcto basado en el tipo de pieza
        GameObject prefabToUse = null;
        switch (newPart.RoomType)
        {
            case DungeonPart.DungeonPartType.Room:
                prefabToUse = RegularRoomPrefab;
                break;
            case DungeonPart.DungeonPartType.Hallway:
                prefabToUse = HallwayPrefab;
                break;
            case DungeonPart.DungeonPartType.Entrance:
                prefabToUse = EntranceRoomPrefab;
                break;
            case DungeonPart.DungeonPartType.Special:
                prefabToUse = SpecialRoomPrefab;
                break;
            case DungeonPart.DungeonPartType.Boss:
                prefabToUse = BossRoomPrefab;
                break;
            default:
                return;
        }

        if (prefabToUse == null) return;

        // 2. Instancia el icono de UI en el contenedor
        GameObject icon = Instantiate(prefabToUse, MapContainer);

        // 3. Posiciona el icono en el Canvas usando las coordenadas lógicas (MapCoords)
        icon.GetComponent<RectTransform>().anchoredPosition = new Vector2(newPart.MapCoords.x * mapScale, newPart.MapCoords.y * mapScale);

        // 4. Inicializa el estado (Niebla de Guerra)
        icon.SetActive(true); // <--- DEBEN ESTAR ACTIVOS PARA VERSE EN GRIS/NEGRO

        // Fusionamos los dos bloques TryGetComponent para declarar 'image' solo una vez
        if (icon.TryGetComponent(out Image image))
        {
            // 1. **GUARDAR EL COLOR ORIGINAL**
            // (Asegúrate de que originalIconColors usa UnityEngine.Color para evitar el error CS0104)
            if (!originalIconColors.ContainsKey(icon))
            {
                originalIconColors.Add(icon, image.color);
            }

            // 2. Aplicar la Niebla de Guerra (cambiar a gris oscuro)
            image.color = UnityEngine.Color.gray; // Usamos el prefijo por seguridad
        }

        //// **CAMBIO TEMPORAL PARA TESTEO:** Activar todos los iconos:
        //icon.SetActive(true);
        // 5. Guarda la referencia
        if (!mapIcons.ContainsKey(newPart))
        {
            mapIcons.Add(newPart, icon);
        }
        else
        {
            Debug.LogWarning("MinimapManager: Ya existe una entrada para esta DungeonPart.");
            Destroy(icon);
        }

        // *** NUEVO: DIBUJAR LAS CONEXIONES ***
        DrawConnectionsForPart(newPart);

        // ** NUEVO: ORDENAR EL DIBUJO **
        // Las salas regulares, especiales y de jefe deben dibujarse DESPUÉS de los pasillos
        if (newPart.RoomType == DungeonPart.DungeonPartType.Room ||
            newPart.RoomType == DungeonPart.DungeonPartType.Special ||
            newPart.RoomType == DungeonPart.DungeonPartType.Boss)
        {
            // Mueve el icono al final de la lista de hermanos (se dibuja encima)
            icon.transform.SetAsLastSibling();
        }

        // Si es la sala de entrada, la descubrimos inmediatamente.
        if (newPart.RoomType == DungeonPart.DungeonPartType.Entrance)
        {
            DiscoverPart(newPart); // Llama a DiscoverPart para darle color a la primera sala
            mapWorldOrigin = newPart.transform.position;
        }

    }

    // --- DESCUBRIMIENTO DE MAPA (Niebla de Guerra) ---

    /// <summary>
    /// Hace visible el icono de una DungeonPart cuando el jugador entra en ella.
    /// </summary>
    public void DiscoverPart(DungeonPart part)
    {
        if (mapIcons.ContainsKey(part))
        {
            GameObject icon = mapIcons[part];

            if (icon != null && icon.TryGetComponent(out Image image) && originalIconColors.ContainsKey(icon))
            {
                // Restaura el color original. Si por alguna razón el diccionario falla, usa UnityEngine.Color.white
                image.color = originalIconColors[icon];
            }
        }
    }

    public void UpdatePlayerIcon(Vector3 worldPosition)
    {
        // Esta función ahora solo existe para llamarse desde el Player/RoomDiscovery si es necesario,
        // pero ya no mueve nada. Su llamado ya no es necesario en Update.
        // Dejarla así evita errores de compilación si otras clases la llaman, pero su cuerpo está vacío.
    }
    // FUNCIÓN REVERTIDA/ELIMINADA: La lógica de centrado del mapa por sala ya no es necesaria.
    // El MapContainer se centrará una sola vez.

    /// <summary>
    /// Elimina todos los iconos de pieza generados en el minimapa.
    /// </summary>
    public void ClearMap()
    {
        // Limpiamos los iconos de las piezas
        foreach (var icon in mapIcons.Values)
        {
            if (icon != null)
            {
                Destroy(icon);
            }
        }
        mapIcons.Clear();

        // *** NUEVO: LIMPIAR ICONOS DE CONEXIÓN ***
        foreach (var connectionList in partConnections.Values)
        {
            foreach (var icon in connectionList)
            {
                if (icon != null)
                {
                    Destroy(icon);
                }
            }
        }
        partConnections.Clear();

        // Opcional: También eliminamos el icono del jugador si lo hemos instanciado
        if (playerIcon != null)
        {
            Destroy(playerIcon);
            playerIcon = null;
        }

        Debug.Log("MinimapManager: Iconos de mapa limpiados para regeneración.");
    }

    /// <summary>
    /// Establece la visibilidad del panel del minimapa.
    /// </summary>
    public void SetMapVisibility(bool visible)
    {
        if (MinimapPanel == null) return;

        // Si el estado no cambia, salir
        if (isMapVisible == visible) return;

        isMapVisible = visible;

        // ACTIVAR/DESACTIVAR AMBOS OBJETOS
        MinimapPanel.SetActive(visible);

        // Si MapContainer es un hijo de MinimapPanel, esta línea puede ser redundante, 
        // pero asegura que la visibilidad se controle explícitamente.
        if (MapContainer != null)
        {
            MapContainer.gameObject.SetActive(visible);
        }
    }

    /// <summary>
    /// Fuerza la creación del ícono del jugador si no ha sido instanciado.
    /// </summary>
    public void EnsurePlayerIconIsInstantiated()
    {
        // Verifica si el ícono (playerIcon) no existe Y si tenemos las referencias
        if (playerIcon == null && PlayerIconPrefab != null && MapContainer != null)
        {
            // Instancia el Prefab del Ícono en el Contenedor
            playerIcon = Instantiate(PlayerIconPrefab, MapContainer.transform);
            playerIcon.SetActive(true);

            // Lo ponemos como último para que se dibuje SIEMPRE por encima de las salas
            playerIcon.transform.SetAsLastSibling();

            // (Opcional, busca al jugador para una posición inicial, si aún no está en cache)
            if (cachedPlayer == null)
            {
                cachedPlayer = GameObject.FindGameObjectWithTag("Player");
            }
        }
    }

    // Localice y reemplace el método StartRoomTracking:
    public void StartRoomTracking(DungeonPart part)
    {
        currentTrackedRoom = part;

        // *** NUEVO ***
        // Mueve el ícono del jugador a la coordenada de la nueva sala.
        MovePlayerIconToRoomCenter(part.MapCoords);
    }

    // MANTENEMOS ESTE MÉTODO VACÍO PARA EVITAR ERRORES SI OTRAS CLASES LO LLAMAN:
    private void UpdatePlayerIconRelative(Vector3 worldPosition, Vector3 roomCenter3D, Vector2Int roomCoords2D)
    {
        // Esta función no hace nada, el mapa es estático.
    }

    // Este método ahora asegura que el mapa esté en la posición inicial (centrado).
    public void CenterMapInitial()
    {
        if (MapContainer == null || DungeonGenerator.Instance == null) return;

        // 1. Calcular el centro geométrico de la mazmorra.
        // Usaremos el centro de la mazmorra para posicionar el MapContainer de forma estática.
        Vector2 minCoords = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 maxCoords = new Vector2(float.MinValue, float.MinValue);

        // Asumimos que todas las piezas ya están registradas en el diccionario mapIcons
        foreach (var partIcon in mapIcons)
        {
            Vector2 coords = partIcon.Key.MapCoords;
            minCoords.x = Mathf.Min(minCoords.x, coords.x);
            minCoords.y = Mathf.Min(minCoords.y, coords.y);
            maxCoords.x = Mathf.Max(maxCoords.x, coords.x);
            maxCoords.y = Mathf.Max(maxCoords.y, coords.y);
        }

        // 2. Calcular la posición central para el MapContainer
        Vector2 centerMapCoords = (minCoords + maxCoords) / 2f;
        Vector2 centerMapPosition = centerMapCoords * mapScale;

        // 3. Posicionar el MapContainer para centrar todo el mapa visiblemente
        MapContainer.anchoredPosition = -centerMapPosition;

        // 4. Aseguramos que el contenedor del mapa no rote.
        MapContainer.localRotation = Quaternion.identity;
    }

    private void OnDestroy()
    {
        // *** IMPORTANTE: DESENLACE PARA EVITAR ERRORES ***
        if (DungeonGenerator.Instance != null)
        {
            DungeonGenerator.Instance.OnSuccessfulGeneration -= CenterMapInitial;
        }
    }

    /// <summary>
    /// Mueve el MapContainer para centrar la sala actual en la pantalla.
    /// Se llama cuando el jugador entra en una nueva sala (tepeo).
    /// </summary>
    private void CenterMapOnRoom(Vector2Int roomCoords2D)
    {
        if (MapContainer == null) return;

        // 1. Calcular la Posición Lógica (la coordenada central de la sala actual en el minimapa).
        Vector2 roomMapPosition = new Vector2(
            roomCoords2D.x * mapScale,
            roomCoords2D.y * mapScale
        );

        // 2. Mover el MapContainer al negativo de la posición lógica del centro de la sala.
        // Esto hace que el ícono (fijo en 0,0) parezca estar DENTRO de la sala.
        Vector2 targetContainerPosition = -roomMapPosition;

        // 3. Aplicar la posición instantáneamente (sin Lerp) para el efecto de "tepeo".
        MapContainer.anchoredPosition = targetContainerPosition;
    }

    /// <summary>
    /// Mueve el ícono del jugador al centro de la nueva sala.
    /// EL MAPA NO SE MUEVE.
    /// </summary>
    private void MovePlayerIconToRoomCenter(Vector2Int roomCoords2D)
    {
        if (playerIcon == null) return;

        // 1. Calcular la Posición Lógica (la coordenada central de la sala actual en el minimapa).
        Vector2 roomMapPosition = new Vector2(
            roomCoords2D.x * mapScale,
            roomCoords2D.y * mapScale
        );

        // 2. Aplicar la posición a un objeto de UI.
        // ¡El jugador ahora debe moverse!
        RectTransform playerIconRect = playerIcon.GetComponent<RectTransform>();

        // 3. Aplicar la posición instantáneamente (sin Lerp) al ícono del jugador.
        // OJO: La posición del ícono se calcula relativa a su padre estático (MinimapPanel) 
        // y debe compensarse por la posición de MapContainer (que se ajustó en CenterMapInitial).
        // Para simplificar, si el ícono del jugador es hijo del MapContainer, la posición es simplemente:

        // *****************************************************************************************
        // IMPORTANTE: PARA QUE ESTO FUNCIONE, EL PLAYER ICON DEBE SER HIJO DEL MAPCONTAINER.
        // *****************************************************************************************

        // Revertiremos la jerarquía del ícono del jugador para que sea hijo del MapContainer:

        // AHORA: El MapContainer está estáticamente centrado.
        // El ícono del jugador (que es hijo del MapContainer) se mueve a la coordenada de la sala.
        playerIconRect.anchoredPosition = roomMapPosition;
    }

    /// <summary>
    /// Dibuja indicadores de conexión (puertas) alrededor del icono de la pieza.
    /// </summary>
    private void DrawConnectionsForPart(DungeonPart part)
    {
        if (ConnectionIconPrefab == null || !mapIcons.ContainsKey(part)) return;

        // Obtener el icono de la sala en el minimapa (donde se dibujarán las líneas)
        GameObject partIcon = mapIcons[part];
        RectTransform partRect = partIcon.GetComponent<RectTransform>();

        List<GameObject> drawnIcons = new List<GameObject>();

        // 1. Iterar sobre todos los puntos de entrada/salida de la pieza
        foreach (var entryPoint in part.EntryPoints)
        {
            // 2. Solo dibujamos la conexión si la puerta está realmente ocupada (conectada)
            if (entryPoint.IsOccupied())
            {
                // 3. Calcular la dirección de la puerta en el mundo (X/Z)
                Vector3 forward = entryPoint.transform.forward;
                Vector2 connectionDirection = Vector2.zero;
                float threshold = 0.8f;

                // Mapeo de 3D a 2D (X->X, Z->Y)
                if (Mathf.Abs(forward.z) > threshold)
                {
                    connectionDirection.y = Mathf.Sign(forward.z); // Arriba (+Y) o Abajo (-Y)
                }
                else if (Mathf.Abs(forward.x) > threshold)
                {
                    connectionDirection.x = Mathf.Sign(forward.x); // Derecha (+X) o Izquierda (-X)
                }

                if (connectionDirection != Vector2.zero)
                {
                    // 4. Instanciar y posicionar el icono de conexión
                    GameObject connectionIcon = Instantiate(ConnectionIconPrefab, partRect);
                    Image image = connectionIcon.GetComponent<Image>();
                    RectTransform connRect = connectionIcon.GetComponent<RectTransform>();

                    // 5. Configurar el color y la posición (temporalmente usando color amarillo)
                    image.color = ConnectedColor;

                    // Posición: Desplazarlo ligeramente hacia el borde del icono
                    float offsetDistance = mapScale / 2f;
                    connRect.anchoredPosition = new Vector2(
                        connectionDirection.x * offsetDistance,
                        connectionDirection.y * offsetDistance
                    );

                    // Rotación: Rotar el icono para que apunte en la dirección de la conexión
                    float angle = Mathf.Atan2(connectionDirection.y, connectionDirection.x) * Mathf.Rad2Deg;
                    connRect.localRotation = Quaternion.Euler(0, 0, angle - 90f); // -90 para que el eje Y apunte al forward

                    // Tamaño: Escalar para simular una línea o un punto
                    connRect.sizeDelta = new Vector2(ConnectionLineWidth, ConnectionLineLength);

                    drawnIcons.Add(connectionIcon);
                }
            }
        }

        // Guardar la lista de iconos de conexión para limpieza futura
        if (drawnIcons.Count > 0)
        {
            partConnections.Add(part, drawnIcons);
        }
    }
}