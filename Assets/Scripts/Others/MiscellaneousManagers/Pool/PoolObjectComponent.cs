using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolObjectComponent : MonoBehaviour
{

    [Header("Setup")]
    [SerializeField] IObjectPool<PoolObjectComponent> _MyPool;

    public void Initialize(IObjectPool<PoolObjectComponent> NewPool)
    {
        _MyPool = NewPool;
    }

    public void ReleaseToPool()
    {
        if(_MyPool != null )
        {
            _MyPool.Release(this);
        }
        else
        {
            Destroy(this.gameObject);
        }

    }

    private void OnDisable()
    {
        ReleaseToPool();
    }

}
