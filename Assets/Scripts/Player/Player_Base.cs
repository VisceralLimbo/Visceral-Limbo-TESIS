using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



//patricio malvasio maddalena
//6/4/2025 21:57
//este script sirve como el corazon de la logica basica del jugador
//este script se va a encargar de actualizar cosas, pasar inputs y funcionar =>
// => como un pseudo state machine.

public class Player_Base : Visceral_Script
{
    public InputStruct _MovementInput { get; private set; }
    public InputStruct _CameraInput { get; private set; }
    [SerializeField] private Player_Movement _Player_Movement;
    [SerializeField] private Player_CameraController _Player_CameraController;
    [SerializeField] private Player_DashTest _DashTest;
    [SerializeField] private Player_MeleeAttack _MeleeAttack;
    [SerializeField] private Player_ChargedMeleeCombat _ChargedMeleeCombat;
    [SerializeField] private Visceral_SkillManager _SkillManager;
    [SerializeField] private bool IsAlive = true, OnDialogue;
    public PlayerContext _PlayerContext;
    public Health_Component _PlayerHealth { get; private set; }

    // necesito ref para guardar el input de el frame
    public InputMovement CurrentMovementInput { get; private set; }


    //PlayerInputActions es el mappeo de las acciones de Input del jugador
    //similar al Unreal con su Input Map

    //WHAT A FUCKING HACK, HAY QUE REWORKEAR TODO ESTO!!!
    public PlayerInputActions _Player_InputActions { get; private set; }

    public Action OnPlayerSkillUse;

    [SerializeField] private RectTransform _uiElementToMove;
    [SerializeField] private float moveAmount;
    [SerializeField] private float moveSpeed = 5f;
    private Vector2 _originalUIPosition;
    private bool _isTabPressed;
    [SerializeField] private float moveAmountRight;

    [SerializeField] private RectTransform uiElementToMoveRight1;
    [SerializeField] private RectTransform uiElementToMoveRight2;

    private Vector2 _originalUIRight1Pos;
    private Vector2 _originalUIRight2Pos;

    private bool previousTabState = false;
    [SerializeField] SoundData _uiTabSound;
    // bool para el control de las habilidades
    public bool IsSkillActive { get; private set; } = false;

    [SerializeField] private bool IsPlayerActive = true;

    [SerializeField] SoundData _walkSound;
    bool isWalking = false;


    void Start()
    {
        _PlayerContext = GetComponent<PlayerContext>();
        _PlayerContext.KCCMotor = _Player_Movement.KKCMotor;
        _PlayerContext.VS_Initialize();
        _Player_Movement.VS_Initialize();
        _Player_CameraController.VS_InitializeWithParameters(_Player_Movement.GetCameraTarget());
        //_DashTest.VS_InitializeWithParameters(_Player_Movement);

        _SkillManager = GetComponent<Visceral_SkillManager>();


        _SkillManager.VS_Initialize();
        IsAlive = true;
        _PlayerHealth = GetComponentInChildren<Health_Component>();
        _PlayerHealth.OnDeath += DeathEventFlag;

        //creo e inicio Inputs
        _Player_InputActions = new PlayerInputActions();
        _Player_InputActions.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        DialogueManager.instance.OnDialogueStart += DialogueStart;
        DialogueManager.instance.OnDialogueEnd += DialogueEnd;

        if (uiElementToMoveRight1 != null && uiElementToMoveRight2 != null && _uiElementToMove != null)
        {
            _originalUIPosition = _uiElementToMove.anchoredPosition;

            _originalUIRight1Pos = uiElementToMoveRight1.anchoredPosition;


            _originalUIRight2Pos = uiElementToMoveRight2.anchoredPosition;

        }

        DungeonGenerator Generator = FindObjectOfType<DungeonGenerator>();
        if (Generator != null)
        {
            SetPlayerInactive();
            Generator.OnSuccessfulGeneration += SetPlayerActive;
        }
        else
        {
            IsPlayerActive = true;
        }


    }

    private void Update()
    {
        if (!IsAlive || !IsPlayerActive) return;


        var Input = _Player_InputActions.Gameplay;

        _isTabPressed = Input.Inventory.IsPressed();

        // Detectar apertura del inventario
        if (_isTabPressed && !previousTabState)
        {
            SoundManager.Instance.CreateSound().WithSoundData(_uiTabSound).play();
        }
        // Actualizar estado
        previousTabState = _isTabPressed;

        //logica de camara
        //
        // recibir camera input y actualizar la rotacion
        //creo struct
        var cameraInput = new InputStruct
        {
            //set variable lookdelta con el valor del input.look creado en playerinput
            LookDelta = Input.Look.ReadValue<Vector2>()

        };
        _Player_CameraController.UpdatePosition(_Player_Movement.GetCameraTarget());
        _Player_CameraController.UpdateRotation(cameraInput);

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

        };

