using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonEntryPoint : MonoBehaviour
{
    [SerializeField] private bool IsOcupied = false;
    [SerializeField] private DungeonPart Owner;
    [SerializeField] private DungeonPart ConectedTo;

    public bool SetOccupied(DungeonPart ConnectedTo,bool value = true)
    {
        if(ConnectedTo == null)
        {
            IsOcupied = false;
            return false;
        }

        if(ConnectedTo == Owner)
        {
            Debug.LogWarning("Proc.gen Error: se trato de conectar un entry point con su dueño" + this.name);
            IsOcupied = false;
            return false;
        }
        else
        {
            ConectedTo = ConnectedTo;
            IsOcupied = true;
            return true;
        }
    }

    public bool IsOccupied() => IsOcupied;

    public void SetOwner(DungeonPart dungeonPart) => Owner = dungeonPart;

    public DungeonPart GetOwner() => Owner;
}
