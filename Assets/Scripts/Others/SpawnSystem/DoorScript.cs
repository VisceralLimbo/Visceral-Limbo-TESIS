using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DoorScript : MonoBehaviour, IRaycastInteractable
{
    [Header("References")]
    [SerializeField] RoomSpawnerManager roomSpawnerManager;
    [SerializeField] AnimatorHandler _AnimHandler;
    [SerializeField] DungeonEntryPoint _EntryPoint;

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
    [SerializeField] bool _Initialized;

    [Header("Glow puerta sala de cofres")]
    // refe al doorglow q va a ser null en varias puertas 
    private DoorGlow _doorGlow;

    [SerializeField] SoundData _SoundOpenDoor;
    [SerializeField] SoundData _SoundCloseDoor;

    [Header("Door Lights")]
    [SerializeField] List<Light> doorLights = new List<Light>();
    [SerializeField] float lightFadeTime = 0.5f;
    private float[] originalIntensities;

    // bool para saber si se abrio
    [SerializeField] bool _HasBeenOpenedOnce = false;


    private void Start()
    {
        originalIntensities = new float[doorLights.Count];

        for (int i = 0; i < doorLights.Count; i++)
        {
            if (doorLights[i] != null)
                originalIntensities[i] = doorLights[i].intensity;
            else
                originalIntensities[i] = 1f;
        }

    }
    public void Initialize(RoomSpawnerManager SpawnerManager,DungeonEntryPoint entryPoint)
    {
        // busco doorglow dsp de q se añado con lo de procedural
        if (_doorGlow == null)
        {
            _doorGlow = GetComponent<DoorGlow>();
            if (_doorGlow != null)
            {
                Debug.Log("Puerta encontro DoorClow Component");
            }
        }

        _EntryPoint = entryPoint;
        if(_EntryPoint != null)
        {
            this.transform.position = entryPoint.transform.position;
            this.transform.rotation = entryPoint.transform.rotation;
            this.transform.SetParent(entryPoint.transform,true);
        }

        if (SpawnerManager == null) { NonTriggerRoom = true; return; };
        roomSpawnerManager = SpawnerManager;
        roomSpawnerManager.OnCombatEnded += UnlockDoor;
        roomSpawnerManager.OnCombatStart += LockDoor;


        // Paso 1) comenzamos a alinear la puerta con el punto de entrada.

        this.transform.position = entryPoint.transform.position;
        this.transform.rotation = entryPoint.transform.rotation;

        // Paso 2) Alineamos el adentro usando el vector de atras del entrypoint
        InWardAlignment = GetSnapPoint(-entryPoint.transform.forward);


        // si el manager esta parado, no emitimos eventos
        if (roomSpawnerManager.ManagerStopped)
        {
            ShouldGenerateEvents(true);
        }

        //Paso3 ) PONDERAMOS LA ROTACION DEL PREFAB EN FUNCION DEL ROOM

        Vector3 SnappedLocalForward = GetSnapPoint(this.transform.forward);

        // Paso 4) calculamos si hay que invertir las rotaciones del prefab.
        if (Vector3.Dot(SnappedLocalForward, InWardAlignment) > 0)
        {
            _InvertAnimations = true;
        }
        else
        {
            _InvertAnimations = false;
        }

    }

    private Coroutine lightFadeCoroutine;

    private void SetDoorLightsSmooth(bool on)
    {
        if (lightFadeCoroutine != null)
            StopCoroutine(lightFadeCoroutine);

        lightFadeCoroutine = StartCoroutine(LerpLights(on));
    }

    private IEnumerator LerpLights(bool turnOn)
    {
        if (doorLights == null || doorLights.Count == 0)
            yield break;

        float[] startIntensities = originalIntensities;


        float timer = 0f;

        // Si se apagan, usamos la intensidad actual como punto de partida
        float[] fromIntensity = new float[doorLights.Count];
        float[] toIntensity = new float[doorLights.Count];

        for (int i = 0; i < doorLights.Count; i++)
        {
            if (doorLights[i] == null)
                continue;

            fromIntensity[i] = doorLights[i].intensity;
            toIntensity[i] = turnOn ? startIntensities[i] : 0f;

            // Para fade in queremos que estén activadas desde el inicio
            if (turnOn)
                doorLights[i].enabled = true;
        }

        while (timer < lightFadeTime)
        {
            timer += Time.deltaTime;
            float t = timer / lightFadeTime;

            for (int i = 0; i < doorLights.Count; i++)
            {
                if (doorLights[i] == null)
                    continue;

                doorLights[i].intensity = Mathf.Lerp(fromIntensity[i], toIntensity[i], t);
            }

            yield return null;
        }

        // Apagar la luz cuando terminó el fade out
        if (!turnOn)
        {
            for (int i = 0; i < doorLights.Count; i++)
            {
                if (doorLights[i] != null)
                    doorLights[i].enabled = false;
            }
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


    private void FallbackAlignment(RoomSpawnerManager SpawnerManager)
    {

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
        for (int I = 1; I < _DungeonPart._Colliders.Length; I++)
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


        if (Vector3.Dot(SnappedLocalForward, InWardAlignment) > 0)
        {
            _InvertAnimations = true;
        }
        else
        {
            _InvertAnimations = false;
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
            
            SoundManager.Instance.CreateSound().WithSoundData(_SoundCloseDoor).WithRandomPitch(true).WithPosition(this.transform.position).play();
        }

    }

    public void OnInteract(RayCastWrapper Detector)
    {
        print("DoorInteracted");
        if (!_LockDoor && (roomSpawnerManager != null && !roomSpawnerManager.ManagerStopped) || !_LockDoor || NonTriggerRoom)
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

                SoundManager.Instance.CreateSound().WithSoundData(_SoundOpenDoor).WithRandomPitch(true).WithPosition(this.transform.position).play();
            }
            else
            {

                // correr animacion de puerta abriendose
                _AnimHandler.SetParameter("DoorAnim", "Abrir", AnimatorControllerParameterType.Bool, true);

                SoundManager.Instance.CreateSound().WithSoundData(_SoundOpenDoor).WithRandomPitch(true).WithPosition(this.transform.position).play();
            }
            _IsOpen = true;
            SetDoorLightsSmooth(false);

            // apago perma las luces de la puerta q abri
            _HasBeenOpenedOnce = true;
            SetDoorLightsSmooth(false);

            // las apago si no son de la sala de cfores

            if (_doorGlow != null)
            {
                _doorGlow.StartFadeOut();
                Debug.Log("apagando las particulas");
            }

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
        // solo se encienden si NUNCA se abrio la peurta
        if (!_HasBeenOpenedOnce)
        {
            SetDoorLightsSmooth(true);
        }

    }

    private void LockDoor()
    {
        _LockDoor = true;
        // se apaga si NUNCA fue abierta, si ya la abri queda apagada y el booleano hace q se ignore
        if (!_HasBeenOpenedOnce)
        {
            SetDoorLightsSmooth(false);
        }

    }

    public void SetEntryPointReference(DungeonEntryPoint Entry) => _EntryPoint = Entry;


}
