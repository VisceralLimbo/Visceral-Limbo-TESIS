using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpus_Thinking_Main_State : BaseState
{
    [Header("References")]
    [SerializeField] Transform Target;
    [SerializeField] GameObject Model;

    [Header("Variables")]
    [Tooltip("Distancia minima para realizar un ataque, recubre tanto melee como rango")]
    [SerializeField] float MinimumAttackRange;

    [Tooltip("Energia del Corpus, determina cuantas acciones puede hacer")]
    [SerializeField] int Energy;

    [Tooltip("Energia maxima del Corpus")]
    [SerializeField] int MaxEnergy;

    [Tooltip("Regeneracion del Corpus en segundos")]
    [SerializeField] float EnergyRefillRate;

    private bool _StopRegeneratingEnergy;
    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        if(Vector3.Distance(Target.transform.position,Model.transform.position) <= MinimumAttackRange)
        {
            stateMachine.SetGlobalCondition("InRange", true);
        }
        else
        {
            stateMachine.SetGlobalCondition("InRange", false);
        }
        return base.EvaluateTransitions(GlobalParams, out TO);
    }

    public override void OnDeInitialize(VisceralStateMachine CTX)
    {
        
    }

    public override void OnEnter(VisceralStateMachine CTX)
    {
        if(Target == null)
        {
            Target = FindObjectOfType<Player_Movement>().transform;
        }

        if(Energy < MaxEnergy)
        {
            StartCoroutine(EnergyCoroutine());
        }
    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        if (_StopRegeneratingEnergy)
        {
            StopCoroutine(EnergyCoroutine());
        }
    }

    public override void OnInitialize(VisceralStateMachine CTX)
    {
        Target = FindObjectOfType<Player_Movement>().transform;
        stateMachine = CTX;
    }

    float EnergyPulse = 0;
    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        if(Target == null)
        {
            return;
        }

        // PASO 1: Calcular la distancia al jugador del Corpus
        float DistanceToPlayer = Vector3.Distance(Target.transform.position,Model.transform.position);
        
        // PASO 2: Determinar si el Corpus esta a distancia como para realizar melee.
        if(DistanceToPlayer <= MinimumAttackRange && Energy > 0)
        {
            // llamar ataque
            return;
        }
        else if(DistanceToPlayer <= MinimumAttackRange && Energy <= 0)
        {
            // vamos a idle
            return;
        }

        // PASO 3: Determinar si vamos a Idle o movernos
        if(Target != null)
        {
            //movernos
            return;
        }
        else
        {
            // idle
            return;
        }
       


    }

    private IEnumerator EnergyCoroutine()
    {
        while(Energy < MaxEnergy)
        {
            if (EnergyPulse < EnergyRefillRate)
            {
                EnergyPulse += Time.deltaTime * TimeDilationManager.GlobalTimeScale;

            }
            else
            {
                EnergyPulse = 0;
                Energy++;
            }

            yield return null;
        }
    }

    public Transform GetTargetTransform { get { return Target.transform; } }

    public float GetCurrentEnergy { get { return Energy; } }

    public GameObject GetModel { get { return Model; } }

}
