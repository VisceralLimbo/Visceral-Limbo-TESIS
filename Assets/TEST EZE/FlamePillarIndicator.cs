using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlamePillarIndicator : MonoBehaviour
{
    [Header("config del quad (indicador)")]
    [SerializeField] private GameObject flamePillarVisuals;
    public float scaleSpeed = 3f; //escalado
    public float pillarActiveDuration = 3f; // tiempo activo

    private float duration; // viene del jefe el tiempo
    private float startTime; // para saber el tiempo q paso

    // llamado en boos ability
    public void SetupIndicator(Vector3 pos, float dur)
    {
        // guardo el tiempo de inicio para el escalado
        startTime = Time.time;
        duration = dur;
        StartCoroutine(ExecutePillar());
    }

    private void Update()
    {
        // escalo si la corrutina no se termino, y el time - startime es para saber el tiempo q paso
        float timeElapsed = Time.time - startTime;

        // de 0 a 1
        float t = timeElapsed / duration;

        // aplico escala con lerp para ir de 0 a 1 y t se multiplica por la escala para setear que tan rapido crece (siento que tiene q ser mas rapido pero se ve)
        transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t * scaleSpeed);
    }

    private IEnumerator ExecutePillar()
    {
        // espero tiempo para q el player tenga chance
        yield return new WaitForSeconds(duration);

        // activo pilar
        if (flamePillarVisuals != null)
        {
            Instantiate(flamePillarVisuals, transform.position, Quaternion.identity);
        }

        // espero que termine la vida del pilar
        yield return new WaitForSeconds(pillarActiveDuration);

        // chau todo
        Destroy(gameObject);
    }
}

