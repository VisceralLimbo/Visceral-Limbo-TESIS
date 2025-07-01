using UnityEngine;

public class FireballTrap : MonoBehaviour
{
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 2f; 
    [SerializeField] private float fireballSpeed = 10f; 
    [SerializeField] private float fireballDamage = 10f; 
    [SerializeField] private float fireballLifetime = 5f; 

    private float fireTimer = 0f; 

    void Update()
    {
        
        fireTimer += Time.deltaTime;

        
        if (fireTimer >= fireRate)
        {
            Fire();
            fireTimer = 0f; 
        }
    }

    void Fire()
    {
        
        if (fireballPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Fireball prefab or fire point not assigned in Turret script!");
            return;
        }

       
        GameObject fireball = Instantiate(fireballPrefab, firePoint.position, firePoint.rotation);

        
        Rigidbody rb = fireball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = firePoint.forward * fireballSpeed;
        }

        
        Fireball fireballScript = fireball.AddComponent<Fireball>();
        fireballScript.damage = fireballDamage;

        
        Destroy(fireball, fireballLifetime);
    }
}


public class Fireball : MonoBehaviour
{
    public float damage; 

    private void OnCollisionEnter(Collision collision)
    {
        
        Health_Component healthComponent = collision.gameObject.GetComponent<Health_Component>();

        if (healthComponent != null)
        {
            
            healthComponent.SimpleDamage(damage);
        }

        
        Destroy(gameObject);
    }
}
