using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class Corpus_MovementRotateState : BaseState
{
    [Header("References")]
    [SerializeField] Transform Model;
    [SerializeField] Transform Target;
    [SerializeField] Corpus_Thinking_Main_State thinkingMain;

    [SerializeField] IMovementStrategy MovementStrategy;
    [SerializeField] AnimatorHandler _AnimHandler;
    [Space]

    [Header("Variables")]
    [SerializeField] float movementSpeed;
    [SerializeField] float OrbitRadius;

    [Tooltip("Como es un movimiento radial con aceleracion, lentamente el personaje se va a ir alejando por fuerza centrifugal, para evitar eso se le aplica una correccion determinada por este valor")]
    [SerializeField] float SpiralDriftCorrection;

   

    [SerializeField] float Statepulse = 0;

    bool FlipFlopDirection;
    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        if(_MinStateLifetime > Statepulse)
        {
            Statepulse += Time.deltaTime * TimeDilationManager.GlobalTimeScale;
            TO = null;
            return false;
        }

        return base.EvaluateTransitions(GlobalParams, out TO);
    }

    public override void OnDeInitialize(VisceralStateMachine CTX)
    {
        base.OnDeInitialize(CTX);
    }

    public override void OnEnter(VisceralStateMachine CTX)
    {

        base.OnEnter(CTX);
        Target = thinkingMain.GetTargetTransform;
        Model = thinkingMain.GetModel.transform;
        MovementStrategy.SetActiveState(true);

        CTX.SetGlobalCondition("ShouldMove", false);
        CTX.SetGlobalCondition("ShouldMoveAround", false);
        Statepulse = 0;
        thinkingMain.DeactivateEnergy(false);

        _AnimHandler.SetParameter("Corpus_Anim", "SideWalk", AnimatorControllerParameterType.Bool, true);

        FlipFlopDirection = !FlipFlopDirection;
    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        if(thinkingMain.GetCurrentEnergy > 3)
        {
            _AnimHandler.SetParameter("Corpus_Anim", "SideWalk", AnimatorControllerParameterType.Bool, false);
        }
        base.OnExit(CTX);
        Statepulse = 0;
    }

    public override void OnInitialize(VisceralStateMachine CTX)
    {
        base.OnInitialize(CTX);
        if(thinkingMain == null)
        {
            thinkingMain = CTX.GetComponentInChildren<Corpus_Thinking_Main_State>();
        }

        if(MovementStrategy == null)
        {
            MovementStrategy = CTX.GetComponentInChildren<IMovementStrategy>();
            MovementStrategy.Initialize(CTX.GetComponentInChildren<KinematicCharacterMotor>(),thinkingMain.GetModel);
        }
    }

    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        base.OnTick(CTX, TickRate);


        // paso 1) calcular el vector de direccion al centro del radio
        Vector3 VectorRadius = Model.transform.position - Target.transform.position;

        float CurrentDistance = VectorRadius.magnitude;
        if (CurrentDistance < 0.01f) return; // evitar division por cero 

        Vector3 RadiusDirection = VectorRadius / CurrentDistance;

        #region depreciated:
        /*
        // paso 2) calcular la distancia del radio
        float RadiusDistance = VectorRadius.magnitude;
        if(RadiusDistance < 0.01f)
        {
            // evitamos calcular con un valor igual a 0
            return;
        }

        // paso 3) velocidad angular

        float AngularVelocity = movementSpeed / RadiusDistance;

        // paso 4) calculamos el desplazamiento en este frame (convertimos a grados )

        float DeltaAngle = AngularVelocity * Mathf.Deg2Rad * (Time.deltaTime * TimeDilationManager.GlobalTimeScale);
        Quaternion FrameRotation = Quaternion.AngleAxis(DeltaAngle, Vector3.up);

        // paso 5) calcular la nueva posicion
        Vector3 NewRelativePosition = FrameRotation * VectorRadius;
        Vector3 NewAbsolutePosition = Target.transform.position + NewRelativePosition;

        // paso 6) calcular el vector de velocidad de desplazamiento (destino - origen / tiempo)
        Vector3 FinalVelocity = (NewAbsolutePosition - Model.transform.position) / (Time.deltaTime * TimeDilationManager.GlobalTimeScale);

        MovementStrategy.UpdateVelocity(FinalVelocity);

        */
        #endregion

        //paso 2) velocidad tangencial

        // notas: Vector3.cross da un vector tangencial a los dos pasados.

        Vector3 TangentDirection;

        // invertir sentido con un flip flop :)
        if (FlipFlopDirection)
        {
            TangentDirection = Vector3.Cross(Model.transform.up, RadiusDirection).normalized;
        }
        else
        {
            TangentDirection = Vector3.Cross(RadiusDirection,Model.transform.up).normalized;
        }

        // paso 3) Calcular nuestra posicion radial deseada

        // nuestra posicion deseada
        Vector3 DesiredPositionOnRadius = Target.transform.position + (RadiusDirection * OrbitRadius);

        // paso 4) calcular la compensacion deseada.

        // este vector determina hacia adonde tenemos que compensar
        Vector3 CorrectionVector = DesiredPositionOnRadius - (Model.transform.position);

        // Paso 5 ) calcular desplazamiento final

        Vector3 FinalDirection = TangentDirection + (CorrectionVector * SpiralDriftCorrection);

        MovementStrategy.UpdateVelocity(FinalDirection.normalized);
    }
}
