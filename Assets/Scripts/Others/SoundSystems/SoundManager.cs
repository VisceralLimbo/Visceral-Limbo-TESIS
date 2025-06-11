using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class SoundManager : MonoBehaviour
{

    public static SoundManager Instance;
    /// <summary>
    /// pool de emisores de sonido
    /// </summary>
    IObjectPool<SoundEmitter> SoundPool;
    readonly List<SoundEmitter> _ActiveSounds = new List<SoundEmitter>();

    public readonly Queue<SoundEmitter> FrequentSoundEmitters = new();

    [SerializeField] SoundEmitter _SoundEmitterPrefab;
    [SerializeField] bool CollectionCheck = true;

    [Space]
    [Header("Pool Settings")]
    [SerializeField] int DefaultPoolCapacity; // capacidad default del pool
    [SerializeField] int MaxPoolCapacity; // capacidad maxima de objetos en el pool
    [SerializeField] int MaxSoundInstances; // capacidad maxima del MISMO sonido
                                            // que puede ser ejecutado


    private void Awake()
    {
        if(Instance == null && Instance != this)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this.gameObject );
        }
    }



    private void Start()
    {
        InitializePool();
    }

    public SoundBuilder CreateSound() => new SoundBuilder(this);


    /// <summary>
    /// chequeo para ver si es posible correr un sonido 
    /// o si ya hay muchos sonidos similares en escena
    /// </summary>
    /// <param name="data">la data del sonido a revisar</param>
    /// <returns></returns>
    public bool CanPlaySound(SoundData data)
    {
        if (data == null) return false;
        if (!data.IsFrequent) return true;

        if (FrequentSoundEmitters.Count >= MaxSoundInstances 
           && FrequentSoundEmitters.TryDequeue(out var emitter))
        {
                try
                {
                    emitter.Stop();
                    return true;
                }
                catch
                {
                    Debug.Log("<Color:lightblue> Visceral Warning: Sound Emitter ya esta libre</Color>");
                }
                return false;
        }

        return true;
    }



    /// <summary>
    /// Obtener un Emisor de sonidos del Pool de emisores
    /// </summary>
    /// <returns></returns>
    public SoundEmitter Get() { return SoundPool.Get(); }

    /// <summary>
    /// Devolver un emisor al pool de emisores
    /// </summary>
    /// <param name="sound"> la referencia del emisor para devolver al pool</param>
    public void ReturnToPool(SoundEmitter sound) { SoundPool.Release(sound); }


    void InitializePool()
    {
                SoundPool = new ObjectPool<SoundEmitter>(
                CreateSoundEmitter,
                OnTakeFromPool,
                OnReturnedPool,
                OnDestroyPoolObject,
                CollectionCheck,
                DefaultPoolCapacity,
                MaxPoolCapacity
                );

        Prewarm((ObjectPool<SoundEmitter>)SoundPool, DefaultPoolCapacity);
    }

    void Prewarm(ObjectPool<SoundEmitter> Pool, int count)
    {
        var tempList = new List<SoundEmitter>(count);

        for(int i = 0; i < count; i++)
        {
            var obj = Pool.Get();
            tempList.Add(obj);
            obj.gameObject.transform.parent = this.transform;
        }

        foreach(var obj in tempList)
        {
            Pool.Release(obj);
        }
    }


    //funcion de destruccion del pool entero
    void OnDestroyPoolObject(SoundEmitter soundEmitter)
    {
        Destroy(soundEmitter.gameObject);
    }

    //funcion de devolver un objeto al pool
    void OnReturnedPool(SoundEmitter soundEmitter)
    {
        soundEmitter.gameObject.SetActive(false);
        _ActiveSounds.Remove(soundEmitter);
    }

    //funcion de extraer un objeto del pool
    void OnTakeFromPool(SoundEmitter soundEmitter)
    {
        soundEmitter.gameObject.SetActive(true);
        _ActiveSounds.Add(soundEmitter);
    }


    //funcion de creacion de un emisor de sonidos
    SoundEmitter CreateSoundEmitter()
    {
        var SoundPrefab = Instantiate(_SoundEmitterPrefab);
        SoundPrefab.gameObject.SetActive( false );
        return SoundPrefab;
    }

}
