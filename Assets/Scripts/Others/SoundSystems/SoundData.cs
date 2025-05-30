using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Audio;


[Serializable]
public class SoundData
{
    /// <summary>
    /// el Clip que queremos escuchar
    /// </summary>
    public AudioClip Clip;

    /// <summary>
    /// El Mixer de sonido
    /// </summary>
    public AudioMixerGroup mixerGroup;

    /// <summary>
    /// si queremos hacer que el audioclip sea loopeado
    /// </summary>
    public bool Loop;

    /// <summary>
    /// si queremos que el clip suene en awake
    /// </summary>
    public bool PlayOnAwake;

    /// <summary>
    /// si queremos que el clip sea tridimensional (osea, que se vea afectado por distancia)
    /// </summary>
    /// 
    [Range(0,1f)]
    public int SpatialBlend= 1;

    /// <summary>
    /// Este booleano es para que este sonido este sujeto al limite maximo de sonidos similares
    /// regla general, si esperamos que el jugador pueda escuchar muchas veces este sonido en poco tiempo
    /// o es un sonido no importante que no afecta que no se escuche
    /// </summary>
    public bool IsFrequent;


}
