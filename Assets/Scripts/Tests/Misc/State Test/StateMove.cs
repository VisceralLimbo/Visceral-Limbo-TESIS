using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class StateMove : BaseState
{
    [Header("References")]
    [SerializeField] KinematicCharacterMotor _KCC;
    [SerializeField] AnimatorHandler _AnimatorHandler;
    [SerializeField] Transform _Target;
    IMovementStrategy _MovementStrategy;


    [Space]

    [Header("variables")]
    [SerializeField] float _Speed;
    [SerializeField] float _MovementAccel;
    [SerializeField] float _MaxRotationSpeed;
    [SerializeField] float _MinDistance;
    [SerializeField] float _MaxDistance;

    [Header("Sounds")]
    [SerializeField] float _SoundDownTime;
    [SerializeField] float _RandomSoundOffset;
    [SerializeField] SoundData _SoundData;

    Vector3 targetdirection;
    public override void OnInitialize(VisceralStateMachine CTX)
    {
        base.OnInitialize(CTX);
        if(_KCC == null )
        {
            _KCC = CTX.GetComponent<KinematicCharacterMotor>();
            if(_KCC == null)
            {
                _KCC = CTX.GetComponentInChildren<KinematicCharacterMotor>();
            }
            _Target = FindObjectOfType<Player_Movement>().transform;
        }

        if(_MovementStrategy == null)
        {
            if(CTX.gameObject.TryGetComponent<IMovementStrategy>(out IMovementStrategy _Movement))
            {
                _MovementStrategy = _Movement;
            }
            else
            {
                _MovementStrategy = CTX.GetComponentInChildren<IMovementStrategy>();
            }
        }

        if(_AnimatorHandler == null)
        {
            if(CTX.gameObject.TryGetComponent(out AnimatorHandler Handler))
            {
                _AnimatorHandler = Handler;
            }
            else
            {
                _AnimatorHandler = CTX.gameObject.GetComponentInChildren<AnimatorHandler>();
            }
        }

        _MovementStrategy.Initialize(_KCC, CTX.gameObject);
    }

    public override void OnEnter(VisceralStateMachine CTX)
    {
        base.OnEnter(CTX);
        _MovementStrategy.SetActiveState(true);
    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        stateMachine.SetGlobalCondition("Moving", false);
        _MovementStrategy.KillAllMovement();
    }


    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        if (!isActiveAndEnabled || !CTX.gameObject.activeSelf)
        {
            return;
        }

        Vector3 TargetDirection = _Target.transform.position - _KCC.Capsule.transform.position;

        _MovementStrategy.UpdateVelocity(TargetDirection);

        if (_AnimatorHandler != null)
        {
            _AnimatorHandler.SetParameter("Wretched", "IsMoving"
                , AnimatorControllerParameterType.Trigger);
        }



        //CALCULO DE SITUACION
        var Distance = Vector3.Distance(_KCC.Capsule.transform.position, _Target.transform.position);
        if (Distance <= _MinDistance)
        {
            stateMachine.SetGlobalCondition("Melee", true);

        }
        if (Distance > _MaxDistance)
        {
            stateMachine.SetGlobalCondition("Moving", false);
        }
        else
        {
            stateMachine.SetGlobalCondition("Moving", true);
        }

        if(_SoundData != null && _SoundData.Clip != null)
        {
            EmitSounds();
        }
      
    }

    float pulseLifeTime;
    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        
        if(pulseLifeTime < _MinStateLifetime) //stopgap to avoid the state from switching to fast
        {
            pulseLifeTime += Time.unscaledDeltaTime;
            TO = null;
            return false;
        }

        if (GlobalParams != null)
        {
            if(Mytransitions.Length> 0)
            {
                foreach(var transition in Mytransitions) 
                {
                    if(transition.ShouldTransition(GlobalParams, out BaseState _TO))
                    {
                        print("Should transition to " + _TO.name);
                        TO = _TO;

                        pulseLifeTime = 0;
                        return true;
                    }
                }
            }
        }

        //NO SE PUDO TRANSICIONAR
        TO = null;
        return false;
    }


    float SoundPulse = 0;

    private SoundEmitter emitter;
    private void EmitSounds()
    {
        if (SoundManager.Instance == null) return;

        if( emitter != null && emitter.isActiveAndEnabled)
        {
            emitter.transform.position = _KCC.Capsule.transform.position;
            return;
        }


        if (SoundPulse < _SoundDownTime + _RandomSoundOffset)
        {
            SoundPulse += Time.deltaTime * TimeDilationManager.GlobalTimeScale;
            return;
        }
        else
        {
            _RandomSoundOffset = Random.Range(-_RandomSoundOffset, _RandomSoundOffset + 0.1f);

            SoundPulse = 0;

            SoundManager.Instance.CreateSound()
            .WithSoundData(_SoundData)
            .WithRandomPitch(true)
            .WithPosition(_KCC.Capsule.transform.position)
            .WithSpatialBlend(_SoundData.SpatialBlend, _SoundData.MinimunSoundDistance, _SoundData.MaximunSoundDistance)
            .play(out SoundEmitter Emit);

            emitter = Emit;

        }
    

    }
}
