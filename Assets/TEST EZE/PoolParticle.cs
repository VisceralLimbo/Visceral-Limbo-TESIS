using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolParticle : MonoBehaviour
{
    public static PoolParticle Instance;

    [Header("config")]
    public GameObject particlePrefab;
    public int poolSize = 10;

    private List<GameObject> pool;

    private void Awake()
    {
        Instance = this;

        pool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(particlePrefab);
            obj.SetActive(false);
            pool.Add(obj);

            //seteo de la lista basicamente. apaga las que instancia para hacer el pool
        }
    }

    public void PlayParticle(Vector3 position)
    {
        foreach (var particle in pool)
        {
            if (!particle.activeInHierarchy) //busco una desactivada 
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
