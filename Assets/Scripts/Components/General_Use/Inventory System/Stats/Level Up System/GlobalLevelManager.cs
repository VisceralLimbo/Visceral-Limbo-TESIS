using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GlobalLevelManager
{
    private static int _GlobalLevelStage = 1;

    /// <summary>
    /// Event for the change of the level Stage
    /// </summary>
    public static Action<int> OnGlobalLevelStageChange;


    /// <summary>
    /// Sets the current Global Level Stage for growth purposes
    /// </summary>
    /// <param name="NewStage"> the new value</param>
    public static void SetGlobalLevelStage(int NewStage)
    {
        _GlobalLevelStage = NewStage;
        OnGlobalLevelStageChange?.Invoke(_GlobalLevelStage);
    }

    /// <summary>
    /// Adds levels to the current Global Level Stage
    /// </summary>
    /// <param name="Amount"></param>
    public static void AddGlobalLevelStage(int Amount)
    {
        _GlobalLevelStage += Amount;
        OnGlobalLevelStageChange?.Invoke(_GlobalLevelStage);
    }

    /// <summary>
    /// Resets the Global Level Stage to 1.
    /// </summary>
    public static void ResetGlobalLevelStage()
    {
        _GlobalLevelStage = 1;
        OnGlobalLevelStageChange?.Invoke(_GlobalLevelStage);
    }

    public static int GetGlobalLevelStage() { return _GlobalLevelStage; }




}
