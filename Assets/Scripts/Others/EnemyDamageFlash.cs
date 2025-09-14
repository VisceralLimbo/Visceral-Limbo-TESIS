using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HostileNPC_HealthComp))]
public class EnemyDamageFlash : MonoBehaviour
{
    public Renderer rend;
    public float flashDuration = 0.2f;
    private Material mat;
    private float flashTimer;

    private HostileNPC_HealthComp healthComp;

    public Transform particleSpawn; // pos para las particulas

    void Start()
    {
        if (rend == null)
            rend = GetComponentInChildren<Renderer>(); 

        mat = rend.material;

        
        healthComp = GetComponent<HostileNPC_HealthComp>();

        
        if (healthComp != null)
            healthComp.OnDamaged += TriggerFlash;
    }

    void TriggerFlash()
    {
        //activo particulas 
        Vector3 hitPosition = particleSpawn.position;
        PoolParticle.Instance.PlayParticle(hitPosition);
        flashTimer = flashDuration; 
    }

    void Update()
    {
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            float t = flashTimer / flashDuration;
            mat.SetFloat("_HitFlash", 1 - t); 
        }
        else
        {
            mat.SetFloat("_HitFlash", 0);
        }
    }

    private void OnDestroy()
    {
        if (healthComp != null)
            healthComp.OnDamaged -= TriggerFlash;
    }
}
