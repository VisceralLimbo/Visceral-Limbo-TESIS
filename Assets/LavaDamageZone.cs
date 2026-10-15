
using UnityEngine;

public class LavaDamageZone : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField, Min(0f)] private float damageAmount = 10f;
    [SerializeField, Min(0.01f)] private float damageInterval = 1f;
    [SerializeField] private bool damageImmediately = true;

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    private float damageTimer;
    private Health_Component playerHealth;
    private Collider zoneCollider;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider>();

        if (zoneCollider == null)
        {
            enabled = false;
            return;
        }

        zoneCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerHealth =
            other.GetComponentInParent<Health_Component>();

        damageTimer = damageImmediately ? 0f : damageInterval;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (playerHealth == null)
            playerHealth = other.GetComponentInParent<Health_Component>();

        if (playerHealth == null)
            return;

        damageTimer += Time.deltaTime;

        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;
            ApplyDamage(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerHealth = null;
        damageTimer = 0f;
    }

    private void ApplyDamage(Collider playerCollider)
    {
        DamageScore damageScore = new DamageScore()
        {
            Attacker = null,
            DamageAmount = damageAmount,
            FactionID = FactionID.LimboTrap
        };

        DamageDispatcher.ProcessContinuousHit(
            playerCollider,
            ref damageScore,
            null,
            0,
            new System.Collections.Generic.Dictionary<Health_Component, float>(),
            0f,
            false
        );
    }
}
