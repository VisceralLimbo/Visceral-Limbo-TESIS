using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//7 años para entender que tenia que pedir el componente xddxd me tiraba error siempre dxdxdxddd
[RequireComponent(typeof(Health_Component))]

public class EnemyLoot : MonoBehaviour
{
    [Header("Drop settings")]
    [SerializeField] private GameObject healthDropPrefab;
    [SerializeField, Range(0f, 1f)] private float dropChance = 0.3f; // lo hice slider para testear pero si quieren lo ponen en una fija (probe en 0.3f y me salio 1 sola en 2 salas xd)
    [SerializeField] private Vector3 dropOffset = new Vector3(0, -0.5f, 0); //les pregunte si era como en el deathmatch de valorant me dijeron q si asi q hice q flote un toque para q no quede tosco en el piso

    [SerializeField] private int orbs = 5;
    [SerializeField] private float radioSpawn = 1.5f;

    private Health_Component healthComp;

    private void Awake()
    {
        healthComp = GetComponent<Health_Component>();
    }

    private void OnEnable()
    {
        healthComp.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        healthComp.OnDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        // para q el drop sea solo en los enemigos y no con el jugador
        if (healthComp.Context != null && healthComp.Context.faction == FactionID.Player)
            return;

        if (healthDropPrefab != null && Random.value <= dropChance)
        {
            for (int i = 0; i < orbs; i++)
            {
                Vector2 circle = Random.insideUnitCircle * radioSpawn;
                Vector3 spawnPos = transform.position + dropOffset + new Vector3(circle.x, 0, circle.y);

                GameObject orb = Instantiate(healthDropPrefab, spawnPos, Quaternion.identity);

                OrbMovement mov = orb.GetComponent<OrbMovement>();
                if (mov != null)
                {
                    mov.Init();
                }
            }
        }
    }
}
