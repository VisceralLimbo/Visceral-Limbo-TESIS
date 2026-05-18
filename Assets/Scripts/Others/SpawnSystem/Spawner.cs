using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Spawner : MonoBehaviour
{
    [Header("Variables")]

    [SerializeField] GameObject[] EnemySpawns; // listado de spawns
    [SerializeField] int Index = 0;
    [SerializeField] bool RandomizedTime,BossSpawnSystem;
    [SerializeField] float RandomTimeSeed;
    private IEnumerator<GameObject> EnemyGenerator;

    public bool IsSpent,HasMinion;

    [Space]
    [Header("MinionVariables")]
    [SerializeField] Health_Component EnemySpawnedHP;

    [Space]
    [Header("References")]
    [SerializeField] RoomSpawnerManager _SpawnManager;

    [Header("boss ui references")]
    public BossHealthBarUI bossUIManagerReference; //panel

    [Header("Boss References")]
    public GameObject playerTarget; // player aca ahre  

    [Header("VFX Spawn")]
    [SerializeField] private GameObject spawnVFX;
    [SerializeField] private float spawnDelay = 2f;

    private void Start()
    {
        if(_SpawnManager == null)
        {
            _SpawnManager= GetComponentInParent<RoomSpawnerManager>();
        }
        EnemyGenerator = GetNextEnemy().GetEnumerator();
    }

    public int RemainingSpawns
    {
        get
        {
            if (EnemySpawns == null) return 0;
            return Mathf.Max(0, EnemySpawns.Length - Index);
        }
    }

    //spawn de enemigo
    // lo hago gameobject para guardar y mandarlo al manager
    public GameObject SpawnEnemy()
    {
        RandomizedTime = false;
        if (EnemySpawns == null) 
        {
            Debug.LogError("<Color = blue> Visceral Error: Spawner has no assigned Enemies </color>");
            return null;
        }

        if (HasMinion || IsSpent)
        {
            return null;
        }

        if(EnemyGenerator.MoveNext())
        {
            Index++;
            var NextEnemy = EnemyGenerator.Current;

            var InstantiatedEnemy = Instantiate(NextEnemy,transform.position,transform.rotation);
            //var DumbEnemySC = InstantiatedEnemy.GetComponent<DumbEnemy>();
            InstantiatedEnemy.transform.SetParent(this.transform,true);

            if (BossSpawnSystem)
            {
                // agarro scripts
                BossAbility bossAbilityScript = InstantiatedEnemy.GetComponent<BossAbility>();

                if (bossAbilityScript == null)
                {
                    bossAbilityScript = InstantiatedEnemy.GetComponentInChildren<BossAbility>();
                }

                // si lo encuentro asigno el player
                if (bossAbilityScript != null && playerTarget != null)
                {
                    bossAbilityScript.playerTarget = playerTarget.transform;
                    Debug.Log("aca ta el jefe y le asigno el player");
                }

                // conecto las cosas del boss
                Boss_HealthComp bossHealthComp = InstantiatedEnemy.GetComponent<Boss_HealthComp>();
                if (bossHealthComp == null) bossHealthComp = InstantiatedEnemy.GetComponentInChildren<Boss_HealthComp>();

                if (bossHealthComp != null && bossUIManagerReference != null)
                {
                    // boss a la ui
                    bossHealthComp.ConnectBossUI(bossUIManagerReference);
                }
            }
          
            EnemySpawnedHP = InstantiatedEnemy.GetComponent<Health_Component>();
            if (EnemySpawnedHP == null) EnemySpawnedHP = InstantiatedEnemy.GetComponentInChildren<Health_Component>();
            EnemySpawnedHP.OnDeath += MyMinionDied;
            HasMinion = true;

            return InstantiatedEnemy;
        }
        else
        {
            IsSpent = true;
            _SpawnManager.NotifyMinionDeath();
            print("isSpent");
        }

        return null; // si no hay mas enemigos
    }

    /// <summary>
    /// obtener el siguiente enemigo lazy
    /// </summary>
    /// <returns></returns>
    IEnumerable<GameObject> GetNextEnemy()
    {
        for(int I = 0; I < EnemySpawns.Length; I++)
        {
            GameObject enemy = EnemySpawns[I];
            yield return enemy;
        }
    }

    private void MyMinionDied()
    {
        EnemySpawnedHP.OnDeath -= MyMinionDied;

        EnemySpawnedHP = null;
        HasMinion = false;
        _SpawnManager.NotifyMinionDeath();
    }

    public IEnumerator SpawnEnemyWithVFX(System.Action<GameObject> onSpawned)
    {
        GameObject vfx = null;

        if (spawnVFX != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.1f;
            vfx = Instantiate(spawnVFX, spawnPos, Quaternion.identity);

            vfx.SetActive(true);

            foreach (var ps in vfx.GetComponentsInChildren<ParticleSystem>())
                ps.Play();

            foreach (var fx in vfx.GetComponentsInChildren<UnityEngine.VFX.VisualEffect>())
                fx.Play();
        }

        // esperamos antes de spawnear
        yield return new WaitForSeconds(spawnDelay);

        GameObject enemy = SpawnEnemy();

        if (enemy != null)
        {
            var dissolve = enemy.GetComponent<DissolveController>();
            if (dissolve == null)
                dissolve = enemy.GetComponentInChildren<DissolveController>();

            if (dissolve != null)
            {
                dissolve.StartAppear();
                yield return new WaitForSeconds(0.5f);
            }

        }

        // destruir VFX justo al final
        if (vfx != null)
        {
            Destroy(vfx);
        }

        onSpawned?.Invoke(enemy);
    }

}

//script hecho por patricio malvasio maddalena
//uso de Lazy Generator - 
