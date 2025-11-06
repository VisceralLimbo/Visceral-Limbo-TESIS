using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarDamage : MonoBehaviour
{
    [Header("Configuración de Daño")]
    public float initialDamage = 20f; // daño cuando aparece
    public float dotDamage = 5f;      // daño por segundo
    public float damageTickRate = 0.5f; // tickrate xd
    public float pillarDuration = 3f;   // tiempo del pilar

    private bool hasDoneInitialDamage = false;
    private Collider damageCollider;

    void Start()
    {
        damageCollider = GetComponent<Collider>();
        // corrutina para destruir el pilar
        StartCoroutine(HandlePillarLifetime());
    }

    // ontrigerstay para el dot
    private void OnTriggerStay(Collider other)
    {
        // si colisiono con el jugador lo hago papilla dea
        Player_HealthComp playerHealth = other.GetComponent<Player_HealthComp>();

        if (playerHealth != null)
        {
            // daño q se aplica la primera vez q se collisiona con el pilar (la idea seria que este daño sea elevado asi tiene sentido q sea algo de lo q hay q tener cuidado bue)
            if (!hasDoneInitialDamage)
            {
                // PERDON PATO PERO SIMPLEDAMAGE SERA AHRE
                playerHealth.SimpleDamage(initialDamage);
                hasDoneInitialDamage = true;
            }
        }
    }

    // corrutina para el dot
    private IEnumerator HandlePillarLifetime()
    {
        float timer = 0f;

        // bucle dot
        while (timer < pillarDuration)
        {
            // espero el tickrate q se puso
            yield return new WaitForSeconds(damageTickRate);
            timer += damageTickRate;

            // agarro los colliders q esten dentro del pilar
            Collider[] collidersInArea = Physics.OverlapBox(damageCollider.bounds.center, damageCollider.bounds.extents, Quaternion.identity, LayerMask.GetMask("Player") // con layer para no mandar un moco
            );

            foreach (Collider col in collidersInArea)
            {
                // dot a los q sigan dentro
                Player_HealthComp playerHealth = col.GetComponent<Player_HealthComp>();
                if (playerHealth != null)
                {
                    playerHealth.SimpleDamage(dotDamage);
                }
            }
        }

        // chau pilar
        Destroy(gameObject);
    }
}

