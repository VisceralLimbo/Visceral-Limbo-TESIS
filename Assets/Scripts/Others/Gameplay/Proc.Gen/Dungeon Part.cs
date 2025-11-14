using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonPart : MonoBehaviour
{

    /// <summary>
    /// Enum de tipos de partes de mazmorra
    /// </summary>
    public enum DungeonPartType
    {
        Room,
        Hallway,
        Entrance,   // La sala de inicio (usa esto para el Entrance Prefab)
        Special,    // Habitaciones especiales
        Boss        // Habitaciones de jefe
    }

    [HideInInspector] // Oculta esto en el Inspector de Unity si no quieres verlo
                      // Modifica el setter de MapCoords para actualizar la variable debug
    public Vector2Int MapCoords
    {
        get { return _debugMapCoords; }
        set { _debugMapCoords = value; }
    }

    [Header("Minimap Debug")]
    [SerializeField]
    private Vector2Int _debugMapCoords;

    [Header("Variables & SetUp")]
    /// <summary>
    /// El layer mask de las habitaciones, usado para detectar colisiones
    /// </summary>
    [SerializeField]
    private LayerMask _RoomLayerMask;


    /// <summary>
    /// El tipo de parte de mazmorra, usado para generar los pools de habitaciones
    /// </summary>
    [SerializeField]
    private DungeonPartType _RoomType;
    public DungeonPartType RoomType { get { return _RoomType; } }

    /// <summary>
    /// El tipo de "Fill" usado para tapar las salidas no usadas.
    /// </summary>
    [SerializeField]
    private GameObject _WallFill;

    /// <summary>
    /// Los puntos de entrada/salida de la sala, usados para conectar la sala con otras
    /// </summary>
    public List<DungeonEntryPoint> EntryPoints;

    public new Collider collider;

    public Collider[] _Colliders;

    [Space]
    [Header("Debug")]
    [SerializeField] bool DrawRoomCollider;
    [SerializeField] bool DrawEntryPoints;

    public void Awake()
    {
        foreach (var entryPoint in EntryPoints)
        {
            entryPoint.SetOwner(this);
        }
    }

    public bool HasAvailableEntryPoint(out DungeonEntryPoint EntryPoint)
    {
        DungeonEntryPoint ResultingPoint = null; // punto resultante que esta disponible
        bool Result = false; // si hay un punto disponible

        int totalTries = 100; // watchdog de maximos chequeos
        int RetryIndex = 0;

        // solo tenemos 1 entrada
        if (EntryPoints.Count == 1)
        {
            DungeonEntryPoint Entry = EntryPoints[0];

            // si la entrada esta ocupada
            if (Entry.IsOccupied())
            {
                Result = false;
                ResultingPoint = null;
            }
            else
            {
                // Guardamos el punto libre
                ResultingPoint = Entry;
                Result = true;
            }

            EntryPoint = ResultingPoint;
            return Result;

        }

        // hay multiples puntos de ingreso
        while (ResultingPoint == null && RetryIndex < totalTries)
        {
            int randomEntryIndex = Random.Range(0, EntryPoints.Count);

            // agarro un punto vacio
            DungeonEntryPoint Entry = EntryPoints[randomEntryIndex];

            // si el punto chequeado no esta ocupado
            if (!Entry.IsOccupied())
            {
                ResultingPoint = Entry;
                Result = true;

            }
            RetryIndex++;
        }

        //retorno punto elegido
        EntryPoint = ResultingPoint;
        return Result;
    }

    /// <summary>
    /// Funcion para setear el estado de ocupacion de un punto de entrada
    /// </summary>
    /// <param name="EntryPoint"></param>
    public void UnuseEntryPoint(DungeonEntryPoint EntryPoint)
    {
        EntryPoint.SetOccupied(null, false);
    }

    public List<DungeonEntryPoint> GetAvailableEntryPoints()
    {
        List<DungeonEntryPoint> AvailableEntryPoints = new List<DungeonEntryPoint>();

        foreach (DungeonEntryPoint Entry in EntryPoints)
        {
            if (Entry.IsOccupied())
            {
                continue;
            }
            else
            {
                AvailableEntryPoints.Add(Entry);
                // Guardamos el punto libre
            }
        }

        return AvailableEntryPoints;
    }

    public void FillEmptyPoints()
    {
        foreach (DungeonEntryPoint Entry in EntryPoints)
        {
            if (!Entry.IsOccupied())
            {
                var wall = Instantiate(_WallFill, Entry.transform.position, Entry.transform.rotation);
                wall.transform.SetParent(Entry.transform, true);
            }
        }
    }


    private void OnDrawGizmos()
    {
        if (DrawRoomCollider)
        {
            foreach (var _Col in _Colliders)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(_Col.bounds.center, _Col.bounds.size);
            }

        }

        if (DrawEntryPoints)
        {
            foreach (DungeonEntryPoint entryPoint in EntryPoints)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(entryPoint.transform.position, 0.3f);

                Gizmos.color = Color.red;
                Gizmos.DrawLine(entryPoint.transform.position
                    , entryPoint.transform.position + entryPoint.transform.forward);
            }

        }

    }
}
