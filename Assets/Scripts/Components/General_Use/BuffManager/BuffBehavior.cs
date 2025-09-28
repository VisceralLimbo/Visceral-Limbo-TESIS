using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BuffBehavior : MonoBehaviour
{
    [SerializeField] protected BuffManager _buffManager;
    [SerializeField] protected StatsManager _StatMan;

    [SerializeField] protected int _BuffPotency = 0;
    [SerializeField] protected BuffSO _BuffSO;

    /// <summary>
    /// Update method del buff
    /// </summary>
    /// <param name="deltaTime"></param>
    public abstract void OnUpdate(float deltaTime);

    /// <summary>
    /// funcion llamada cuando el buffo es aplicado. se llama 1 vez por aplicacion
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="StatMan"></param>
    public abstract void OnApply(BuffManager manager,StatsManager StatMan);

    /// <summary>
    /// funcion llamada cuando se añade 1 stack de potencia al buff.
    /// usar para logica de empowerment
    /// </summary>
    /// <param name="ExtraPotency"></param>
    public abstract void OnAddPotency(float ExtraPotency);

    /// <summary>
    /// funcion llamada cuando el buffo se expira
    /// </summary>
    public abstract void OnExpire();

    public int GetPotency()
    {
        return _BuffPotency;
    }

    public void SetSO(BuffSO SO) { _BuffSO = SO; }
    public BuffSO GetSO() { return _BuffSO; }
}
