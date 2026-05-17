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

    // color de inicio
    [SerializeField] private Color _InitialColor = Color.white;

    // movi el diccionario q tenia en la corutina del fade
    private Dictionary<ParticleSystem, Color> _startColors = new Dictionary<ParticleSystem, Color>();
    private Dictionary<ParticleSystem, float> _startEmissions = new Dictionary<ParticleSystem, float>();

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
            return;
        }

        // aplico aca si hay color
        if (_InitialColor != Color.white)
        {
            ApplyInitialColor(_InitialColor);
        }

        // guardo dsp de aplicar el color
        CacheInitialValues();

        // play a las prticulas
        foreach (var ps in particleSystems)
        {
            if (!ps.isPlaying)
            {
                ps.Play();
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

        // si no se guardaroin los guardo ahora para evitar errores mas q nada (me pasaba q no se prendian idk)
        if (_startColors.Count == 0)
        {
            CacheInitialValues();
        }

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);

            foreach (var ps in particleSystems)
            {
                Color startColor = _startColors[ps];
                float startEmission = _startEmissions[ps];

                // chau alpha
                Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
                Color newColor = Color.Lerp(startColor, endColor, t);

                var mainModule = ps.main;
                mainModule.startColor = new ParticleSystem.MinMaxGradient(newColor);

                // bajo la emision
                float newEmission = Mathf.Lerp(startEmission, 0f, t);

                var emissionModule = ps.emission;
                emissionModule.rateOverTime = newEmission;
            }

            yield return null;
        }

        // fuerzo por las dudas q se apgue
        foreach (var ps in particleSystems)
        {
            // alpha y emision me aseguro q queden 0
            Color finalColor = new Color(_startColors[ps].r, _startColors[ps].g, _startColors[ps].b, 0f);
            var mainModule = ps.main;
            mainModule.startColor = new ParticleSystem.MinMaxGradient(finalColor);

            var emissionModule = ps.emission;
            emissionModule.rateOverTime = 0f;
            ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }
        IsRunning = false;
    }

    // para el dungeon
    public void SetGlowColor(Color color)
    {
        _InitialColor = color;
        if (_startColors.Count > 0)
        {
            ApplyInitialColor(color);
        }
    }

    // aplico a las partiuclas
    private void ApplyInitialColor(Color color)
    {
        foreach (var ps in particleSystems)
        {
            var mainModule = ps.main;
            Color fullAlphaColor = new Color(color.r, color.g, color.b, 1f);
            mainModule.startColor = new ParticleSystem.MinMaxGradient(fullAlphaColor);
        }
    }

    // guardo las cosas para el fade
    private void CacheInitialValues()
    {
        _startColors.Clear();
        _startEmissions.Clear();
        foreach (var ps in particleSystems)
        {
            _startColors.Add(ps, ps.main.startColor.color);
            _startEmissions.Add(ps, ps.emission.rateOverTime.constant);
        }
    }
}
