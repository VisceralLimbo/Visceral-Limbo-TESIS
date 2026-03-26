using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// este enum indica que tipo de input de agachado usa el juego
/// </summary>
public enum CrouchEnum
{
    None, Toggle
}


public struct InputMovement
{
    /// <summary>
    /// El quaternion de rotacion que tiene que realizar
    /// </summary>
    public Quaternion rotation;

    /// <summary>
    /// el vector de movimiento que tiene que utilizar
    /// </summary>
    public Vector2 Movement;

    /// <summary>
    /// booleano que indica que esta saltando
    /// </summary>
    public bool Jumping;

    /// <summary>
    /// booleano que indica que sigue saltando
    /// </summary>
    public bool JumpSustaining; //bool que se mantiene true mientras el jugador presione espacio

    /// <summary>
    /// Enum que indica que estamos agachandonos
    /// </summary>
    public CrouchEnum Crouch;

    /// <summary>
    /// booleano que indica que el jugador uso la habilidad soporte
    /// </summary>
    public bool Ability_Support;

    /// <summary>
    /// booleano que indica que el presiono el click izquierdo
    /// </summary>
    public bool LeftMouseClick;

    /// <summary>
    /// booleano que indica que el jugador mantiene presionado el click izquierdo
    /// </summary>
    public bool SustainedLeftMouseClick;

    /// <summary>
    /// booleano que indica que el jugador solto  el click izquierdo
    /// </summary>
    public bool ReleasedLeftMouseClick;

    /// <summary>
    /// booleano que indica que el presiono el click Derecho
    /// </summary>
    public bool RightMouseClick;

    /// <summary>
    /// booleano que indica que se mantiene
    /// </summary>
    public bool SustainedRightMouseClick;

    /// <summary>
    /// booleano que indica que se largo el click Derecho
    /// </summary>
    public bool ReleasedRightMouseClick;

    /// <summary>
    /// booleano que indica que el jugador presiono la habilidad 1
    /// </summary>
    public bool Ability_1;
    /// <summary>
    /// booleano que indica que el jugador presiono la habilidad 2
    /// </summary>
    public bool Ability_2;
    /// <summary>
    /// booleano que indica que el jugador presiono la Ultimate
    /// </summary>
    public bool Ultimate;

    /// <summary>
    /// booleano que indica que el jugador presiono el kick
    /// </summary>
    public bool Kick;

    public bool Interact;

    public bool Inventory;
}



/// <summary>
/// Este Script maneja el Input System del jugador
/// </summary>
public class Player_InputHandler : MonoBehaviour
{
    public PlayerInputActions _Player_InputActions { get; private set; }
    public InputMovement CurrentMovementInput { get; private set; }

    public static Player_InputHandler instance;

    public InputStruct _MovementInput { get; private set; }
    public InputStruct _CameraInput { get; private set; }

    public Player_CameraController _Player_CameraController;


    private void Awake()
    {
        _Player_InputActions = new PlayerInputActions();
        _Player_InputActions.Enable();

        if(instance == null && instance != this)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

    }


    public void Update()
    {

        var Input = _Player_InputActions.Gameplay;

        //logica de camara
        //
        // recibir camera input y actualizar la rotacion
        //creo struct
        var cameraInput = new InputStruct
        {
            //set variable lookdelta con el valor del input.look creado en playerinput
            LookDelta = Input.Look.ReadValue<Vector2>(),
        };

        //logica de movimiento
        //recibo movement inputs y actualizo 
        //creo struct
        var movementInput = new InputMovement
        {
            //rotation => rotacion del jugador
            //movement => movimiento del jugador
            //jumping/jumpsustaining => salto del jugador

            rotation = _Player_CameraController.transform.rotation,
            Movement = Input.Movement.ReadValue<Vector2>(),
            Jumping = Input.Jump.WasPressedThisFrame(),
            JumpSustaining = Input.Jump.IsPressed(),

            // pseudo codigo => si el boton crouch fue presionado, devolver toggle , de no serlo devolver none
            Crouch = Input.Crouch.WasPressedThisFrame() ? CrouchEnum.Toggle : CrouchEnum.None,


            Ability_Support = Input.Ability_Support.WasPressedThisFrame(),
            LeftMouseClick = Input.Mouse1.WasPressedThisFrame(),
            SustainedLeftMouseClick = Input.Mouse1.IsPressed(),
            ReleasedLeftMouseClick = Input.Mouse1.WasReleasedThisFrame(),
            Ability_1 = Input.Ability_1.WasPressedThisFrame(),
            Ability_2 = Input.Ability_2.WasPressedThisFrame(),
            Ultimate = Input.Ultimate.WasPressedThisFrame(),
            Kick = Input.Kick.WasPressedThisFrame(),

            RightMouseClick = Input.Mouse2.WasPressedThisFrame(),
            SustainedRightMouseClick = Input.Mouse2.IsPressed(),
            ReleasedRightMouseClick = Input.Mouse2.WasReleasedThisFrame(),
            Interact = Input.Interact.WasPressedThisFrame(),
            Inventory = Input.Inventory.WasPressedThisFrame(),
            
        };

        CurrentMovementInput = movementInput;
        _CameraInput = cameraInput;


    }

}
