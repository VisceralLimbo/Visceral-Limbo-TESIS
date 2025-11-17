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

    private Vector3 mapWorldOrigin = Vector3.zero;

    // --- DATOS INTERNOS ---
    private Dictionary<DungeonPart, GameObject> mapIcons = new Dictionary<DungeonPart, GameObject>();
    private GameObject cachedPlayer; // Referencia al objeto del jugador

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
        // Si MinimapPanel está disponible, lo usamos como padre del ícono
        Transform parentTransform = (MinimapPanel != null) ? MinimapPanel.transform : MapContainer.transform.parent;

        if (cachedPlayer != null && playerIcon == null && PlayerIconPrefab != null && parentTransform != null)
        {
            // Instancia como hijo del contenedor estático (MinimapPanel o su padre)
            playerIcon = Instantiate(PlayerIconPrefab, parentTransform);
            playerIcon.SetActive(true);

            // ANCLAJE CLAVE: Fijar el ícono en el centro de su contenedor estático
            RectTransform rt = playerIcon.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.one * 0.5f;
            rt.anchorMax = Vector2.one * 0.5f;
            rt.pivot = Vector2.one * 0.5f;
            rt.anchoredPosition = Vector2.zero; // <--- POSICIÓN FIJA EN EL CENTRO

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

    // CORREGIR Y USAR ESTA FUNCIÓN:
    public void UpdatePlayerIcon(Vector3 worldPosition)
    {
        if (playerIcon == null) return;

        // Si la sala rastreada no está configurada, salimos, o usamos el origen del mundo
        if (currentTrackedRoom == null)
        {
            // Esto solo debería ocurrir al inicio. En ese caso, usa el origen del mundo.
            UpdatePlayerIconRelative(worldPosition, mapWorldOrigin, Vector2Int.zero);
            return;
        }

        // Si tenemos una sala, usamos su centro 3D y su coordenada lógica
        UpdatePlayerIconRelative(worldPosition,
                                 currentTrackedRoom.transform.position,
                                 currentTrackedRoom.MapCoords);
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
    }

    // NUEVO MÉTODO (Ya deberías tenerlo): Llamado desde RoomDiscovery.cs
    public void StartRoomTracking(DungeonPart part)
    {
        currentTrackedRoom = part;
    }

    // FUNCIÓN MODIFICADA: Implementa la lógica de rastreo relativo (SOLO CUADRÍCULA).
    private void UpdatePlayerIconRelative(Vector3 worldPosition, Vector3 roomCenter3D, Vector2Int roomCoords2D)
    {
        if (MapContainer == null || playerIcon == null) return;

        float mapScale = 100f;

        // --- CAMBIO CLAVE: IGNORAR EL MOVIMIENTO FINO ---
        // La posición del jugador en el mapa es simplemente el centro de la celda de la sala actual.
        // Ya no necesitamos 'relativeToRoom' ni 'fineMovement'.

        // 1. Calcular la Posición Lógica (la coordenada central de la sala actual en el minimapa).
        Vector2 playerMapPosition = new Vector2(
            roomCoords2D.x * mapScale,
            roomCoords2D.y * mapScale
        );

        // 2. Mover el MapContainer: Para que el jugador quede estático en el centro (0,0), 
        // el contenedor debe moverse al negativo de la posición lógica del jugador.
        Vector2 targetContainerPosition = -playerMapPosition;

        // 3. Mover el MapContainer usando Lerp para un "salto" suave de una sala a otra.
        MapContainer.anchoredPosition =
            Vector2.Lerp(
                MapContainer.anchoredPosition, // Posición actual del contenedor
                targetContainerPosition,       // Posición objetivo (el centro de la sala actual)
                Time.deltaTime * 10f // Velocidad de movimiento del mapa
            );

        // El playerIcon debe permanecer en el centro del MinimapPanel (generalmente Vector2.zero) 
        // para estar fijo. Su posición no se toca aquí.
    }
}