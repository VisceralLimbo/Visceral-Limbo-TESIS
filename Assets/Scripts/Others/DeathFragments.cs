using UnityEngine;

public class DeathFragments : MonoBehaviour
{
    [Header("Fragmentation Settings")]
    public GameObject fracturedBoxPrefab;
    public float destroyFragmentsAfter = 5f;
    public float explosionForce = 4f;

    [Header("Breaking Particles")]
    public GameObject breakingParticlesPrefab;

    [Tooltip("Todos los puntos donde aparecerán las partículas al mismo tiempo.")]
    public Transform[] particleSpawnPoints;

    private Health_Component healthComponent;

    [SerializeField] private SoundData breakingSound;

    private void Awake()
    {
        healthComponent = GetComponent<Health_Component>();
    }

    private void OnEnable()
    {
        if (healthComponent != null)
        {
            healthComponent.OnDeath += SpawnFragments;
        }
    }

    private void OnDisable()
    {
        if (healthComponent != null)
        {
            healthComponent.OnDeath -= SpawnFragments;
        }
    }

    private void SpawnFragments()
    {
        if (fracturedBoxPrefab == null) return;

        // Sonido
        if (breakingSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.CreateSound()
                .WithSoundData(breakingSound)
                .WithRandomPitch(true)
                .WithPosition(transform.position)
                .WithSpatialBlend(1f, 1f, 50f)
                .play();
        }

        // PARTÍCULAS
        if (breakingParticlesPrefab != null)
        {
            // Si hay puntos configurados, crear partículas en TODOS
            if (particleSpawnPoints != null && particleSpawnPoints.Length > 0)
            {
                foreach (Transform spawnPoint in particleSpawnPoints)
                {
                    if (spawnPoint == null)
                        continue;

                    SpawnParticles(spawnPoint.position, spawnPoint.rotation);
                }
            }
            else
            {
                // Si no hay puntos configurados, usar el centro del objeto
                SpawnParticles(transform.position, Quaternion.identity);
            }
        }

        // Spawn de los fragmentos
        GameObject fragments = Instantiate(
            fracturedBoxPrefab,
            transform.position,
            transform.rotation
        );

        foreach (Rigidbody rb in fragments.GetComponentsInChildren<Rigidbody>())
        {
            Vector3 randomDir =
                (rb.transform.position - transform.position).normalized
                + Vector3.up * 0.3f;

            rb.AddForce(
                randomDir * Random.Range(
                    explosionForce * 0.8f,
                    explosionForce * 1.2f
                ),
                ForceMode.Impulse
            );
        }

        Destroy(fragments, destroyFragmentsAfter);
    }

    private void SpawnParticles(Vector3 position, Quaternion rotation)
    {
        GameObject particles = Instantiate(
            breakingParticlesPrefab,
            position,
            rotation
        );

        ParticleSystem particleSystem = particles.GetComponent<ParticleSystem>();

        if (particleSystem != null)
        {
            particleSystem.Play();

            Destroy(
                particles,
                particleSystem.main.duration +
                particleSystem.main.startLifetime.constantMax
            );
        }
        else
        {
            Destroy(particles, 2f);
        }
    }
}