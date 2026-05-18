using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class Corpus_MovementRotateState : BaseState
{
    [Header("References")]
    [SerializeField] private Corpus_Controller _controller;
    [SerializeField] private IMovementStrategy _movementStrategy;
    [SerializeField] private AnimatorHandler _animHandler;

    [Space]
    [Header("Variables")]
    [SerializeField] private float movementSpeed = 8f;
    [SerializeField] private float OrbitRadius = 7f;

    [Tooltip("Qué tan agresivo es el ajuste para volver al radio ideal (Valores entre 0.5 y 2 recomendados)")]
    [SerializeField] private float SpiralDriftCorrection = 1f;

    private float _statePulse = 0;
    private bool _flipFlopDirection;

    public override void OnInitialize(VisceralStateMachine CTX)
    {
        base.OnInitialize(CTX);

        if (_controller == null)
        {
            _controller = CTX.GetComponentInChildren<Corpus_Controller>();
        }

        if (_movementStrategy == null)
        {
            _movementStrategy = CTX.GetComponentInChildren<IMovementStrategy>();

            if (_movementStrategy != null && _controller != null)
            {
                _movementStrategy.Initialize(CTX.GetComponentInChildren<KinematicCharacterMotor>(), _controller.playerContext.PlayerGameObject);
            }
        }
    }

    public override void OnEnter(VisceralStateMachine CTX)
    {
        base.OnEnter(CTX);

        _statePulse = 0;
        _flipFlopDirection = !_flipFlopDirection; // Cambia de dirección (horario/antihorario) en cada entrada

        if (_movementStrategy != null)
        {
            _movementStrategy.SetActiveState(true);
            _movementStrategy.SetMovementSpeed(movementSpeed); // Aseguramos que la estrategia conozca la velocidad
        }

        CTX.SetGlobalCondition("ShouldMove", false);
        CTX.SetGlobalCondition("ShouldMoveAround", false);

        _animHandler.SetParameter("Corpus_Anim", "SideWalk", AnimatorControllerParameterType.Bool, true);
    }

    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        base.OnTick(CTX, TickRate);

        if (_controller == null || _controller.Target == null || _movementStrategy == null) return;

        Vector3 modelPos = _controller.playerContext.PlayerGameObject.transform.position;
        Vector3 targetPos = _controller.Target.position;

        // 1) Vector base hacia el jugador (Aplanado en Y)
        Vector3 toTarget = targetPos - modelPos;
        toTarget.y = 0;

        float currentDistance = toTarget.magnitude;
        if (currentDistance < 0.1f) return; // Failsafe para evitar división por cero

        Vector3 lookDirection = toTarget / currentDistance; // Dirección normalizada hacia el jugador

        // 2) Rotación: El modelo siempre clava la mirada en el jugador de forma limpia
        _movementStrategy.UpdateRotation(lookDirection);

        // 3) Dirección Tangencial (El movimiento lateral de la órbita)
        Vector3 tangentDirection;
        if (_flipFlopDirection)
        {
            tangentDirection = Vector3.Cross(Vector3.up, -lookDirection).normalized;
        }
        else
        {
            tangentDirection = Vector3.Cross(-lookDirection, Vector3.up).normalized;
        }

        // 4) Cálculo del error de distancia escalar (Control de órbita estable)
        // Si distError > 0: Estamos lejos, hay que inclinarse HACIA el jugador.
        // Si distError < 0: Estamos muy cerca, hay que inclinarse LEJOS del jugador.
        float distanceError = currentDistance - OrbitRadius;

        // Creamos un vector de corrección empujando hacia adelante o hacia atrás del eje de mirada
        Vector3 correctionDirection = lookDirection * distanceError * SpiralDriftCorrection;

        // 5) Combinación final: Sumamos el movimiento lateral + la inclinación de corrección
        Vector3 finalMovementDirection = (tangentDirection + correctionDirection).normalized;

        // 6) Inyección al KCC multiplicando OBLIGATORIAMENTE por la velocidad de movimiento
        _movementStrategy.UpdateVelocity(finalMovementDirection * movementSpeed);
    }

    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        if (_MinStateLifetime > _statePulse)
        {
            _statePulse += Time.deltaTime * TimeDilationManager.GlobalTimeScale;
            TO = null;
            return false;
        }

        return base.EvaluateTransitions(GlobalParams, out TO);
    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        _animHandler.SetParameter("Corpus_Anim", "SideWalk", AnimatorControllerParameterType.Bool, false);
        _statePulse = 0;

        if (_movementStrategy != null)
        {
            _movementStrategy.ResetMovementSpeed();
        }

        base.OnExit(CTX);
    }
}
