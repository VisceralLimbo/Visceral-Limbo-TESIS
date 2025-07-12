using System.Threading;
using UnityEngine;

public class FireballTrap : MonoBehaviour
{
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 2f; 
    [SerializeField] private float fireballSpeed = 10f; 
    [SerializeField] private float fireballDamage = 10f; 
    [SerializeField] private float fireballLifetime = 5f;
    [SerializeField] private ParticleSystem fireballParticle;
    [SerializeField] private PlayerContext Context;

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
            Debug.LogWarning("Fireball prefab or fire point not assigned in Turret script");
            return;
        }

        GameObject fireball = Instantiate(fireballPrefab, firePoint.position, firePoint.rotation);
        fireball.GetComponent<Fireball>().Initiliaze(Context);
    }
}
