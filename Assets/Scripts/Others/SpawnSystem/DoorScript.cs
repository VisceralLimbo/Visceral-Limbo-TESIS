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
    public void Initialize(RoomSpawnerManager SpawnerManager)
    {
        print("Initilializing with spawner" + SpawnerManager.name);

        if (
            SpawnerManager == null) return;
            
        roomSpawnerManager = SpawnerManager;
        

        // alineamiento de las puertas.
        // calculamos la direccion hacia el centro de la sala
        Vector3 DirectionToRoom = SpawnerManager.transform.position - this.transform.position;

        // aplanamos esa direccion, porque solo nos interesa un eje 
        //(ya que las conecciones son en angulos de 90 grados)
        if(Mathf.Abs(DirectionToRoom.x) > Mathf.Abs(DirectionToRoom.z))
        {
            // la direccion principal es el eje X
            InWardAlignment = new Vector3(Mathf.Sign(DirectionToRoom.x), 0, 0);
        }
        else
        {
            // la direccion principal es el eje Z

            InWardAlignment = new Vector3(0, 0, Mathf.Sign(DirectionToRoom.x));
        }

        roomSpawnerManager.OnCombatEnded += UnlockDoor; 
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
        if (PlayerEnteredRoom || _LockDoor)
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

        //usamos el valor de InwardAlignment para calcular la direccion del player
        float DotProduct = Vector3.Dot(InWardAlignment, DirectionToPlayer);

        print("Dot product: " + DotProduct);

        // calculamos si el jugador cruzo la puerta
        if (DotProduct > DoorThreshold && !NonTriggerRoom)
        {
            print("entro a la habitacion");
            // correr animacion de puerta abriendose
            _AnimHandler.SetParameter("DoorAnim", "Abrir", AnimatorControllerParameterType.Bool, false);
            roomSpawnerManager.AssignPlayerContext(Pcontext);
            roomSpawnerManager.StartRoomCombat();
            roomSpawnerManager.NotifyMinionDeath();
            PlayerEnteredRoom = true;
            _LockDoor = true;
        }

    }

    public void OnInteract()
    {
        if (!_LockDoor)
        {
            // correr animacion de puerta abriendose
            _AnimHandler.SetParameter("DoorAnim", "Abrir", AnimatorControllerParameterType.Bool, true);
        }
    }

    public void OnRayCastEnter(RayCastWrapper Detector = null)
    {
   
    }

    public void OnRayCastStay(RayCastWrapper Detector = null)
    {
       
    }
 
    public void OnRayCastExit(RayCastWrapper Detector = null)
    {
        
    }

    private void UnlockDoor()
    {
        _LockDoor = false;
    }

}
