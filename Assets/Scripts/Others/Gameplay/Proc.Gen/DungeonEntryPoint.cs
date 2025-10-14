using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonEntryPoint : MonoBehaviour
{
    [SerializeField] private bool IsOcupied = false;
    [SerializeField] private DungeonPart Owner;

    public bool SetOccupied(bool value = true) => IsOcupied = value;

    public bool IsOccupied() => IsOcupied;

    public void SetOwner(DungeonPart dungeonPart) => Owner = dungeonPart;

    public DungeonPart GetOwner() => Owner;
}
