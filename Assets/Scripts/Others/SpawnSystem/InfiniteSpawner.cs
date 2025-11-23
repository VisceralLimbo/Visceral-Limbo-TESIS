using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteSpawner : MonoBehaviour
{
    [SerializeField] GameObject _EnemyPrefab;
    [SerializeField] float _TimeToSpawn;
    [SerializeField] bool _CanSpawn;
    [SerializeField] Trigger_Component Trigger;
    [SerializeField] string _TriggerEventName;

    private void Start()
    {
         
    }

    private void Update()
    {
        
    }

    private void StartSpawning() { }

    private void StopSpawning() { }
}