        if (movementInput.Movement.sqrMagnitude > 0.1f)
        {
            if (!isWalking)
            {
                isWalking = true;
                SoundManager.Instance.CreateSound().WithSoundData(_walkSound).play();
            }
        }
        else
        {
            isWalking = false;
        }
        // guardo el input para llamar en speedlogic
        CurrentMovementInput = movementInput;
        _Player_Movement.UpdateBodyPositions(Time.deltaTime);
        _Player_Movement.UpdateInput(movementInput);
        //_DashTest.PerformDash(movementInput);


        if (!OnDialogue)
        {
            //_MeleeAttack.RunData(movementInput);
            //_ChargedMeleeCombat.VS_Runlogic(movementInput);

            if (!IsSkillActive)
            {
                // si no tengo una hablidad activa puedo hacer el melee 
                _MeleeAttack.VS_Runlogic(movementInput);

                // lo q ya teniamos
                if (!movementInput.SustainedLeftMouseClick && !movementInput.LeftMouseClick)
                {
                    //x2
                    ActivateSkills(movementInput);
                }
            }

            if (!movementInput.SustainedLeftMouseClick && !movementInput.LeftMouseClick)
            {
                ActivateSkills(movementInput);
            }
        }

        if (_uiElementToMove != null)
        {
            Vector2 targetPosition = _isTabPressed
                ? _originalUIPosition + Vector2.left * moveAmount
                : _originalUIPosition;

            _uiElementToMove.anchoredPosition = Vector2.Lerp(
                _uiElementToMove.anchoredPosition,
                targetPosition,
                Time.deltaTime * moveSpeed
            );
        }

        if (uiElementToMoveRight1 != null)
        {
            Vector2 targetPos1 = _isTabPressed
                ? _originalUIRight1Pos + Vector2.right * moveAmountRight
                : _originalUIRight1Pos;

            uiElementToMoveRight1.anchoredPosition = Vector2.Lerp(
                uiElementToMoveRight1.anchoredPosition,
                targetPos1,
                Time.deltaTime * moveSpeed
            );
        }

        if (uiElementToMoveRight2 != null)
        {
            Vector2 targetPos2 = _isTabPressed
                ? _originalUIRight2Pos + Vector2.right * moveAmountRight
                : _originalUIRight2Pos;

            uiElementToMoveRight2.anchoredPosition = Vector2.Lerp(
                uiElementToMoveRight2.anchoredPosition,
                targetPos2,
                Time.deltaTime * moveSpeed
            );
        }

    }



    private void ActivateSkills(InputMovement Inputs)
    {
        // si estoy usando una habilidad return / si no sigue la cadena de ifs
        if (IsSkillActive)
        {
            return;
        }

        if (Inputs.Ability_1)
        {
            _SkillManager.TryUseSkill("Skill1");
            OnPlayerSkillUse?.Invoke();
        }
        if (Inputs.Ability_2)
        {
            _SkillManager.TryUseSkill("Skill2");
            OnPlayerSkillUse?.Invoke();
        }
        if (Inputs.Ultimate)
        {
            _SkillManager.TryUseSkill("Ult");
            OnPlayerSkillUse?.Invoke();
        }
        if (Inputs.Kick)
        {
            _SkillManager.TryUseSkill("Kick");
            OnPlayerSkillUse?.Invoke();
        }
        if (Inputs.Ability_Support)
        {
            _SkillManager.TryUseSkill("Support");
            OnPlayerSkillUse?.Invoke();
        }

        _SkillManager.VS_RunLogic();
    }


    private void OnDestroy()
    {
        _Player_InputActions?.Dispose(); // me deshago de inputs porque si no siguen de fondo
    }
    private void DeathEventFlag()
    {
        IsAlive = false;
        _PlayerHealth.OnDeath -= DeathEventFlag;
    }


    private void DialogueStart()
    {
        OnDialogue = true;
    }

    private void DialogueEnd()
    {
        OnDialogue = false;
    }

    // para q cambien el estado las habilidaeds
    public void SetSkillActiveState(bool state)
    {
        IsSkillActive = state;
    }

    public void SetPlayerActive() => IsPlayerActive = true;
    public void SetPlayerInactive() => IsPlayerActive = false;
}
