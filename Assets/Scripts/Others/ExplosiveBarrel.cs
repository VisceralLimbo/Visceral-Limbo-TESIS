using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Unity.Collections;

public class ExplosiveBarrel : MonoBehaviour
{
    [SerializeField] PlayerContext _Context;
    public float explosionRadius;
    public float damage;
    public float knockbackForce;
    public LayerMask targetLayer;
    [SerializeField] AudioSource _BarrelLaughs;

    private Health_Component healthComponent;

    [SerializeField] private GameObject[] explosionParticles; // Partículas de explosión

    //tuve q hacerlo audiosource porque suaban clips no mas y no podia asignar el mixer
    [SerializeField] private AudioSource explosionSound;

    [SerializeField] private Renderer barrelRenderer;

    private Material barrelMaterial;

    private void Start()
    {
        healthComponent = GetComponent<Health_Component>();
        if (healthComponent != null)
        {
            healthComponent.OnDeath += () => StartCoroutine(DelayedExplosion());
        }

        if (barrelRenderer != null)
        {
            barrelMaterial = barrelRenderer.material;
        }
    }

    private IEnumerator DelayedExplosion()
    {
        float explosionDelay = 1f;
        float elapsed = 0f;

        while (elapsed < explosionDelay)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / explosionDelay; 

            
            if (barrelMaterial != null)
            {
                barrelMaterial.SetFloat("_ExplosionTime", normalizedTime);
            }

            yield return null; 
        }

        
        if (barrelMaterial != null)
        {
            barrelMaterial.SetFloat("_ExplosionTime", 1f);
        }

        Kaboom();

        //multiplico el shakesize por la intensidad
        CameraShake.instance.ShakeCamera(0.75f, 1f * CameraShakeIntensity.currentIntensity, ShakeType.AllDirections);
    }

    private void Kaboom()
    {
        
        if (explosionParticles != null && explosionParticles.Length > 0)
        {
            foreach (GameObject particlePrefab in explosionParticles)
            {
                if (particlePrefab == null) continue;

                GameObject effectInstance = Instantiate(particlePrefab);
                effectInstance.transform.position = transform.position;
                effectInstance.transform.rotation = particlePrefab.transform.rotation; 



                var allParticles = effectInstance.GetComponentsInChildren<ParticleSystem>(true);

                foreach (var ps in allParticles)
                {
                    ps.gameObject.SetActive(true);
                    ps.Play(true);
                }
            }
        }

        if (explosionSound != null)
        {
            explosionSound.Play();
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, targetLayer);
        List<Health_Component> healthTargets = new List<Health_Component>();

        foreach (var col in colliders)
        {
            var hc = col.GetComponent<Health_Component>();
            if (hc != null && hc != healthComponent)
            {
                healthTargets.Add(hc);
            }
        }

        var orderedTargets = healthTargets
            .OrderBy(x => Vector3.Distance(transform.position, x.transform.position))
            .ToList();

        var filteredTargets = orderedTargets
            .Where(x => Vector3.Distance(transform.position, x.transform.position) < explosionRadius * 0.75f)
            .ToList();

        if (filteredTargets.Count <= 0)
        {
            Destroy(this.gameObject);
            return;
        }

        foreach (var enemy in filteredTargets)
        {
            Vector3 dir = (enemy.transform.position - transform.position).normalized;
            var damageTuple = new Tuple<Vector3, float, float>(dir, damage, knockbackForce);

            DamageScore DamageDT = new DamageScore();
            DamageDT.Attacker = _Context;
            DamageDT.DamageAmount = damage;
            DamageDT.ElementalDamage = ElementType.Fire;
            DamageDT.FactionID = _Context.faction;
            DamageDT.ScoreTags = ScoreFlags.Explosion;

            enemy.TakeDamageWithKnockback(damageTuple.Item1, damageTuple.Item3, DamageDT);

        }

        print(filteredTargets.Count);

        if (filteredTargets.Count > 0)
        {
            _BarrelLaughs.transform.SetParent(null);
            _BarrelLaughs.Play();
        }

        Destroy(this.gameObject);
    }

    /* private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f); 
        Gizmos.DrawSphere(transform.position, explosionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius * 0.75f); 
    } */

}


//
// Script hecho por Lucas Torres
// se encarga de definir el funcionamiento del barril explosivo
//