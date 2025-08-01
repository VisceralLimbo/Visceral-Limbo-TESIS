using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public static class HitStop
{ 

    private class HitStopRunner: MonoBehaviour { }

    private static HitStopRunner _Runner;

    private static bool Waiting;


    private static HitStopRunner Runner
    {
        get
        {
            if(_Runner == null)
            {
                GameObject runnerobj = new GameObject();
                _Runner = runnerobj.AddComponent<HitStopRunner>();
                GameObject.DontDestroyOnLoad(runnerobj);
            }
            return _Runner;
        }

    }


    public static void Stop(float duration, float slowdownFactor = 0.2f)
    {
        if (Waiting) return;

        slowdownFactor = Mathf.Clamp(slowdownFactor, 0.01f, 1f);

        float originalFixedDeltaTime = Time.fixedDeltaTime;

        Time.timeScale = slowdownFactor;
        Time.fixedDeltaTime = originalFixedDeltaTime * slowdownFactor;

        // Obtener todos los AudioSource activos
        AudioSource[] audioSources = GameObject.FindObjectsOfType<AudioSource>();
        float[] originalPitches = new float[audioSources.Length];

        // Reducir pitch de todos los sonidos
        for (int i = 0; i < audioSources.Length; i++)
        {
            originalPitches[i] = audioSources[i].pitch;
            audioSources[i].pitch = originalPitches[i] * slowdownFactor;
        }

        Runner.StartCoroutine(WaitCR(duration, originalFixedDeltaTime, audioSources, originalPitches));
    }

    static IEnumerator WaitCR(float duration, float originalFixedDeltaTime, AudioSource[] sources, float[] originalPitches)
    {
        Waiting = true;
        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        // Restaurar pitch original
        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i] != null)
                sources[i].pitch = originalPitches[i];
        }

        Waiting = false;
    }
}
