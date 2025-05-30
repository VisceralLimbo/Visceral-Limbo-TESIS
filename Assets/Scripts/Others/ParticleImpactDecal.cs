using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleImpactDecal : MonoBehaviour
{
    [SerializeField] SoundData S_Data; // pato, datos del sonido que queremos tocar

   [SerializeField] private ParticleSystem particleSystemBlood;
   [SerializeField] private GameObject decalPrefab;
   [SerializeField] private LayerMask impactLayers;

    private ParticleSystem.Particle[] particles;
    private Vector3[] previousPositions;
    [SerializeField]private Health_Component health;

    [SerializeField, Range(0.001f, 0.1f)]
    private float decalOffset = 0.01f;

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

    void OnDamagedHandler()
    {
        if (!particleSystemBlood.isPlaying)
            particleSystemBlood.Play();

        RaycastHit hit;
        Vector3 origin = transform.position;
        Vector3 direction = -transform.up;

        if (Physics.Raycast(origin, direction, out hit, 2f, impactLayers))
        {
           Instantiate(decalPrefab, hit.point + hit.normal * 0.05f, Quaternion.LookRotation(hit.normal));
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
                if (Physics.Raycast(worldPrevPos, dir.normalized, out RaycastHit hit, distance, impactLayers))
                {
                    Vector3 decalPos = hit.point - hit.normal * 0.01f;
                    Quaternion decalRot = Quaternion.LookRotation(-hit.normal);

                    GameObject decal = Instantiate(decalPrefab, decalPos, decalRot);
                    Destroy(decal, 30f);

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
                                .WithSpatialBlend(1) // con blendeo espacial / 3D
                                .play(); //enviamos el audio final al manager para tocar
        print("Sound off");
    }
}
