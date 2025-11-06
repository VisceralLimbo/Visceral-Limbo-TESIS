using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAbility : MonoBehaviour
{
    [Header("config del pilarr")]
    public GameObject playerTarget;
    public GameObject indicatorPrefab;
    public float timeBetweenPillars = 0.5f; // cada cuanto se instancia un pilar
    public int numberOfPillars = 5; // cuantos pilares en el ataque (estaria bueno jugar con esto y hace q mientras menos vida tenga sean mas pilares)
    public float indicatorDuration = 1.5f; // tiempo del quad (indicador) hasta q muere
    public float timeBetweenAttacks = 10f; // por ahora es un ataque automatico a modo de testeo (se ejecuta cada 10seg)
    private bool isAttacking = false;

    private void Start()
    {
        // inicio corrutina
        StartCoroutine(AutoAttackCoroutine());
    }

    private IEnumerator AutoAttackCoroutine()
    {
        // bucle para q se repita el ataque (esto no iria asi en el boss)
        while (true)
        {
            // lanzo el ataque
            StartPillarAttack();

            // espero los 10seg para empezar de nuevo
            yield return new WaitForSeconds(timeBetweenAttacks);
        }
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
            Vector3 targetPosition = playerTarget.transform.position;

            // rotacion del quad 
            Quaternion flatRotation = Quaternion.Euler(90f, 0f, 0f);

            // se instacia el indicador
            GameObject indicator = Instantiate(indicatorPrefab, targetPosition, flatRotation);


            // paso las cosas al indicador
            FlamePillarIndicator pillarScript = indicator.GetComponent<FlamePillarIndicator>();
            if (pillarScript != null)
            {
                pillarScript.SetupIndicator(targetPosition, indicatorDuration);
            }

            // espero el tiempo entre pilar
            yield return new WaitForSeconds(timeBetweenPillars);
        }

        isAttacking = false;
    }
}

