using System.Collections;
using System.Collections.Generic;

using UnityEngine;

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
    [SerializeField]
    private List<DungeonPart> generatedRooms;

    /// <summary>
    /// la lista de pasillos generados
    /// </summary>
    [SerializeField]
    private List<DungeonPart> generatedHallways;

    /// <summary>
    /// la lista de habitaciones especiales generadas
    /// </summary>
    [SerializeField]
    private List<DungeonPart> generatedSpecialRooms;

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
            Regenerate();
        }
    }
    private void Regenerate()
    {
        foreach(DungeonPart room in generatedRooms)
        {
            Destroy(room.gameObject);
        }
        generatedRooms.Clear();

        foreach(DungeonPart hall in generatedHallways)
        {
            Destroy(hall.gameObject);
        }
        generatedHallways.Clear();
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

            // Place the entrance room first
            if (RegularRoomCount > 0)
            {
                GameObject entranceObj = Instantiate(Entrance, this.transform.position, this.transform.rotation);
                if (entranceObj.TryGetComponent(out DungeonPart part))
                {
                    generatedRooms.Add(part);
                    GenerateHallway = true;
                }
            }

            // Use a 'while' loop to ensure we generate the correct number of rooms
            while (generatedRooms.Count < RegularRoomCount)
            {
                bool placementSuccessful = false;
                DungeonPart partToPlace = null; // This will hold our new room or hallway

               
                // It will try up to 'TotalTriesPerRoomGeneration'
                // times to place ONE new part.
                for (int i = 0; i < TotalTriesPerRoomGeneration; i++)
                {
                // 1. SELECT A RANDOM EXISTING ROOM AND ENTRY POINT
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

                    // Catch! we avoid possible crash due to empty pool
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
                        continue; // This room has no open doors, try another one  
                    }

                    // 2. CREATE THE NEW PART TO PLACE (ROOM OR HALLWAY)
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
                        Destroy(newPartObject); // Clean up the failed part
                        sourceEntryPoint.SetOccupied(false);
                        sourceRoom.UnuseEntryPoint(sourceEntryPoint);
                        continue; // The prefab was bad or had no entries, try again
                    }

                    // 3. ALIGN THE NEW PART AND CHECK FOR INTERSECTIONS
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

                        // INTERSECTION! Clean up and let the loop try again.
                        Destroy(newPartObject);
                        partToPlace = null;
                        continue;
                }
                    else
                    {
                    
                        // 4. SUCCESS!
                        // =====================================================
                        placementSuccessful = true;

                    // Lock the entry points since we used them
                        sourceEntryPoint.SetOccupied(true);
                        newPartEntryPoint.SetOccupied(true);

                        // Create the door between them
                        var Door = Instantiate(DoorOBJ, sourceEntryPoint.transform.position, sourceEntryPoint.transform.rotation);
                        Door.transform.SetParent(sourceEntryPoint.transform, true);

                        // Add the new part to the correct list
                        if (GenerateHallway)
                        {
                            generatedHallways.Add(partToPlace);
                        }
                        else
                        {
                            generatedRooms.Add(partToPlace);
                        }

                        GenerateHallway = !GenerateHallway; // Alternate for the next part

                        newPartObject.transform.SetParent(this.transform,true);
                        break; // Exit the "Retry" loop, we successfully placed this part.
                    }
                }

                // If after all tries we couldn't place a part, stop the generation to avoid an infinite loop.
                if (!placementSuccessful)
                {
                    Debug.LogError("Dungeon generation failed. Could not find a valid placement after " + TotalTriesPerRoomGeneration + " attempts.");
                    Regenerate();
                    break; // Exit the main 'while' loop
                }

                if (SlowGen)
                {
                    yield return new WaitForSeconds(SlowGenSpeed);
                }
            }

            FillEmptyEntries();
            IsGenerated = true;
            print("Dungeon generation finished!");

    }


    private void FillEmptyEntries()
    {
        generatedRooms.ForEach(room => room.FillEmptyPoints());
        generatedHallways.ForEach(hallway => hallway.FillEmptyPoints());

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
        foreach (var other in generatedRooms)
        {
            if (other == Part) continue;
            if (Part.collider.bounds.Intersects(other.collider.bounds))
                return true;
        }

        foreach (var other in generatedHallways)
        {
            if (other == Part) continue;
            if (Part.collider.bounds.Intersects(other.collider.bounds))
                return true;
        }

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
