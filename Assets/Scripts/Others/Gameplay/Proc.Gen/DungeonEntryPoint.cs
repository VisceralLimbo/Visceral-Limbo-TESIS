using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonEntryPoint : MonoBehaviour
{
    [SerializeField] private bool IsOcupied = false;

    public bool SetOccupied(bool value = true) => IsOcupied = value;

    public bool IsOccupied() => IsOcupied;


}
