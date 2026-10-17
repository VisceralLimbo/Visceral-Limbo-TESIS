using System.Collections;
using UnityEngine;

public class FallingStoneSpawner : MonoBehaviour
{
    [Header("Piedra")]
    [SerializeField] private FallingStoneTrap stonePrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 3f;

    private FallingStoneTrap currentStone;

    private void Start()
    {
        if (stonePrefab == null)
        {
            enabled = false;
            return;
        }

        if (spawnPoint == null)
            spawnPoint = transform;

        StartCoroutine(SpawnCycle());
    }

    private IEnumerator SpawnCycle()
    {
        while (true)
        {
            // spawneo la peidra en el spawn
            currentStone = Instantiate(
                stonePrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            // espero a q se rompa
            while (currentStone != null)
                yield return null;

            // delay para el respawn
            yield return new WaitForSeconds(respawnDelay);
        }
    }
}
