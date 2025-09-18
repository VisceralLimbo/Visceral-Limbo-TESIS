using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [SerializeField] private float damage = 10f;         // dmg de la trampa
    [SerializeField] private float damageInterval = 1f;  // cd del dmg

    private float nextDamageTime = 0f;

    private void OnTriggerStay(Collider other)
    {
        if (Time.time < nextDamageTime) return;

        if (other.TryGetComponent(out Health_Component HPComp))
        {
            // aplico dmg
            HPComp.SimpleDamage(damage);
            nextDamageTime = Time.time + damageInterval;
        }
    }
}
