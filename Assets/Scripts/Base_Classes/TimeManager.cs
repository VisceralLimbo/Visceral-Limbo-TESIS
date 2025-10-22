using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class TimeDilationManager
{
    /// <summary>
    /// el valor de escala global de tiempo
    /// </summary>
    public static float GlobalTimeScale { get; private set; } = 1f;

    /// <summary>
    /// Evento para actualizar la escala global
    /// </summary>
    public static Action<float> OnTimeScaleChanged;

    private static float StartingTimeScale = 1;


    /// <summary>
    /// Setear la nueva escala global de tiempo
    /// </summary>
    /// <param name="timeScale"></param>
    public static void SetTimeScale(float timeScale)
    {
        GlobalTimeScale = timeScale;
        OnTimeScaleChanged?.Invoke(GlobalTimeScale);

        Debug.Log("New timescale: " + GlobalTimeScale);
    }

    public static void ResetTimeScale()
    {
        GlobalTimeScale= 1f;
        OnTimeScaleChanged?.Invoke(GlobalTimeScale);
    }



}