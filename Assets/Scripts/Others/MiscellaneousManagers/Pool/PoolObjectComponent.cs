using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using System;

public class PoolObjectComponent : MonoBehaviour
{
    /// <summary>
    /// La Funcion / Evento de liberacion del objeto al pool
    /// </summary>
    private Action _releaseAction;
    private bool _isReturning = false; // evitamos recursividad de return

    // Inyectamos la funcion de liberacion
    public void Initialize(Action releaseAction)
    {
        _releaseAction = releaseAction;
    }

    public void ReleaseToPool()
    {
        // evitamos double return a pool
        if (_isReturning) return;
        _isReturning = true;

        if (_releaseAction != null)
        {
            _releaseAction.Invoke();
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

    private void OnEnable()
    {
        _isReturning = false;
    }


}
