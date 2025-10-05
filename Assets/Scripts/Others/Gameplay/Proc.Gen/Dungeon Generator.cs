using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System.Linq;

public class DungeonGenerator : MonoBehaviour
{
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


    [Space]
    [Header("Debug")]
    [SerializeField] bool SlowGen = false;
    [SerializeField] float SlowGenSpeed;
    [SerializeField] bool Regen = false;
    [SerializeField] bool IsRegenerating;
    public List<DungeonPart> AllParts = new List<DungeonPart>();

    private void Awake()
    {
        // no hago un singleton porque la idea es que se muera el dungeon generator al 
        // pasar de nivel, así es más sencillo crear otros tipos de niveles
        Instance = this;
    }

    private void Start()
    {
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
        }
    }
    private IEnumerator Regenerate()
    {
        if (IsRegenerating)
        {
            yield break;
        }

        IsRegenerating= true;

        foreach (DungeonPart room in generatedRooms)
        {
            if(room !=null) Destroy(room.gameObject);
        }
        generatedRooms.Clear();

        foreach (DungeonPart hall in generatedHallways)
        {
            if(hall != null)Destroy(hall.gameObject);
        }
        generatedHallways.Clear();

        foreach (DungeonPart special in generatedSpecialRooms)
        {
            if(special != null)Destroy(special.gameObject);
        }
        generatedSpecialRooms.Clear();

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
        
            print("Generating dungeon...");

            // ponemos la sala inicial
            if (RegularRoomCount > 0)
            {
                GameObject entranceObj = Instantiate(Entrance, this.transform.position, this.transform.rotation);
                if (entranceObj.TryGetComponent(out DungeonPart part))
                {
                    generatedRooms.Add(part);
                    GenerateHallway = true;
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
                // 1. SELECCIONAMOS UNA PIEZA EXISTENTE PARA COLOCARLE LA NUEVA
                // ===================================================
                    List<DungeonPart> SourcePool;
                    

                    if (GenerateHallway)
                    {
                       SourcePool = generatedRooms.FindAll(x=> x.HasAvailableEntryPoint(out _));
                    }
                    else
                    {
                       SourcePool = generatedHallways.FindAll(x => x.HasAvailableEntryPoint(out _));
                    }

                    // CATCH! evitamos referenciar un pool nulo
                    if(SourcePool.Count == 0)
                    {
                       continue;
                    }

                    int randomLinkRoomIndex = Random.Range(0,SourcePool.Count);
                    DungeonPart sourceRoom = SourcePool[randomLinkRoomIndex];

                    if (!sourceRoom.HasAvailableEntryPoint(out DungeonEntryPoint sourceEntryPoint))
                    {
                        Debug.Log("room has no entry points" + sourceRoom.name );
                        //SourcePool.Remove(sourceRoom);
                        continue; // esta sala no tiene puntos abiertos, probemos otro lugar
                    }

                    // 2. CREAMOS LA PIEZA NUEVA Y LA COLOCAMOS
                    // =====================================================
                    GameObject newPartObject;
                    if (GenerateHallway)
                    {
                        int randomHallwayIndex = Random.Range(0, HallwayPrefabs.Count);
                        newPartObject = Instantiate(HallwayPrefabs[randomHallwayIndex]);
                    }
                    else
                    {
                        int randomIndex = Random.Range(0, RoomPrefabs.Count);
                        newPartObject = Instantiate(RoomPrefabs[randomIndex]);
                    }

                    if (!newPartObject.TryGetComponent(out partToPlace) 
                                                       ||!partToPlace.HasAvailableEntryPoint
                                                      (out DungeonEntryPoint newPartEntryPoint))
                    {
                        Debug.Log("We failed to place a room" + sourceRoom.name);
                        Destroy(newPartObject); // destruimos la pieza ERRONEA

                        sourceEntryPoint.SetOccupied(false);
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
                        sourceEntryPoint.SetOccupied(false);
                        newPartEntryPoint.SetOccupied(false);
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
                        sourceEntryPoint.SetOccupied(true);
                        newPartEntryPoint.SetOccupied(true);

                        // creamos una puerta en el punto ocupado
                        var Door = Instantiate(DoorOBJ, sourceEntryPoint.transform.position, sourceEntryPoint.transform.rotation);
                        Door.transform.SetParent(sourceEntryPoint.transform, true);

                        //Ahora le decimos a la puerta a quien le pertenece
                        if(Door.TryGetComponent(out DoorScript DoorScript))
                        {
                            //Si estamos generando un pasillo, entonces nuestra habitacion
                            //asignada sería el Source
                            if (GenerateHallway)
                            {
                                if(sourceRoom.TryGetComponent(out RoomSpawnerManager Manager))
                                {
                                    DoorScript.Initialize(Manager);
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
                                if(partToPlace.TryGetComponent(out RoomSpawnerManager manager))
                                {
                                    DoorScript.Initialize(manager);
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

                        newPartObject.transform.SetParent(this.transform,true);
                        break; // salimos del loop para colocar una nueva pieza
                    }
                }

                // si consumimos todos los intentos posibles y no generamos nada
                // salimos del LOOP para evitar stack overflow
                if (!placementSuccessful)
                {
                    Debug.LogError("Dungeon generation failed. Could not find a valid placement after " + TotalTriesPerRoomGeneration + " attempts.");
                    if(!IsRegenerating)StartCoroutine(Regenerate());
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
                    Debug.LogError("Visceral Proc.Gen : no hay posibles espacios en la mazmorra para salas especiales");
                    if (!IsRegenerating) StartCoroutine(Regenerate());
                    break;
                }
                int randomSourceSeed = Random.Range(0, SourcePool.Count-1);
                if(randomSourceSeed < 0 || randomSourceSeed > SourcePool.Count)
                {
                    Debug.LogError("Visceral Proc.Gen : no hay posibles espacios en la mazmorra para salas especiales");
                    if (!IsRegenerating) StartCoroutine(Regenerate());
                    break;
                }

                DungeonPart SourceRoom = SourcePool[randomSourceSeed];

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

                    sourceEntryPoint.SetOccupied(false);
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
                    sourceEntryPoint.SetOccupied(false);
                    NewPartEntryPoint.SetOccupied(false);
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
                    sourceEntryPoint.SetOccupied(true);
                    NewPartEntryPoint.SetOccupied(true);

                    // creamos una puerta en el punto ocupado
                    var Door = Instantiate(DoorOBJ, sourceEntryPoint.transform.position, sourceEntryPoint.transform.rotation);
                    Door.transform.SetParent(sourceEntryPoint.transform, true);

                    // TESTEAMOS SI EL SOURCE ES UNA SALA,LA INICIAMOS
                    if(SourceRoom.RoomType == DungeonPart.DungeonPartType.Room)
                    {
                        if(SourceRoom.TryGetComponent(out RoomSpawnerManager Manager))
                        {
                            if (Door.TryGetComponent(out DoorScript DoorSC))
                            {
                                DoorSC.Initialize(Manager);
                                DoorSC.ShouldGenerateEvents(false);

                            }
                        }
                    }



                    // añadimos la sala especial al listado de salas generadas
                    generatedSpecialRooms.Add(NewPart);
                    NewPart.transform.SetParent(this.transform,true);
                    placementSuccessful = true;
                    break;
                }

            }
            if (!placementSuccessful)
            {
                Debug.LogError("Dungeon generation failed. Could not find a valid placement for special room after : " + TotalTriesPerRoomGeneration + " attempts.");
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

        FillEmptyEntries();
    }


    private void FillEmptyEntries()
    {
        generatedRooms.ForEach(room => room.FillEmptyPoints());
        generatedHallways.ForEach(hallway => hallway.FillEmptyPoints());
        print("Dungeon generation finished!");
        IsGenerated = true;

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

        /*
        foreach (var other in generatedRooms)
        {
            if (other == Part) continue;
            if (Part.collider.bounds.Intersects(other.collider.bounds))
            return true;
            
            //tiene multiples colliders
            foreach(var _col in Part._Colliders)
            {
                //por cada collider en part, chequeamos si colisiona con other colliders
                foreach(var _otherCol in other._Colliders)
                {
                    if (_col.bounds.Intersects(_otherCol.bounds))
                    {
                        //interseccion
                        return true;
                    }
                }
            }
        }

        foreach (var other in generatedHallways)
        {
            if (other == Part) continue;
            if (Part.collider.bounds.Intersects(other.collider.bounds))
                return true;

            foreach (var _col in Part._Colliders)
            {
                //por cada collider en part, chequeamos si colisiona con other colliders
                foreach (var _otherCol in other._Colliders)
                {
                    if (_col.bounds.Intersects(_otherCol.bounds))
                    {
                        //interseccion
                        return true;
                    }
                }
            }
        }

        */

        List<DungeonPart> AllParts = new List<DungeonPart>();
        AllParts.AddRange(generatedRooms);
        AllParts.AddRange(generatedHallways);
        AllParts.AddRange(generatedSpecialRooms);

        foreach(var ExistingPart in AllParts)
        {
            if (ExistingPart == Part)
            {
                continue; // no nos chequemos a nosotros xd
            }

            // por cada collider en la pieza a poner
            foreach(var partCol in Part._Colliders)
            {
                //por cada colider en TODAS las piezas existentes
                foreach(var otherCol in ExistingPart._Colliders)
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
    private void RetryPlacement(DungeonPart ItemToPlace,GameObject DoorToPlace)
    {
        // la sala que vamos a usar para generar un vecino
        DungeonPart RandomGeneratedRoom = null;
        DungeonEntryPoint EntryPoint1 = null;

        int TotalTries = 100; // watchdog
        int RetryIndex = 0;


        while(RandomGeneratedRoom == null && RetryIndex < TotalTries) 
        {
            //seleccionamos una sala a testear.
            int RandomLinkRoomIndex = Random.Range(0, generatedRooms.Count - 1);
            DungeonPart RoomToTest = generatedRooms[RandomLinkRoomIndex];

            //chequeamos si la sala tiene espacios abiertos de coneccion
            if(RoomToTest.HasAvailableEntryPoint(out EntryPoint1))
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
}
