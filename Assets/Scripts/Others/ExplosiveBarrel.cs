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

    [SerializeField] private AudioClip explosionSound;

    private void Start()
    {
        healthComponent = GetComponent<Health_Component>();
        if (healthComponent != null)
        {
            healthComponent.OnDeath += () => StartCoroutine(DelayedExplosion());
        }
    }

    private IEnumerator DelayedExplosion()
    {
        yield return new WaitForSeconds(1f); // Hecho por Lucas - Time-slicing
        Kaboom();

        CameraShake.instance.ShakeCamera(1f, 1f); // shake de la camara
    }

    private void Kaboom()
    {
        // Instanciar y reproducir todas las partículas del array
        if (explosionParticles != null && explosionParticles.Length > 0)
        {
            foreach (GameObject particlePrefab in explosionParticles)
            {
                if (particlePrefab == null) continue;

                GameObject effectInstance = Instantiate(particlePrefab, transform.position, Quaternion.identity);

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
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
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

}


//
// Script hecho por Lucas Torres
// se encarga de definir el funcionamiento del barril explosivo
//