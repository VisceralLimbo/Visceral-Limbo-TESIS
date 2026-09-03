using UnityEngine;

public class DeathFragments : MonoBehaviour
{
    [Header("Fragmentation Settings")]
    public GameObject fracturedBoxPrefab;
    public float destroyFragmentsAfter = 5f;
    public float explosionForce = 4f;

    [Header("Breaking Particles")]
    public GameObject breakingParticlesPrefab;

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

        
        if (breakingSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.CreateSound()
                .WithSoundData(breakingSound)
                .WithRandomPitch(true)
                .WithPosition(transform.position)
                .WithSpatialBlend(1f, 1f, 50f)
                .play();
        }

        
        if (breakingParticlesPrefab != null)
        {
            GameObject particles = Instantiate(
                breakingParticlesPrefab,
                transform.position,
                Quaternion.identity
            );

            ParticleSystem particleSystem = particles.GetComponent<ParticleSystem>();

            if (particleSystem != null)
            {
                particleSystem.Play();

                // Destruir las partículas cuando termine su reproducción
                Destroy(particles, particleSystem.main.duration + particleSystem.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(particles, 2f);
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
                randomDir * Random.Range(explosionForce * 0.8f, explosionForce * 1.2f),
                ForceMode.Impulse
            );
        }

        Destroy(fragments, destroyFragmentsAfter);
    }
}