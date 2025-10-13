using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PoolParticle : MonoBehaviour
{
    public static PoolParticle Instance;

    [Header("config")]
    public GameObject normalParticlePrefab; // particula normal
    public GameObject bleedParticlePrefab;  // particula bleed
    public int poolSize = 10;

    private List<GameObject> normalPool; // pool normal
    private List<GameObject> bleedPool;  // pool bleed

    private void Awake()
    {
        Instance = this;

        // pool normal
        normalPool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(normalParticlePrefab);
            obj.SetActive(false);
            normalPool.Add(obj);
        }

        // pool bleed
        bleedPool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(bleedParticlePrefab);
            obj.SetActive(false);
            bleedPool.Add(obj);
        }
    }

    // play a la q quiero
    public void PlayParticle(Vector3 position, bool isBleedEffect)
    {
        // elijo dependiendo si esta activo o no el bleed
        List<GameObject> targetPool = isBleedEffect ? bleedPool : normalPool;

        foreach (var particle in targetPool)
        {
            if (!particle.activeInHierarchy) // agarro una apagada
            {
                // muevo la particula a la pos q quiero
                particle.transform.position = position;

                // on
                particle.SetActive(true);

                // playxd
                particle.GetComponent<ParticleSystem>().Play();

                // la desactivo cuando termina
                StartCoroutine(DisableAfterTime(particle, particle.GetComponent<ParticleSystem>().main.duration));
                return;
            }
        }
    }

    private IEnumerator DisableAfterTime(GameObject obj, float time)
    {
        //la desactivo y vuelve al pool
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
    }
}
