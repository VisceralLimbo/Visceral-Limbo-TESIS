using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DoorScript : MonoBehaviour, IRaycastInteractable
{
    [Header("References")]
    [SerializeField] RoomSpawnerManager roomSpawnerManager;
    [SerializeField] AnimatorHandler _AnimHandler;

    private Vector3 InWardAlignment;

    [Space]
    [Header("Variables")]


    [Range(-1, 1)]
    [Tooltip("El limite a partir de que se considera adentro de la habitacion" +
        " 1 = el jugador esta frente de la puerta, -1 = el jugador esta detras de la puerta")]
    [SerializeField] float DoorThreshold; // el limite a partir de que se considera adentro de la habitacion
    [SerializeField] bool PlayerEnteredRoom;
    [Tooltip("Si esta puerta debería de activar eventos (ejemplo: comenzar combate)")]
    [SerializeField] bool NonTriggerRoom; // esta habitacion NO genera trigger Events

    [SerializeField] bool _LockDoor;
    [SerializeField] bool _IsOpen;
    [SerializeField] bool _OpenDoorInside;
    [SerializeField] bool _InvertAnimations;

    public void Initialize(RoomSpawnerManager SpawnerManager)
    {
        print("Initilializing with spawner" + SpawnerManager.name);

        if (
            SpawnerManager == null) return;
            
        roomSpawnerManager = SpawnerManager;
        roomSpawnerManager.OnCombatEnded += UnlockDoor;
        roomSpawnerManager.OnCombatStart += LockDoor;

        // alineamiento de las puertas.
        // calculamos la direccion hacia el centro de la sala

        SpawnerManager.GetDungeonPart(out DungeonPart _DungeonPart);

        Vector3 DirectionToRoom;

        // fallback chequeo que tenemos dungeonPart
        if (_DungeonPart == null || _DungeonPart._Colliders.Count() == 0)
        {
            Debug.LogWarning("Visceral Warning Proc. Gen : sala generada " + SpawnerManager.name + " no posee dungeon part o no tiene collider, usando default");
            DirectionToRoom = SpawnerManager.transform.position - this.transform.position;
            InWardAlignment = GetSnapPoint(DirectionToRoom);
            return;
        }

        // 1) CALCULAMOS LOS LIMITES COMBINADOS DE TODOS LOS COLLIDERS DE LA SALA.

        Bounds EncapsulatedBounds = _DungeonPart._Colliders[0].bounds;
        for(int I = 1; I < _DungeonPart._Colliders.Length; I++)
        {
            EncapsulatedBounds.Encapsulate(_DungeonPart._Colliders[I].bounds);
        }

        // 2) OBTENEMOS EL VERDADERO CENTRO DE LA SALA 

        Vector3 BoundCenter = EncapsulatedBounds.center;

        // 3) CALCULAMOS LA DIRECCION HACIA EL CENTRO CORRECTO

        DirectionToRoom = BoundCenter - this.transform.position;

        // 4) SNAPPEAMOS LA PUERTA
        InWardAlignment = GetSnapPoint(DirectionToRoom);

        if (roomSpawnerManager.ManagerStopped)
        {
            ShouldGenerateEvents(true);
        }


        // 5) PONDERAMOS LA ROTACION DEL PREFAB EN FUNCION DEL ROOM

        Vector3 SnappedLocalForward = GetSnapPoint(this.transform.forward);


        if (Vector3.Dot(SnappedLocalForward,InWardAlignment) > 0)
        {
            _InvertAnimations = true;
        }
        else
        {
            _InvertAnimations = false;
        }



    }


    private Vector3 GetSnapPoint(Vector3 Direction) 
    {
        Direction.y = 0;

        if (Mathf.Abs(Direction.x) > Mathf.Abs(Direction.z))
        {

            // la direccion nueva de snap
            return new Vector3(Mathf.Sign(Direction.x), 0, 0);
        }
        else
        {
            // la direccion en Z es mayor, por ende vamos a snappear en funcion de Z
            return new Vector3(0, 0, Mathf.Sign(Direction.z));
        }
    }


    public void ShouldGenerateEvents(bool value = false)
    {
        NonTriggerRoom = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckPlayerEnteredRoom(other);

    }

    private void OnTriggerStay(Collider other)
    {

        CheckPlayerEnteredRoom(other);

    }

    private void CheckPlayerEnteredRoom(Collider other)
    {
        if (PlayerEnteredRoom || _LockDoor || NonTriggerRoom|| roomSpawnerManager.ManagerStopped)
        {
            return;
        }

        if (!other.gameObject.CompareTag("Player")
           || roomSpawnerManager == null)
        {
            return;
        }

        if (other.gameObject.TryGetComponent(out PlayerContext Pcontext))
        {
            if (Pcontext.faction != FactionID.Player) return;
        }

        var DirectionToPlayer = other.transform.position - this.transform.position;
        DirectionToPlayer.Normalize();

        //usamos el valor de InwardAlignment para calcular la direccion del player
        float DotProduct = Vector3.Dot(InWardAlignment, DirectionToPlayer);

        if(roomSpawnerManager == null)
        {
            Debug.LogError("Visceral Error: Door Script no tiene asignado el room manager");
            _AnimHandler.SetParameter("DoorAnim", "Abrir", AnimatorControllerParameterType.Bool, false);
            return;
        }

        // calculamos si el jugador cruzo la puerta
        if (DotProduct > DoorThreshold && !NonTriggerRoom && _IsOpen == true)
        {
            print("entro a la habitacion");
            // correr animacion de puerta cerrandose
            _AnimHandler.SetParameter("DoorAnim", "Abrir", AnimatorControllerParameterType.Bool, false);
            // correr animacion de puerta cerrandose
            _AnimHandler.SetParameter("DoorAnim", "AbrirAdentro", AnimatorControllerParameterType.Bool, false);
            roomSpawnerManager.AssignPlayerContext(Pcontext);
            roomSpawnerManager.StartRoomCombat();
            roomSpawnerManager.NotifyMinionDeath();
            _LockDoor = true;
            
        }

    }

    public void OnInteract(RayCastWrapper Detector)
    {
        print("DoorInteracted");
        if (!_LockDoor && (roomSpawnerManager != null && !roomSpawnerManager.ManagerStopped) || !_LockDoor)
        {
            // CALCULO DE SENTIDO DE PUERTA 

            Vector3 Dir = Detector.transform.position - this.transform.position;

             float DirAlign = Vector3.Dot(Dir, InWardAlignment);

            //LA PUERTA ESTA ADENTRO
            if (DirAlign >= 0)
            {
                _OpenDoorInside = true;
            }
            else
            {
                _OpenDoorInside = false;
            }

            if (_InvertAnimations)
            {
                _OpenDoorInside = !_OpenDoorInside;
            }

            if (_OpenDoorInside)
            {
                // correr animacion de puerta abriendose
                _AnimHandler.SetParameter("DoorAnim", "AbrirAdentro", AnimatorControllerParameterType.Bool, true);
            }
            else
            {

                // correr animacion de puerta abriendose
                _AnimHandler.SetParameter("DoorAnim", "Abrir", AnimatorControllerParameterType.Bool, true);
            }
            _IsOpen = true;

            PlayerEvents.Interact();
        }
    }

    public void OnRayCastEnter(RayCastWrapper Detector = null)
    {  
            PlayerEvents.InteractSeeing();   
    }

    public void OnRayCastStay(RayCastWrapper Detector = null)
    {
    }
 
    public void OnRayCastExit(RayCastWrapper Detector = null)
    {
        PlayerEvents.InteractStopSeeing();
    }

    private void UnlockDoor()
    {
        _LockDoor = false;
    }

    private void LockDoor()
    {
        _LockDoor = true;
    }

}
