using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("dmg")]
    [SerializeField] private float damage = 10f;         // dmg de la trampa
    [SerializeField] private float damageInterval = 1f;  // cd del dmg

    [Header("sube y baja")]
    [SerializeField] private float moveHeight = 0.5f;   // cuanto suben y bajan
    [SerializeField] private float moveSpeed = 2f;      // velocidad del movimiento

    private float nextDamageTime = 0f;
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // movimiento arriba y abajo
        float newY = startPos.y + Mathf.Sin(Time.time * moveSpeed) * moveHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    private void OnTriggerStay(Collider other)
    {
        // solo hacen dmg si estan arriba
        if (transform.position.y <= startPos.y) return;

        if (Time.time < nextDamageTime) return;

        if (other.TryGetComponent(out Health_Component HPComp))
        {
            // aplico dmg
            HPComp.SimpleDamage(damage);
            nextDamageTime = Time.time + damageInterval;
        }
    }
}
