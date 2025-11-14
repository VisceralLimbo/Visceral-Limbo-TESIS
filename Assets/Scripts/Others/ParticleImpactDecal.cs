using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class ParticleImpactDecal : MonoBehaviour
{
    [SerializeField] SoundData S_Data; // pato, datos del sonido que queremos tocar

   [SerializeField] private ParticleSystem particleSystemBlood;
    [SerializeField] private List<GameObject> decalPrefabs = new List<GameObject>();
    [SerializeField] private LayerMask impactLayers;

    private ParticleSystem.Particle[] particles;
    private Vector3[] previousPositions;
    [SerializeField]private Health_Component health;

    [SerializeField, Range(0.001f, 0.1f)]
    private float decalOffset = 0.0025f;

    void Start()
    {
        particles = new ParticleSystem.Particle[particleSystemBlood.main.maxParticles];
        previousPositions = new Vector3[particleSystemBlood.main.maxParticles];

        if(health == null) health = GetComponentInParent<Health_Component>();

        if (health != null)
        {
            health.OnDamaged += OnDamagedHandler;
            health.OnDeath += OnDeathHandler;
        }
        
    }

    void OnDestroy()
    {
        if (health != null)
        {
            health.OnDamaged -= OnDamagedHandler;
            health.OnDeath -= OnDeathHandler;
        }

    }

    void SpawnDecal(RaycastHit hit)
    {
        // URP Decal Projector orienta el LookRotation así
        Quaternion rot = Quaternion.LookRotation(hit.normal, Vector3.up);

        Vector3 pos = hit.point + hit.normal * decalOffset;

        GameObject randomDecal = decalPrefabs[Random.Range(0, decalPrefabs.Count)];
        GameObject decal = Instantiate(randomDecal, pos, rot);
        Destroy(decal, 30f);
    }

    void OnDamagedHandler()
    {
        if (!particleSystemBlood.isPlaying)
            particleSystemBlood.Play();

        RaycastHit hit;
        Vector3 origin = transform.position;
        Vector3 dirForward = -transform.up;

        if (Physics.SphereCast(origin, 0.03f, dirForward, out hit, 2f, impactLayers))
        {
            SpawnDecal(hit);
        }
        else
        {
            if (Physics.SphereCast(origin, 0.03f, Vector3.down, out hit, 5f, impactLayers))
            {
                SpawnDecal(hit);
            }
        }

        SoundSFX();
    }

    private void OnDeathHandler()
    {
        // Instanciar las partículas de sangre como objeto independiente
        GameObject bloodGO = Instantiate(particleSystemBlood.gameObject, transform.position, Quaternion.identity);
        ParticleSystem ps = bloodGO.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
            Destroy(bloodGO, ps.main.duration + ps.main.startLifetime.constantMax);
           
        }
    }

    void LateUpdate()
    {
        int count = particleSystemBlood.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            Vector3 currentPos = particles[i].position;
            Vector3 worldCurrentPos = particleSystemBlood.transform.TransformPoint(currentPos);
            Vector3 worldPrevPos = particleSystemBlood.transform.TransformPoint(previousPositions[i]);

            Vector3 dir = worldCurrentPos - worldPrevPos;
            float distance = dir.magnitude;

            if (distance > 0.001f)
            {
                if (Physics.SphereCast(worldPrevPos, 0.03f, dir.normalized, out RaycastHit hit, distance, impactLayers)
)
                {
                    SpawnDecal(hit);
                    particles[i].remainingLifetime = 0;
                }
            }

            previousPositions[i] = currentPos; 
        }

        particleSystemBlood.SetParticles(particles, count);
    }


    //pato
    void SoundSFX()
    {
        SoundManager.Instance.CreateSound()//comenzamos el sistema de sonido
                                .WithSoundData(S_Data)//cargamos el clip que queremos escuchar
                                .WithPosition(health.transform.position) // en la posicion del objeto
                                .WithRandomPitch(true) // con pitch randomizado
                                .WithSpatialBlend(1,S_Data.MinimunSoundDistance,S_Data.MaximunSoundDistance) // con blendeo espacial / 3D
                                .play(); //enviamos el audio final al manager para tocar
        print("Sound off");
    }
}
