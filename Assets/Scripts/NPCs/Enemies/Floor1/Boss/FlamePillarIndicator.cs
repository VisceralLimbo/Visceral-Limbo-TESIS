using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlamePillarIndicator : MonoBehaviour
{
    [Header("References")]

    [Tooltip("El pilar de fuego con daño y visuales finales")]
    [SerializeField] PillarDamage FlamePillarPrefab;

    [Tooltip("Particulas del indicador de fuego")]
    [SerializeField] ParticleSystem[] IndicatorParticles;
    [SerializeField] PlayerContext _Context;

    [Space]
    [Header("Variables")]
    [SerializeField] float _ScaleSpeed = 3f;
    [SerializeField] float _PillarActiveDuration = 3f;
    [SerializeField] float _HeightCorrection = 2f;

    float _Duration;
    float _StartTime;


    public void SetupIndicator(Vector3 pos, float Duration, PlayerContext Context)
    {
        _StartTime = Time.time;
        _Duration = Duration;
        _Context = Context;

        foreach(var indicator in IndicatorParticles)
        {
            if(indicator != null && !indicator.isPlaying) indicator.Play();
        }

        StartCoroutine(ExecutePillar());
        
    }

    private void Update()
    {
        float T = (Time.time - _StartTime) / _Duration;
        transform.localScale = Vector3.Slerp(Vector3.zero, Vector3.one, (T * _ScaleSpeed) * TimeDilationManager.GlobalTimeScale);

    }

    private IEnumerator ExecutePillar()
    {
        yield return new WaitForSeconds(_Duration);

        if(FlamePillarPrefab !=  null)
        {
            Vector3 SpawnPosition = transform.position + (Vector3.up * _HeightCorrection);
            PillarDamage Pillar = Instantiate(FlamePillarPrefab, SpawnPosition, Quaternion.identity);
            Pillar.Initialize(_PillarActiveDuration,_Context);

        }
        Destroy(gameObject);
    }
}

