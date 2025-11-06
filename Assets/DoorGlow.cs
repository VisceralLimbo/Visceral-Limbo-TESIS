using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DoorGlow : MonoBehaviour
{
    public List<ParticleSystem> particleSystems;

    // duracion para apagarse
    public float fadeDuration = 1.5f;
    private bool IsRunning = false;

    void Awake()
    {
        // relleno la lista con los hijos
        if (particleSystems == null || particleSystems.Count == 0)
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>(true).ToList();
        }

        if (particleSystems.Count == 0)
        {
            // debug por las dudas
            Debug.LogError("no esta el particlesystem xd");
            enabled = false;
        }

        if (particleSystems.Count > 0)
        {
            // inicio las particulas en las puertas especiales (solo esas tienen el script)
            foreach (var ps in particleSystems)
            {
                if (!ps.isPlaying)
                {
                    ps.Play();
                }
            }
        }

    }

    // fade out (no se representa bien no se porqeu)

    public void StartFadeOut()
    {
        // incio si no esta apgandose
        if (!IsFadingOut())
        {
            StopAllCoroutines(); // apago otras corrutinas
            StartCoroutine(FadeOutCoroutine());
        }
    }

    // chequeo si la corrutina esta corriendo
    public bool IsFadingOut()
    {
        return IsRunning;
    }

    private IEnumerator FadeOutCoroutine()
    {
        IsRunning = true;
        float elapsedTime = 0f;

        // guardo los valores dafult de color y emision
        Dictionary<ParticleSystem, Color> startColors = new Dictionary<ParticleSystem, Color>();
        Dictionary<ParticleSystem, float> startEmissions = new Dictionary<ParticleSystem, float>();

        // guardo iniciales
        foreach (var ps in particleSystems)
        {
            // .main para el color main
            startColors.Add(ps, ps.main.startColor.color);
            // .emsion para la emision xd
            startEmissions.Add(ps, ps.emission.rateOverTime.constant);
        }

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            // el float t iria de 0 a 1 lento
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);

            foreach (var ps in particleSystems)
            {
                Color startColor = startColors[ps];
                float startEmission = startEmissions[ps];

                // el alpha va de 1 a 0
                Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
                Color newColor = Color.Lerp(startColor, endColor, t);

                var mainModule = ps.main;
                mainModule.startColor = new ParticleSystem.MinMaxGradient(newColor);

                // del valor inicial del rate pasa a 0
                float newEmission = Mathf.Lerp(startEmission, 0f, t);

                var emissionModule = ps.emission;
                emissionModule.rateOverTime = newEmission;
            }

            yield return null;
        }

        // me aseguro de q se apague
        foreach (var ps in particleSystems)
        {
            // alpha 0
            Color finalColor = new Color(startColors[ps].r, startColors[ps].g, startColors[ps].b, 0f);
            var mainModule = ps.main;
            mainModule.startColor = new ParticleSystem.MinMaxGradient(finalColor);

            // emision 0
            var emissionModule = ps.emission;
            emissionModule.rateOverTime = 0f;

            // paro emision y limpio
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        IsRunning = false;
    }
}
