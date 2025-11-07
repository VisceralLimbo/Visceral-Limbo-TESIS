using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAbility : BaseState,IStateEnergyCost
{
    [Header("config del pilarr")]
    public GameObject playerTarget;
    public GameObject indicatorPrefab;
    public float timeBetweenPillars = 0.5f; // cada cuanto se instancia un pilar
    public int numberOfPillars = 5; // cuantos pilares en el ataque (estaria bueno jugar con esto y hace q mientras menos vida tenga sean mas pilares)
    public float indicatorDuration = 1.5f; // tiempo del quad (indicador) hasta q muere
    public float timeBetweenAttacks = 10f; // por ahora es un ataque automatico a modo de testeo (se ejecuta cada 10seg)
    private bool isAttacking = false;

    public LayerMask groundLayer; // layer del piso
    public float raycastDistance = 100f; // distancia del raycast

    [Header("Configuracion de state machine")]
    [SerializeField] private string TransitionKey;

    [SerializeField] private int EnergyCost;

    [SerializeField] Corpus_Thinking_Main_State Thinker;

    public override void OnInitialize(VisceralStateMachine CTX)
    {
        base.OnInitialize(CTX);
        Thinker = GetComponentInParent<Corpus_Thinking_Main_State>();

    }

    public override void OnEnter(VisceralStateMachine CTX)
    {
        base.OnEnter(CTX);
        isAttacking = false;
        Thinker.KillMovement();

        Thinker.DeactivateEnergy(true);
        StartPillarAttack();
    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        base.OnExit(CTX);
    }

    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        if (isAttacking)
        {
            TO = null;
            return false;
        }

        stateMachine.SetGlobalCondition(TransitionKey, false);

        print("Ending pilar attack");
        return base.EvaluateTransitions(GlobalParams, out TO);
    }

    // se llama a esta para inciar el ataque
    public void StartPillarAttack()
    {
        if (!isAttacking)
        {
            StartCoroutine(PillarAttackCoroutine());
        }
    }

    private IEnumerator PillarAttackCoroutine()
    {
        isAttacking = true;

        // aca iria una animacion si tuvieramos :v
        yield return new WaitForSeconds(0.5f); // pausita para la anim :v

        for (int i = 0; i < numberOfPillars; i++)
        {
            // agarro la pos del player
            Vector3 spawnPoint = playerTarget.transform.position;

            RaycastHit hit;
            Vector3 finalSpawnPosition; // pos para el instance

            // raycast al piso
            if (Physics.Raycast(spawnPoint, Vector3.down, out hit, raycastDistance, groundLayer))
            {
                // si choca uso el hitpoint
                finalSpawnPosition = hit.point;

                // levanto un toque porque si no a veces pasa ese parpadeo de que estan encimados los objetos
                finalSpawnPosition.y += 0.01f;
            }
            else
            {
                // si no hay piso uso la del player
                Debug.LogWarning("no encontre suelo uso el player"); // no deberia pasar casi nunca esto igual xd
                finalSpawnPosition = spawnPoint;
            }

            // rotacion del quad
            Quaternion flatRotation = Quaternion.Euler(90f, 0f, 0f);

            // se instancia el indicador usando la pos q se calculo con el raycast
            GameObject indicator = Instantiate(indicatorPrefab, finalSpawnPosition, flatRotation);

            // paso las cosas al indicador
            FlamePillarIndicator pillarScript = indicator.GetComponent<FlamePillarIndicator>();
            if (pillarScript != null)
            {
                pillarScript.SetupIndicator(finalSpawnPosition, indicatorDuration);
            }

            // espero el tiempo entre pilar
            yield return new WaitForSeconds(timeBetweenPillars);
        }

        isAttacking = false;
    }

    public string GetTransitionKey()
    {
        return TransitionKey;
    }

    public void SetCost(float NewCost)
    {
        EnergyCost = (int)NewCost;
    }

    public int GetCost()
    {
        return EnergyCost;
    }

    public BaseState GetState()
    {
        return this;
    }
}

