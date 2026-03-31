using UnityEngine;

public class GulagSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public GameObject playerTarget;

    private bool Spawned = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !Spawned)
        {
            EnemySpawn();
        }
    }

    void EnemySpawn()
    {
        if (enemyPrefab == null) return;

        Spawned = true;

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
