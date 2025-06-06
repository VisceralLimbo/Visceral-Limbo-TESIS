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


    public static void Stop(float duration)
    {
        if (Waiting) return;

        Time.timeScale = 0f;

        Runner.StartCoroutine(WaitCR(duration));
    }

    static IEnumerator WaitCR(float duration)
    {
        Waiting = true;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
        Waiting = false;
    }
}
