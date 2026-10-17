
using System.Collections;
using UnityEngine;

public class FallingStoneTrap : MonoBehaviour
{
    [Header("Caida")]
    [SerializeField] private float timeBeforeFall = 2f;
    [SerializeField] private float colliderActivationDelay = 1f;

    [Header("Particulas")]
    [SerializeField] private GameObject explosionParticles;

    [Header("Daño")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private PlayerContext playerContext;

    private Rigidbody rb;
    private Collider stoneCollider;

    private bool isFalling;
    private bool hasBroken;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        stoneCollider = GetComponent<Collider>();

        // aparece sin collider y no cae
        stoneCollider.enabled = false;

        rb.isKinematic = true;
        rb.useGravity = true;
    }

    private void Start()
    {
        // espera para empezar la caida
        Invoke(nameof(StartFalling), timeBeforeFall);
    }

    private void StartFalling()
    {
        if (hasBroken)
            return;

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.WakeUp();

        isFalling = true;

        // activo el collider desp del delay
        StartCoroutine(EnableColliderAfterDelay());
    }

    private IEnumerator EnableColliderAfterDelay()
    {
        yield return new WaitForSeconds(colliderActivationDelay);

        if (!hasBroken && stoneCollider != null)
            stoneCollider.enabled = true;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (!isFalling || hasBroken)
            return;

        hasBroken = true;
        isFalling = false;

        // dmg
        ApplyDamage(collision.collider);

        // impacto para las particulas
        Vector3 impactPosition = transform.position;

        if (collision.contactCount > 0)
            impactPosition = collision.GetContact(0).point;

        // genero particulasxd
        if (explosionParticles != null)
        {
            GameObject particles = Instantiate(
                explosionParticles,
                impactPosition,
                Quaternion.identity
            );

            ParticleSystem[] systems =
                particles.GetComponentsInChildren<ParticleSystem>();

            float maxLifetime = 0f;

            foreach (ParticleSystem ps in systems)
            {
                ps.Play();

                float lifetime =
                    ps.main.duration +
                    ps.main.startLifetime.constantMax;

                if (lifetime > maxLifetime)
                    maxLifetime = lifetime;
            }

            Destroy(particles, Mathf.Max(maxLifetime + 1f, 5f));
        }

        // f piedra
        Destroy(gameObject);
    }

    //dmg como cualquier otra trampa
    private void ApplyDamage(Collider target)
    {
        if (target == null)
            return;

        bool canTakeDamage =
            target.GetComponent<IDamageable>() != null ||
            target.GetComponent<Health_Component>() != null;

        if (!canTakeDamage)
            return;

        DamageScore damageScore = new DamageScore()
        {
            Attacker = playerContext,
            DamageAmount = damage,
            FactionID = FactionID.LimboTrap
        };

        DamageDispatcher.ProcessContinuousHit(
            target,
            ref damageScore,
            null,
            0,
            new System.Collections.Generic.Dictionary<Health_Component, float>(),
            1f,
            false
        );
    }

}
