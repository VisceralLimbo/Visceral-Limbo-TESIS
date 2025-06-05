using UnityEngine;

public class DeathFragments : MonoBehaviour
{
    [Header("Fragmentation Settings")]
    public GameObject fracturedBoxPrefab;
    public float destroyFragmentsAfter = 5f;
    public float explosionForce = 4f;

    private Health_Component healthComponent;

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

        GameObject fragments = Instantiate(fracturedBoxPrefab, transform.position, transform.rotation);

        foreach (Rigidbody rb in fragments.GetComponentsInChildren<Rigidbody>())
        {
            Vector3 randomDir = (rb.transform.position - transform.position).normalized + Vector3.up * 0.3f;
            rb.AddForce(randomDir * Random.Range(explosionForce * 0.8f, explosionForce * 1.2f), ForceMode.Impulse);
        }

        Destroy(fragments, destroyFragmentsAfter);
    }
}

