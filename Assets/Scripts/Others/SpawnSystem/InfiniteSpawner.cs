using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteSpawner : MonoBehaviour
{
    [SerializeField] GameObject _EnemyPrefab;
    [SerializeField] Health_Component _EnemyComponent;
    [SerializeField] LocalEventBusComponent EventBus;
    [SerializeField] float _TimeToSpawn;
    [SerializeField] bool _CanSpawn;

    [Tooltip("Si este spawner puede generar enemigos constantemente o solo puede tener 1 enemigo a la vez")]
    [SerializeField] bool _CanKeepMultipleInstances;

    [Tooltip("ID de Eventos a suscribirse")]
    [SerializeField] string _StartEventName,_EndEventName;

    private void Start()
    {
         if(EventBus != null)
         {
            EventBus.SubscribeToEvent(_StartEventName, StartSpawning);
            EventBus.SubscribeToEvent(_EndEventName, StopSpawning);
         }
    }


    float Pulse = 0;
    private void Update()
    {
        if (!_CanSpawn) return;

        if (_CanKeepMultipleInstances)
        {
            MultipleSpawnerLogic();
            return;
        }
        else
        {
            SingleSpawnerLogic();
            return;
        }

    }

    private void MultipleSpawnerLogic()
    {
        if(Pulse < _TimeToSpawn)
        {
            Pulse += Time.deltaTime * TimeDilationManager.GlobalTimeScale;
            return;
        }
        else
        {
            Pulse = 0;
            GameObject Enemy = Instantiate(_EnemyPrefab, this.transform.position, this.transform.rotation);

        }
    }

    private void SingleSpawnerLogic()
    {
        if(_EnemyComponent == null ||_EnemyComponent.isActiveAndEnabled == false || _EnemyComponent.CurrentHealth <= 0)
        {
            if (Pulse < _TimeToSpawn)
            {
                Pulse += Time.deltaTime * TimeDilationManager.GlobalTimeScale;
                return;
            }
            else
            {
                Pulse = 0;
                GameObject Enemy = Instantiate(_EnemyPrefab, this.transform.position, this.transform.rotation);
                _EnemyComponent = Enemy.GetComponentInChildren<Health_Component>();
            }
        }
      
    }

    private void StartSpawning() 
    {
        _CanSpawn = true;
        this.gameObject.SetActive(true);
    }

    private void StopSpawning() 
    {
        _CanSpawn = false;
        this.gameObject.SetActive(false);

    }
}
