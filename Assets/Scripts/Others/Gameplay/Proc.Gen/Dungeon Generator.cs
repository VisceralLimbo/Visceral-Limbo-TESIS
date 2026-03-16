using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public class DungeonGenerator : MonoBehaviour
{
    /// <summary>
    /// Devuelve la sala de inicio.
    /// </summary>
    public DungeonPart GetStartingRoom() => StartingRoom; // Asumiendo que 'StartingRoom' es una variable privada

    /// <summary>
    /// Instancia ACTUAL del generador, NO SE MANTIENE ENTRE ESCENAS, OSEA NO ES SINGLETON
    /// </summary>
    public static DungeonGenerator Instance { get; private set; }

    [Header("References")]
    /// <summary>
    /// El punto de inicio de la mazmorra
    /// </summary>
    [SerializeField]
    private GameObject Entrance;

    /// <summary>
    /// La pool de posibles habitaciones de la mazmorra
    /// </summary>
    [SerializeField]
    private List<GameObject> RoomPrefabs;

    /// <summary>
    /// La pool de posibles habitaciones especiales de la mazmorra
    /// </summary>
    [SerializeField]
    private List<GameObject> SpecialRoomPrefabs;

    /// <summary>
    /// La Pool de posibles habitaciones de jefes.
    /// </summary>
    [SerializeField]
    private List<GameObject> BossRoomPrefabs;

    /// <summary>
    /// La Pool de posibles pasillos de la mazmorra
    /// </summary>
    [SerializeField]
    private List<GameObject> HallwayPrefabs;

    /// <summary>
    /// El prefab de la puerta
    /// </summary>
    [SerializeField]
    private GameObject DoorOBJ;

    /// <summary>
    /// la lista de habitaciones generadas
    /// </summary>
    public List<DungeonPart> generatedRooms = new List<DungeonPart>();

    /// <summary>
    /// la lista de pasillos generados
    /// </summary>
    public List<DungeonPart> generatedHallways = new List<DungeonPart>();

    /// <summary>
    /// la lista de habitaciones especiales generadas
    /// </summary>
    public List<DungeonPart> generatedSpecialRooms = new List<DungeonPart>();



    [Space]
    [Header("Variables")]


    /// <summary>
    /// la cantidad de habitaciones regulares de la mazmorra
    /// </summary>
    [SerializeField]
    private int RegularRoomCount;

    /// <summary>
    /// la cantidad de habitaciones especiales de la mazmorra
    /// </summary>
    [SerializeField]
    private int SpecialRoomCount;

    /// <summary>
    /// El layerMask de las habitaciones
    /// </summary>
    [SerializeField]
    private LayerMask RoomMask;

    /// <summary>
    /// Si la mazmorra se ha finalizado de crear
    /// </summary>
    [SerializeField]
    private bool IsGenerated = false;

    /// <summary>
    /// Esta variable controla la máxima cantidad de intentos que hace
    /// el script para generar una sala.
    /// usar valores relativamente altos
    /// </summary>
    [SerializeField]
    private int TotalTriesPerRoomGeneration = 100;

    private bool GenerateHallway = true;
    private bool GeneratedBossRoom = false;

    private DungeonPart StartingRoom;
    private DungeonPart BossRoom;

    [Space]
    [Header("Debug")]
    [SerializeField] bool SlowGen = false;
    [SerializeField] float SlowGenSpeed;
    [SerializeField] bool Regen = false;
    [SerializeField] bool IsRegenerating;
    public List<DungeonPart> AllParts = new List<DungeonPart>();

    [Space]
    [Header("Door Glow Colors")]
    [SerializeField] private Color SpecialRoomGlowColor = Color.magenta; //violetaxd
    [SerializeField] private Color BossRoomGlowColor = Color.red; // rojoxd

    [Header("Progression Settings")]
    [SerializeField] private int baseRegularRoomCount = 5; // salas iniciales del lvl 1
    [SerializeField] private int roomsPerLevelIncrease = 3; // las q se agregan por lvl

    public Action<float> GenerationValue;

    public Action OnSuccessfulGeneration;

    public Action OnUnSuccessfulGeneration;

    [Header("Hack")]
    [SerializeField] private KeyCode SkipToBossKey = KeyCode.B; // b para el tp

    private GameObject BossDoorObject; // guardo la puerta del boss

    private void Awake()
    {
        // no hago un singleton porque la idea es que se muera el dungeon generator al 
        // pasar de nivel, así es más sencillo crear otros tipos de niveles
        Instance = this;
    }

    private void Start()
    {
        // leo el nivel del gamemanager
        if (GameManager.Instance != null)
        {
            // ahora seria lvl1 = 5 salas (tengo q tocar las q estan puestas en el inscpector xd)
            RegularRoomCount = baseRegularRoomCount + (roomsPerLevelIncrease * (GameManager.Instance.currentLevel - 1));
            // aumento las especiales tmb
            SpecialRoomCount += GameManager.Instance.currentLevel / 2;
        }

        generatedRooms = new List<DungeonPart>();
        //comenzar a crear salas
        StartGeneration();
    }

    private void Update()
    {
        if (Regen)
        {
            Regen = false;
            StartCoroutine(Regenerate());
            OnUnSuccessfulGeneration?.Invoke();
        }

        // b para el tp
        if (Input.GetKeyDown(SkipToBossKey) && IsGenerated)
        {
            TeleportToBossDoor();
        }
    }

    private IEnumerator Regenerate()
    {
        if (IsRegenerating)
        {
            yield break;
        }

        IsRegenerating = true;

        foreach (DungeonPart room in generatedRooms)
        {
            if (room != null) Destroy(room.gameObject);
        }
        generatedRooms.Clear();

        foreach (DungeonPart hall in generatedHallways)
        {
            if (hall != null) Destroy(hall.gameObject);
        }
        generatedHallways.Clear();

        foreach (DungeonPart special in generatedSpecialRooms)
        {
            if (special != null) Destroy(special.gameObject);
        }
        generatedSpecialRooms.Clear();

        if (BossRoom != null)
        {
            Destroy(BossRoom.gameObject);
            GeneratedBossRoom = false;
            BossRoom = null;
        }


        AllParts.Clear();

        //esperamos un tiempo para que Unity procese OnDestroy()
        yield return null;

        IsRegenerating = false;

        //ahora que ya limpiamos todo, comenzamos con la generacion
        StartGeneration();
    }



    public void StartGeneration()
    {
        StartCoroutine(GenerateDungeon());
        //generateSpecials

    }

    IEnumerator GenerateDungeon()
    {



        // ponemos la sala inicial
        if (RegularRoomCount > 0)
        {
            GameObject entranceObj = Instantiate(Entrance, this.transform.position, this.transform.rotation);
            if (entranceObj.TryGetComponent(out DungeonPart part))
            {

                generatedRooms.Add(part);
                GenerateHallway = true;
                StartingRoom = part;
            }
        }

        // usamos un while loop para garantizar que tenemos la cantidad de salas correctas
        while (generatedRooms.Count < RegularRoomCount)
        {
            bool placementSuccessful = false;
            DungeonPart partToPlace = null; // la nueva pieza a colocar


            // Watchdog de cantidad de veces que PUEDE tratar de poner una pieza
            for (int i = 0; i < TotalTriesPerRoomGeneration; i++)
            {

                // 1. DECLARACIÓN DE VARIABLES FUERA DE LOS BLOQUES IF
                // Esto soluciona "No se puede usar la variable local 'sourceEntryPoint' antes de declararla"
                DungeonPart sourceRoom = null;
                DungeonEntryPoint sourceEntryPoint = null;
                Vector2Int newMapCoords = Vector2Int.zero; // <-- Declarada aquí


                // 1. SELECCIONAMOS UNA PIEZA EXISTENTE PARA COLOCARLE LA NUEVA
                // ===================================================
                List<DungeonPart> SourcePool;


                if (GenerateHallway)
                {
                    SourcePool = generatedRooms.FindAll(x => x.HasAvailableEntryPoint(out _));
                }
                else
                {
                    SourcePool = generatedHallways.FindAll(x => x.HasAvailableEntryPoint(out _));
                }

                // CATCH! evitamos referenciar un pool nulo
                if (SourcePool.Count == 0)
                {
                    continue;
                }

                int randomLinkRoomIndex = UnityEngine.Random.Range(0, SourcePool.Count);
                sourceRoom = SourcePool[randomLinkRoomIndex]; // <-- ASIGNACIÓN AQUÍ

                if (!sourceRoom.HasAvailableEntryPoint(out sourceEntryPoint))
                {
                    Debug.LogWarning("room has no entry points" + sourceRoom.name);
                    //SourcePool.Remove(sourceRoom);
                    continue; // esta sala no tiene puntos abiertos, probemos otro lugar
                }

                // 2. CREAMOS LA PIEZA NUEVA Y LA COLOCAMOS
                // =====================================================
                GameObject newPartObject;
                if (GenerateHallway)
                {
                    int randomHallwayIndex = UnityEngine.Random.Range(0, HallwayPrefabs.Count);
                    newPartObject = Instantiate(HallwayPrefabs[randomHallwayIndex]);
                }
                else
                {
                    int randomIndex = Random.Range(0, RoomPrefabs.Count);
                    newPartObject = Instantiate(RoomPrefabs[randomIndex]);
                }

                if (!newPartObject.TryGetComponent(out partToPlace)
                                                   || !partToPlace.HasAvailableEntryPoint
                                                  (out DungeonEntryPoint newPartEntryPoint))
                {
                    Debug.LogWarning("We failed to place a room" + sourceRoom.name);
                    Destroy(newPartObject); // destruimos la pieza ERRONEA

                    sourceEntryPoint.SetOccupied(null, false);
                    sourceRoom.UnuseEntryPoint(sourceEntryPoint);
                    continue; // el prefab usado no era viable, por ende limpiamos lo que hicimos
                              // y probamos de nuevo
                }

                // 3. ALINEAMIENTO Y CHEQUEO DE COLISIONES
                // =====================================================
                AlignRooms(sourceRoom.transform, partToPlace.transform, sourceEntryPoint.transform, newPartEntryPoint.transform);

                if (HandleIntersection(partToPlace))
                {
                    Debug.Log("Oops, the room intersects" + sourceRoom.name);

                    sourceRoom.UnuseEntryPoint(sourceEntryPoint);
                    // LIBERAR LOS PUNTOS OCUPADOS
                    sourceEntryPoint.SetOccupied(null, false);
                    newPartEntryPoint.SetOccupied(null, false);
                    partToPlace.UnuseEntryPoint(newPartEntryPoint);

                    // HAY INTERSECCION! LIMPIAMOS LO HECHO Y VOLVEMOS A PROBAR
                    Destroy(newPartObject);
                    partToPlace = null;
                    continue;
                }
                else
                {

                    // 4. GENERACION EXITOSA
                    // =====================================================
                    placementSuccessful = true;

                    // bloqueamos los puntos de accesso usados
                    sourceEntryPoint.SetOccupied(newPartEntryPoint.GetOwner(), true);
                    newPartEntryPoint.SetOccupied(sourceEntryPoint.GetOwner(), true);

                    // creamos una puerta en el punto ocupado
                    var Door = Instantiate(DoorOBJ);

                    //Ahora le decimos a la puerta a quien le pertenece
                    if (Door.TryGetComponent(out DoorScript DoorScript))
                    {
                        //Si estamos generando un pasillo, entonces nuestra habitacion
                        //asignada sería el Source
                        if (GenerateHallway)
                        {
                            if (sourceRoom.TryGetComponent(out RoomSpawnerManager Manager))
                            {
                                // como la pieza nueva es un pasillo, esta puerta estará ubicada en el
                                // snap point de la pieza Source
                                DoorScript.Initialize(Manager, sourceEntryPoint);
                            }
                            else
                            {
                                Debug.LogError("Visceral Limbo Proc.Gen: " + sourceRoom.name + " no tiene script de RoomManager");
                            }
                        }
                        // Si no
                        // Entonces acabamos de generar una habitacion.
                        // por ende, nuestro partToPlace será la habitacion
                        else if (!GenerateHallway)
                        {
                            if (partToPlace.TryGetComponent(out RoomSpawnerManager manager))
                            {
                                // como la pieza nueva es una sala, esta puerta estará ubicada en el
                                // snap point de la pieza nueva
                                DoorScript.Initialize(manager, newPartEntryPoint);
                            }
                            else
                            {
                                Debug.LogError("Visceral Limbo Proc.Gen: " + partToPlace.name + " no tiene script de RoomManager");
                            }
                        }
                    }

                    // añadimos la parte nueva al pool de salas generadas
                    if (GenerateHallway)
                    {
                        generatedHallways.Add(partToPlace);
                    }
                    else
                    {
                        generatedRooms.Add(partToPlace);
                    }

                    GenerateHallway = !GenerateHallway; // flip flop de sala / pasillo

                    newPartObject.transform.SetParent(this.transform, true);

                    //levantamos los eventos de generacion
                    GenerationEvents();

                    break; // salimos del loop para colocar una nueva pieza
                }
            }

            // si consumimos todos los intentos posibles y no generamos nada
            // salimos del LOOP para evitar stack overflow
            if (!placementSuccessful)
            {
                Debug.LogWarning("Dungeon generation failed. Could not find a valid placement after " + TotalTriesPerRoomGeneration + " attempts.");
                if (!IsRegenerating) StartCoroutine(Regenerate());
                break;
            }

            if (SlowGen)
            {
                yield return new WaitForSeconds(SlowGenSpeed);
            }
        }

        // =====================================================
        //
        //  AHORA QUE FINALIZAMOS LA ESTRUCTURA BASE DE LA MAZMORRA
        //  VAMOS A COLOCAR LAS SALAS ESPECIALES
        //
        // =====================================================
        StartCoroutine(GenerateSpecialRooms());

    }

    IEnumerator GenerateSpecialRooms()
    {

        print("Generating Special Rooms...");
        while (generatedSpecialRooms.Count < SpecialRoomCount)
        {
            //comenzamos asignando las nuevas salas
            bool placementSuccessful = false;
            DungeonPart partToPlace = null; // la nueva sala especial
            // bucle de intentos maximos
            for (int I = 0; I < TotalTriesPerRoomGeneration; I++)
            {


                // 1: CREAMOS UN NUEVO SOURCE POOL
                // VAMOS A SALIR CON LA IDEA DE QUE NUESTRAS SALAS ESPECIALES
                // PUEDEN CONECTARSE CON CUALQUIER OTRO TIPO DE SALA (MENOS ESPECIALES Y JEFE)
                // ============================================================================
                List<DungeonPart> SourcePool = new List<DungeonPart>();

                List<DungeonPart> FilteredRoomList = generatedRooms.FindAll(x => x.HasAvailableEntryPoint(out _));

                List<DungeonPart> FilteredHallwayList = generatedRooms.FindAll(x => x.HasAvailableEntryPoint(out _));

                SourcePool.AddRange(FilteredRoomList);
                SourcePool.AddRange(FilteredHallwayList);

                // CHEQUEAMOS QUE HAYAN POSIBLES CONECCIONES EN TODA LA MAZMORRA
                // 
                if (SourcePool.Count <= 0)
                {
                    Debug.LogWarning("Visceral Proc.Gen : no hay posibles espacios en la mazmorra para salas especiales");
                    if (!IsRegenerating) StartCoroutine(Regenerate());
                    break;
                }

                int randomSourceSeed = Random.Range(0, SourcePool.Count - 1);
                if (randomSourceSeed < 0 || randomSourceSeed > SourcePool.Count)
                {
                    Debug.LogWarning("Visceral Proc.Gen : no hay posibles espacios en la mazmorra para salas especiales");
                    if (!IsRegenerating) StartCoroutine(Regenerate());
                    break;
                }

                DungeonPart SourceRoom = SourcePool[randomSourceSeed]; // <-- Fuente

                // source pool deberia de contener las salas y pasillos disponibles
                if (!SourceRoom.HasAvailableEntryPoint(out DungeonEntryPoint sourceEntryPoint))
                {
                    Debug.Log("room has no entry points" + SourceRoom.name);
                    //SourcePool.Remove(sourceRoom);
                    continue; // esta sala no tiene puntos abiertos, probemos otro lugar
                }

                // =====================================================
                //
                // 2. CREAMOS LA PIEZA NUEVA Y LA COLOCAMOS
                //
                // =====================================================

                GameObject newPartObject;

                int RandomSpecialPartSeed = Random.Range(0, SpecialRoomPrefabs.Count);

                newPartObject = Instantiate(SpecialRoomPrefabs[RandomSpecialPartSeed]);

                // chequeamos si la pieza generada tiene Script de DungeonPart o
                // tiene espacios disponibles
                if (!newPartObject.TryGetComponent(out DungeonPart NewPart) ||
                    !NewPart.HasAvailableEntryPoint(out DungeonEntryPoint NewPartEntryPoint))
                {

                    Debug.Log("We failed to place a room" + SourceRoom.name);
                    Destroy(newPartObject); // destruimos la pieza ERRONEA

                    sourceEntryPoint.SetOccupied(null, false);
                    SourceRoom.UnuseEntryPoint(sourceEntryPoint);
                    continue; // el prefab usado no era viable, por ende limpiamos lo que hicimos
                              // y probamos de nuevo
                }


                // 3. ALINEAMIENTO Y CHEQUEO DE COLISIONES
                // =====================================================
                AlignRooms(SourceRoom.transform, NewPart.transform, sourceEntryPoint.transform, NewPartEntryPoint.transform);

                if (HandleIntersection(NewPart))
                {
                    Debug.Log("Oops, the special room intersects" + SourceRoom.name);

                    SourceRoom.UnuseEntryPoint(sourceEntryPoint);
                    // LIBERAR LOS PUNTOS OCUPADOS
                    sourceEntryPoint.SetOccupied(null, false);
                    NewPartEntryPoint.SetOccupied(null, false);
                    NewPart.UnuseEntryPoint(NewPartEntryPoint);

                    // HAY INTERSECCION! LIMPIAMOS LO HECHO Y VOLVEMOS A PROBAR
                    Destroy(newPartObject);
                    partToPlace = null;
                    continue;
                }
                else
                {

                    // 4. GENERACION EXITOSA
                    // =====================================================
                    placementSuccessful = true;

                    // bloqueamos los puntos de accesso usados
                    sourceEntryPoint.SetOccupied(NewPartEntryPoint.GetOwner(), true);
                    NewPartEntryPoint.SetOccupied(sourceEntryPoint.GetOwner(), true);

                    // creamos una puerta en el punto ocupado
                    var Door = Instantiate(DoorOBJ, sourceEntryPoint.transform.position, sourceEntryPoint.transform.rotation);
                    Door.transform.SetParent(sourceEntryPoint.transform, true);

                    // añado el  doorflow (nuevo script) a la sala del cofre
                    Door.AddComponent<DoorGlow>();

                    // TESTEAMOS SI EL SOURCE ES UNA SALA,LA INICIAMOS
                    if (SourceRoom.RoomType == DungeonPart.DungeonPartType.Room)
                    {
                        if (SourceRoom.TryGetComponent(out RoomSpawnerManager Manager))
                        {
                            if (Door.TryGetComponent(out DoorScript DoorSC))
                            {
                                DoorSC.Initialize(Manager, sourceEntryPoint);
                                DoorSC.ShouldGenerateEvents(false);

                                // color de la puerta de cofres
                                if (Door.TryGetComponent(out DoorGlow glowSC))
                                {
                                    glowSC.SetGlowColor(SpecialRoomGlowColor);
                                }
                            }
                        }
                    }
                    else if (SourceRoom.RoomType == DungeonPart.DungeonPartType.Hallway)
                    {
                        if (Door.TryGetComponent(out DoorScript DoorSC))
                        {
                            DoorSC.ShouldGenerateEvents(false);
                        }


                    }




                    // añadimos la sala especial al listado de salas generadas
                    generatedSpecialRooms.Add(NewPart);

                    NewPart.transform.SetParent(this.transform, true);
                    placementSuccessful = true;

                    //levantamos los eventos de generacion
                    GenerationEvents();
                    break;
                }

            }
            if (placementSuccessful == false)
            {
                Debug.LogWarning("Dungeon generation failed. Could not find a valid placement for special room after : " + TotalTriesPerRoomGeneration + " attempts.");
                if (!IsRegenerating) StartCoroutine(Regenerate()); // regeneramos la mazmorra desde 0
                break;
            }

            if (SlowGen)
            {
                yield return new WaitForSeconds(SlowGenSpeed);
            }

        }

        // =================================================
        //
        // 5: COMO YA GENERAMOS TODAS LAS SALAS DESEADAS, ENTONCES 
        // BLOQUEAMOS LAS CONECCIONES NO USADAS
        //
        // =================================================
        AllParts.AddRange(generatedHallways);
        AllParts.AddRange(generatedRooms);
        AllParts.AddRange(generatedSpecialRooms);


        // =====================================================
        //
        // 6: VAMOS A GENERAR LA SALA DE JEFE
        //
        // =======================================================

        StartCoroutine(GenerateBossRoom());
    }

    /// <summary>
    /// Generar sala del jefe
    /// </summary>
    /// <returns></returns>
    IEnumerator GenerateBossRoom()
    {

        print("Generating Boss Room");

        // =====================================================================
        //
        // Paso 1: Seleccionamos todos los puntos viables para la generacion procedural
        //
        // ===================================================================

        List<DungeonEntryPoint> viableEntryPoints = new List<DungeonEntryPoint>();

        foreach (DungeonPart Entry in generatedHallways)
        {
            if (Entry == null)
            {
                continue;
            }

            List<DungeonEntryPoint> Entrypoints = Entry.GetAvailableEntryPoints();

            if (Entrypoints != null && Entrypoints.Count > 0)
            {
                viableEntryPoints.AddRange(Entrypoints);
            }
        }

        // ==================================================================
        //
        // Paso 2: Ordenar la lista por distancia de más lejos a menos lejos.
        //
        // ==================================================================

        var orderedEntryPoints = viableEntryPoints.OrderByDescending
            (p => Vector3.Distance(p.transform.position, StartingRoom.transform.position))
            .ToList();

        // ==================================================
        //
        // Paso 3: iterar por lista buscando el punto viable
        //
        // ==================================================

        bool placementSuccessful = false;

        foreach (DungeonEntryPoint SourcePoint in orderedEntryPoints)
        {
            int bossRoomSeed = Random.Range(0, BossRoomPrefabs.Count);

            //obtenemos una referencia del dueño del entrypoint
            DungeonPart SourceRoom = SourcePoint.GetOwner();

            GameObject NewBossRoom = Instantiate(BossRoomPrefabs[bossRoomSeed]);

            if (!NewBossRoom.TryGetComponent(out DungeonPart BossPart) ||
                !BossPart.HasAvailableEntryPoint(out DungeonEntryPoint BossEntryPoint))
            {
                Debug.LogError("Visceral Error: proc.Gen: " + NewBossRoom.name + " no tiene componente de DungeonPart o no tiene EntryPoints viables");
                Destroy(NewBossRoom);
                continue; // prefab invalido
            }

            // =================================================================
            //
            // Paso 4: Alineamos la sala con el punto
            //
            // =================================================================

            AlignRooms(SourceRoom.transform, NewBossRoom.transform, SourcePoint.transform, BossEntryPoint.transform);

            // ==================================================================
            //
            // Paso 5: Chequeamos colisiones
            //
            // ==================================================================
            if (HandleIntersection(BossPart))
            {
                // hay colision
                Destroy(NewBossRoom);
                Debug.LogWarning("Visceral Warning: Proc. Gen. sala de jefe colisiona con la mazmorra");
                continue;
            }
            else
            {
                placementSuccessful = true;
                BossRoom = BossPart;
                GeneratedBossRoom = true;

                SourcePoint.SetOccupied(BossEntryPoint.GetOwner(), true);
                BossEntryPoint.SetOccupied(SourcePoint.GetOwner(), true);

                // =======================================================
                //
                // Paso 6: creamos una puerta
                //
                // =======================================================

                GameObject Door = Instantiate(DoorOBJ, SourcePoint.transform.position, SourcePoint.transform.rotation);
                BossDoorObject = Door; // guardo la refe aca

                Door.transform.SetParent(SourcePoint.transform, true);

                Door.AddComponent<DoorGlow>();

                Door.TryGetComponent(out DoorScript DoorSC);

                if (BossPart.gameObject.TryGetComponent(out RoomSpawnerManager RoomMan))
                {
                    DoorSC.Initialize(RoomMan, BossEntryPoint);
                }
                else
                {
                    Debug.LogWarning("Visceral Warning: BossPart no reporta RoomManager para asignar a la puerta");
                }

                // color puerta jefe
                if (Door.TryGetComponent(out DoorGlow glowSC))
                {
                    glowSC.SetGlowColor(BossRoomGlowColor);
                }

                // hacemos que la sala del jefe sea hijo del dungeonGenerator
                // evita bloating en el hierarchy
                BossPart.transform.SetParent(this.transform, true);

                AllParts.Add(BossPart);

                //levantamos los eventos de generacion
                GenerationEvents();

                break; // salimos del foreach 
            }


        }

        // ==================================================================
        //
        // Paso 7: manejo de fallo
        //
        // ==================================================================

        if (placementSuccessful == false)
        {
            Debug.LogWarning("Visceral Warning: error al poner la sala del jefe, no hay lugar posible. regenerando");
            if (!IsRegenerating) StartCoroutine(Regenerate());
        }
        else
        {
            // genero de manera exitosa.

            print("Boss Room generada");
            FillEmptyEntries();

        }

        yield return null;
    }


    private void FillEmptyEntries()
    {
        generatedRooms.ForEach(room => room.FillEmptyPoints());
        generatedHallways.ForEach(hallway => hallway.FillEmptyPoints());
        BossRoom.FillEmptyPoints();
        print("Dungeon generation finished!");
        IsGenerated = true;

        //levantamos los eventos de generacion
        GenerationEvents();

    }


    /// <summary>
    /// Esta funcion alinea las habitaciones
    /// </summary>
    /// <param name="Room1">transform de la habitacion 1</param>
    /// <param name="Room2">transform de la habitacion 2</param>
    /// <param name="Room1EntryPoint">transform de la entrada de la habitacion 1</param>
    /// <param name="Room2EntryPoint">transform de la entrada de la habitacion 2</param>
    private void AlignRooms(Transform Room1, Transform Room2, Transform Room1EntryPoint, Transform Room2EntryPoint)
    {
        // Get the target rotation for the new room's entry point (opposite of the first)
        Quaternion targetRotation = Quaternion.LookRotation(-Room1EntryPoint.forward, Room1EntryPoint.up);

        // Calculate the difference between the new room's current rotation and its desired rotation
        Quaternion rotationDifference = targetRotation * Quaternion.Inverse(Room2EntryPoint.rotation);

        // Apply the rotation to the new room
        Room2.rotation *= rotationDifference;

        // Now that it's rotated correctly, calculate the position offset and apply it
        Vector3 offset = Room1EntryPoint.position - Room2EntryPoint.position;
        Room2.position += offset;

        // Force physics engine to update transforms immediately for intersection checks
        Physics.SyncTransforms();
    }


    /// <summary>
    /// Esta funcion se encarga de chequear si hay interseccion entre salas
    /// </summary>
    /// <param name="Part"> la sala que queremos chequear por intersecciones</param>
    /// <returns></returns>
    private bool HandleIntersection(DungeonPart Part)
    {
        List<DungeonPart> AllParts = new List<DungeonPart>();
        AllParts.AddRange(generatedRooms);
        AllParts.AddRange(generatedHallways);
        AllParts.AddRange(generatedSpecialRooms);

        foreach (var ExistingPart in AllParts)
        {
            if (ExistingPart == Part)
            {
                continue; // no nos chequemos a nosotros xd
            }

            // por cada collider en la pieza a poner
            foreach (var partCol in Part._Colliders)
            {
                //por cada colider en TODAS las piezas existentes
                foreach (var otherCol in ExistingPart._Colliders)
                {
                    // si la pieza a colocar intersecta con collider existente
                    if (partCol.bounds.Intersects(otherCol.bounds))
                    {
                        Debug.LogWarning("[visceral Proc.Gen]pieza intersecta ");
                        return true;
                    }


                }
            }

        }

        //si llegamos acá, entonces no hay intersecciones
        return false;
    }

    /// <summary>
    /// Esta funcion de encarga de reintentar de ubicar habitaciones
    /// </summary>
    /// <param name="ItemToPlace"> la habitacion a correr</param>
    /// <param name="DoorToPlace"> La puerta a reubicar</param>
    private void RetryPlacement(DungeonPart ItemToPlace, GameObject DoorToPlace)
    {
        // la sala que vamos a usar para generar un vecino
        DungeonPart RandomGeneratedRoom = null;
        DungeonEntryPoint EntryPoint1 = null;

        int TotalTries = 100; // watchdog
        int RetryIndex = 0;


        while (RandomGeneratedRoom == null && RetryIndex < TotalTries)
        {
            //seleccionamos una sala a testear.
            int RandomLinkRoomIndex = UnityEngine.Random.Range(0, generatedRooms.Count - 1);
            DungeonPart RoomToTest = generatedRooms[RandomLinkRoomIndex];

            //chequeamos si la sala tiene espacios abiertos de coneccion
            if (RoomToTest.HasAvailableEntryPoint(out EntryPoint1))
            {
                RandomGeneratedRoom = RoomToTest;
                break;
            }
            RetryIndex++;
        }

        // chequeamos que la sala que queremos poner tiene una salida abierta
        if (ItemToPlace.HasAvailableEntryPoint(out DungeonEntryPoint EntryPoint2))
        {
            //ubicamos la puerta
            DoorToPlace.transform.position = EntryPoint1.transform.position;
            DoorToPlace.transform.rotation = EntryPoint1.transform.rotation;

            //alineamos la sala
            AlignRooms(RandomGeneratedRoom.transform, ItemToPlace.transform
                , EntryPoint1.transform, EntryPoint2.transform);

            // si la sala tiene interseccion
            if (HandleIntersection(ItemToPlace))
            {
                //liberamos los puntos de entrada
                ItemToPlace.UnuseEntryPoint(EntryPoint2);
                RandomGeneratedRoom.UnuseEntryPoint(EntryPoint1);

                //llamado recursivo
                RetryPlacement(ItemToPlace, DoorToPlace);
            }

        }


    }



    public List<DungeonPart> GetRooms() => generatedRooms;

    public bool HasFinishedGeneration() => IsGenerated;


    private void GenerationEvents()
    {
        var FullRoomTarget = RegularRoomCount + SpecialRoomCount + 1f;
        var TotalRooms = generatedRooms.Count + SpecialRoomCount;

        GenerationValue?.Invoke((TotalRooms / FullRoomTarget));

        if (IsGenerated)
        {
            OnSuccessfulGeneration?.Invoke();
        }
    }

    private Vector2Int GetCoordinateOffset(DungeonEntryPoint entryPoint)
    {
        // El vector de dirección del punto de conexión en el mundo
        Vector3 forward = entryPoint.transform.forward;
        // Un umbral más alto para asegurar que el vector esté alineado con un eje
        float threshold = 0.8f;

        // El minimapa usa X (Horizontal) e Y (Vertical), que corresponden a los ejes X y Z del mundo 3D.

        // 1. CHEQUEAR EJE Z DEL MUNDO (Corresponde al eje Y del mapa)
        if (Mathf.Abs(forward.z) > threshold)
        {
            // Si forward.z es positivo (hacia adelante en 3D), movemos el mapa +1 en Y.
            // Si forward.z es negativo (hacia atrás en 3D), movemos el mapa -1 en Y.
            // Usamos Mathf.Sign() para obtener 1 o -1 de forma segura.
            int yOffset = (int)Mathf.Sign(forward.z);
            return new Vector2Int(0, yOffset);
        }

        // 2. CHEQUEAR EJE X DEL MUNDO (Corresponde al eje X del mapa)
        if (Mathf.Abs(forward.x) > threshold)
        {
            // Si forward.x es positivo (hacia la derecha en 3D), movemos el mapa +1 en X.
            // Si forward.x es negativo (hacia la izquierda en 3D), movemos el mapa -1 en X.
            int xOffset = (int)Mathf.Sign(forward.x);
            return new Vector2Int(xOffset, 0);
        }

        // Si la dirección no está alineada (lo cual no debería ocurrir con mazmorras de cuadrícula)
        Debug.LogError($"Punto de entrada con orientación ambigua: {forward}");
        return Vector2Int.zero;
    }

    private void TeleportToBossDoor()
    {
        if (BossDoorObject == null)
        {
            Debug.LogError("no agarre la refe de la puerta");
            return;
        }

        // busco playermovent
        Player_Movement playerMovement = FindObjectOfType<Player_Movement>();

        if (playerMovement != null)
        {
            // pos
            Vector3 targetPos = BossDoorObject.transform.position + (BossDoorObject.transform.forward * 2f) + Vector3.up;

            // tp del kcc usando al funcion q ya esta ahi
            playerMovement.SetCharacterPosition(targetPos, true);

            // roto q el player vea a la puerta
            playerMovement.transform.LookAt(BossDoorObject.transform.position);
        }
    }
}
