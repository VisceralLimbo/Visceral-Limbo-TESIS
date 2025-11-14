using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


[RequireComponent(typeof(AudioSource))]
public class SoundEmitter : MonoBehaviour
{

    [SerializeField] AudioSource _audioSource;
    Coroutine PlayingSoundCoroutine;

    SoundData _Data;

    private void Awake()
    {
        if(TryGetComponent(out AudioSource Source))
        {
            _audioSource= Source;
        }
        else
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    /// <summary>
    /// activar el sonido del SoundEmitter
    /// </summary>
    public void Play()
    {
        if(PlayingSoundCoroutine != null)
        {
            StopCoroutine(PlayingSoundCoroutine);
        }

        _audioSource.Play();

        PlayingSoundCoroutine = StartCoroutine(WaitForSoundToEnd());
    }

    IEnumerator WaitForSoundToEnd()
    {
        //esperar mientras que el audiosource este sonando
        yield return new WaitWhile(() => _audioSource.isPlaying);

        //retornar al pool
        SoundManager.Instance.ReturnToPool(this);

        if (_Data.IsFrequent)
        {
            SoundManager.Instance.FrequentSoundEmitters.Dequeue();
        }
    }

    /// <summary>
    /// frenar el SoundEmitter
    /// </summary>
    public void Stop()
    {
        if(PlayingSoundCoroutine != null)
        {
            StopCoroutine(PlayingSoundCoroutine);
            PlayingSoundCoroutine = null;
        }

        _audioSource.Stop();
        SoundManager.Instance.ReturnToPool(this);
    }



    /// <summary>
    /// Iniciar el Emisor de sonidos con un Sound Data particular
    /// </summary>
    /// <param name="Data"> El dato del sonido que queremos reproducir</param>
    public void Initialize(SoundData Data)
    {
        _Data = Data;
        _audioSource.clip = Data.Clip;
        _audioSource.outputAudioMixerGroup = Data.mixerGroup;
        _audioSource.loop = Data.Loop;
        _audioSource.playOnAwake = Data.PlayOnAwake;
        _audioSource.spatialBlend = Data.SpatialBlend;
        _audioSource.minDistance = Data.MinimunSoundDistance;
        _audioSource.maxDistance= Data.MaximunSoundDistance;

    }


    public void RandomizePitch(float min = -0.05f,float max = 0.05f)
    {
        _audioSource.pitch += Random.Range(min, max);
    }

    public void OnSpatialBlended(float blended,float minimum,float maximum)
    {
        _audioSource.spatialBlend = blended;
        _audioSource.minDistance = minimum;
        _audioSource.maxDistance= maximum;
    }
}
