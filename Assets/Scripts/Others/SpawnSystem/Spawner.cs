using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Spawner : MonoBehaviour
{
    [Header("Variables")]

    [SerializeField] GameObject[] EnemySpawns; // listado de spawns
    [SerializeField] int Index = 0;
    private IEnumerator<GameObject> EnemyGenerator;

    public bool IsSpent,HasMinion;

    [Space]
    [Header("MinionVariables")]
    [SerializeField] Health_Component EnemySpawnedHP;

    [Space]
    [Header("References")]
    [SerializeField] RoomSpawnerManager _SpawnManager;
    public PlayerContext Player;

    private void Start()
    {
        if(_SpawnManager == null)
        {
            _SpawnManager= GetComponentInParent<RoomSpawnerManager>();
        }
        EnemyGenerator = GetNextEnemy().GetEnumerator();
    }

    /// <summary>
    /// set de contexto del jugador
    /// </summary>
    /// <param name="NewPlayer"> nuevo contexto</param>
    public void SetPlayerIndex(PlayerContext NewPlayer)
    {
        Player = NewPlayer;
    }

    //spawn de enemigo
    public void SpawnEnemy()
    {
        if(EnemySpawns == null) 
        {
            Debug.LogError("<Color = blue> Visceral Error: Spawner has no assigned Enemies </color>");
            return;
        }
        if (Player == null)
        {
            Player = _SpawnManager.playerContext;
            if(Player == null)
            {
                Debug.LogError("<Color = blue> Visceral Error: Spawner has no assigned value for PlayerContext");
            }
        }
        if (HasMinion || IsSpent)
        {
            return;
        }

        if(EnemyGenerator.MoveNext())
        {
            Index++;
            var NextEnemy = EnemyGenerator.Current;

            var InstantiatedEnemy = Instantiate(NextEnemy);
            var DumbEnemySC = InstantiatedEnemy.GetComponent<DumbEnemy>();

            DumbEnemySC.SetPlayerReference(Player);
            DumbEnemySC.SetTransformAndRotation(this.transform, this.transform.rotation);

            EnemySpawnedHP = InstantiatedEnemy.GetComponent<Health_Component>();
            if (EnemySpawnedHP == null) EnemySpawnedHP = InstantiatedEnemy.GetComponentInChildren<Health_Component>();
            EnemySpawnedHP.OnDeath += MyMinionDied;
            HasMinion = true;
        }
        else
        {
            IsSpent = true;
            _SpawnManager.NotifyMinionDeath();
            print("isSpent");
        }
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

}

//script hecho por patricio malvasio maddalena
//uso de Lazy Generator - 
