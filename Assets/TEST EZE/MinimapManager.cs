using System.Collections.Generic;
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

    [Header("Centrado de Cámara")]
    [SerializeField] private float mapFollowSpeed = 5f; // Velocidad de seguimiento 

    private Vector3 mapWorldOrigin = Vector3.zero;

    // --- DATOS INTERNOS ---
    private Dictionary<DungeonPart, GameObject> mapIcons = new Dictionary<DungeonPart, GameObject>();
    private GameObject cachedPlayer; // Referencia al objeto del jugador
    private bool _mapNeedsSnap = false; // ¡NUEVA VARIABLE!
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
            MinimapPanel.SetActive(false);
            isMapVisible = false;
        }
    }

    private void Start()
    {
        // BUSCAMOS AL JUGADOR UNA SOLA VEZ
        cachedPlayer = GameObject.FindGameObjectWithTag("Player");

        // Instanciamos el icono.
        if (cachedPlayer != null && playerIcon == null && PlayerIconPrefab != null && MapContainer != null)
        {
            playerIcon = Instantiate(PlayerIconPrefab, MapContainer.transform);
            playerIcon.SetActive(true);
            playerIcon.transform.SetAsLastSibling();
        }
    }

    void Update()
    {
        // *** DEBUG DE EJECUCIÓN DEL SCRIPT ***
        Debug.Log("MinimapManager está activo.");

        // --- CONTROL DE TECLADO (TAB) ---
        if (Input.GetKey(KeyCode.Tab))
        {
            if (!isMapVisible) SetMapVisibility(true);
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            if (isMapVisible) SetMapVisibility(false);
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
        // Coordenada (X, Y) = (MapCoords.x * escala, MapCoords.y * escala)
        icon.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(newPart.MapCoords.x * mapScale, newPart.MapCoords.y * mapScale);

        // 4. Inicializa el estado (Niebla de Guerra)
        if (newPart.RoomType != DungeonPart.DungeonPartType.Entrance)
        {
            // Oculta/Oscurece todas las piezas excepto la inicial
            icon.SetActive(false);
        }
        else
        {
            icon.SetActive(true); // Asegura que la sala inicial esté visible
                                  // *** GUARDAR EL PUNTO DE ORIGEN DE LA MAZMORRA EN EL MUNDO 3D ***
            mapWorldOrigin = newPart.transform.position;
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

        // ** NUEVO: ORDENAR EL DIBUJO **
        // Las salas regulares, especiales y de jefe deben dibujarse DESPUÉS de los pasillos
        if (newPart.RoomType == DungeonPart.DungeonPartType.Room ||
            newPart.RoomType == DungeonPart.DungeonPartType.Special ||
            newPart.RoomType == DungeonPart.DungeonPartType.Boss)
        {
            // Mueve el icono al final de la lista de hermanos (se dibuja encima)
            icon.transform.SetAsLastSibling();
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
            mapIcons[part].SetActive(true); // <-- Hace visible el GameObject del minimapa.
        }
    }

    public void UpdatePlayerIcon(Vector3 worldPosition)
    {
        if (playerIcon == null) return;

        // --- 1. OBTENER PARÁMETROS DE RASTREO Y CALCULAR EL ORIGEN ---

        // Obtenemos el centro 3D de la sala rastreada (currentTrackedRoom)
        Vector3 roomCenter3D = mapWorldOrigin;
        Vector2Int roomCoords2D = Vector2Int.zero;

        // Si la sala rastreada no está seteada (currentTrackedRoom), el rastreo es inútil
        if (currentTrackedRoom == null)
        {
            // En este caso, el jugador solo se queda en el origen. No hay desfase fino.
            roomCenter3D = mapWorldOrigin;
            roomCoords2D = Vector2Int.zero;
        }
        else
        {
            roomCenter3D = currentTrackedRoom.transform.position;
            roomCoords2D = currentTrackedRoom.MapCoords;
        }


        // --- 2. CÁLCULO DE POSICIÓN RELATIVA Y ESCALA ---
        float mapScale = 100f;
        float roomSize = 50f; // Usar el valor que mejor funcionó
        float worldToMapRatio = mapScale / roomSize;
        float smoothSpeed = mapFollowSpeed;


        // a) Posición relativa: ¿Dónde está el jugador DENTRO del área de la sala?
        Vector3 relativeToRoom = worldPosition - roomCenter3D;

        // b) Desplazamiento fino: (Movimiento escalado dentro de la celda, centrado en 0)
        Vector2 fineMovement = new Vector2(
            relativeToRoom.x * worldToMapRatio,
            relativeToRoom.z * worldToMapRatio
        );

        // c) Posición de la Cuadrícula: Punto central de la celda lógica (ej: 100, 0)
        Vector2 gridCenterPosition = new Vector2(
            roomCoords2D.x * mapScale,
            roomCoords2D.y * mapScale
        );

        // d) Posición Absoluta: Dónde estaría el icono si no moviéramos la cámara
        Vector2 absoluteMapPosition = gridCenterPosition + fineMovement;


        // --- 3. APLICAR CENTRADO Y SMOOTHING ---

        // 1. Target de la CÁMARA (MapContainer): La inversa de la posición absoluta del mapa
        Vector2 targetMapPosition = -absoluteMapPosition;

        // 2. Target del ICONO: El movimiento fino DENTRO de la celda
        Vector2 targetIconPosition = fineMovement;


        // Suavizar el MapContainer (Cámara)
        RectTransform mapRect = MapContainer.GetComponent<RectTransform>();
        mapRect.anchoredPosition = Vector2.Lerp(
            mapRect.anchoredPosition,
            targetMapPosition,
            Time.deltaTime * smoothSpeed
        );

        // Mover el Ícono del Jugador (Rastreo fino)
        playerIcon.transform.SetAsLastSibling();
        playerIcon.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(
            playerIcon.GetComponent<RectTransform>().anchoredPosition,
            targetIconPosition,
            Time.deltaTime * smoothSpeed
        );
    }

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

        // Opcional: También eliminamos el icono del jugador si lo hemos instanciado
        if (playerIcon != null)
        {
            Destroy(playerIcon);
            playerIcon = null;
        }

        Debug.Log("MinimapManager: Iconos de mapa limpiados para regeneración.");
    }

    /// <summary>
    /// Establece la visibilidad del panel y actualiza el estado lógico.
    /// </summary>
    public void SetMapVisibility(bool visible)
    {
        if (MinimapPanel == null) return;

        isMapVisible = visible;
        MinimapPanel.SetActive(visible);
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

        // ** NUEVO: ANCLAJE AL INICIO **
        // Si el jugador está activo, forzamos el rastreo a la Sala de Inicio.
        if (DungeonGenerator.Instance != null && DungeonGenerator.Instance.GetStartingRoom() != null)
        {
            DungeonPart startingRoom = DungeonGenerator.Instance.GetStartingRoom();
            StartRoomTracking(startingRoom);
            DiscoverPart(startingRoom); // También revelamos la sala
        }

    }

    // NUEVO MÉTODO (Ya deberías tenerlo): Llamado desde RoomDiscovery.cs
    // Modificar StartRoomTracking:
    public void StartRoomTracking(DungeonPart part)
    {
        if (currentTrackedRoom != part)
        {
            currentTrackedRoom = part;
            _mapNeedsSnap = true; //  Activa el SNAP/Mini TP
            Debug.Log("Cambiando ancla del minimapa a: " + part.MapCoords);
        }
    }

    //// FUNCIÓN NUEVA: Implementa la lógica de rastreo relativo.
    //private void UpdatePlayerIconRelative(Vector3 worldPosition, Vector3 roomCenter3D, Vector2Int roomCoords2D)
    //{
    //    float mapScale = 100f;
    //    float roomSize = 50f; // <--- AUMENTAR el divisor para frenar el movimiento
    //    float worldToMapRatio = mapScale / roomSize;

    //    // 1. Calcular la posición relativa del jugador DENTRO de la sala actual
    //    Vector3 relativeToRoom = worldPosition - roomCenter3D;

    //    // 2. Calcular el desplazamiento del ícono DENTRO de la celda (rastreo fino)
    //    Vector2 fineMovement = new Vector2(
    //        relativeToRoom.x * worldToMapRatio,
    //        relativeToRoom.z * worldToMapRatio
    //    );

    //    // 3. Calcular la Posición Final: Coordenada Lógica + Desfase Fino
    //    Vector2 mapPosition = new Vector2(
    //        roomCoords2D.x * mapScale + fineMovement.x,
    //        roomCoords2D.y * mapScale + fineMovement.y
    //    );

    //    float smoothSpeed = 10f; // Controla la rapidez del suavizado

    //    playerIcon.GetComponent<RectTransform>().anchoredPosition =
    //        Vector2.Lerp(
    //            playerIcon.GetComponent<RectTransform>().anchoredPosition, // Posición actual
    //            mapPosition, // Posición objetivo (calculada)
    //            Time.deltaTime * smoothSpeed
    //        );

    //    playerIcon.transform.SetAsLastSibling();
    //    playerIcon.GetComponent<RectTransform>().anchoredPosition = mapPosition;
    //}
}