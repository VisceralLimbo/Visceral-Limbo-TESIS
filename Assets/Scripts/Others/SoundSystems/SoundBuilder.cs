using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundBuilder
{
    readonly SoundManager soundManager;

    SoundData data;

    Vector3 position = Vector3.zero;

    bool randomPitch;

    float SpatialBlend;


    public SoundBuilder(SoundManager soundManager)
    {
        this.soundManager = soundManager;
    }

    public SoundBuilder WithSoundData(SoundData Sounddata)
    {
        data = Sounddata;
        return this;
    }

    public  SoundBuilder WithPosition(Vector3 pos)
    {
        this.position = pos; return this;
    }

    public SoundBuilder WithRandomPitch(bool randomPitch)
    {
        this.randomPitch = randomPitch;
        return this;
    }

    public SoundBuilder WithSpatialBlend(float blend)
    {
        this.SpatialBlend = blend;
        return this;
    }

    public void play()
    {
        if (!soundManager.CanPlaySound(data)) return;

        SoundEmitter Emitter = soundManager.Get();

        Emitter.Initialize(data);
        Emitter.transform.position = position;
        Emitter.transform.SetParent(SoundManager.Instance.transform,true);
        Emitter.OnSpatialBlended(SpatialBlend);

        if (randomPitch)
        {
            Emitter.RandomizePitch();
        }

        if (data.IsFrequent)
        {
            soundManager.FrequentSoundEmitters.Enqueue(Emitter);
        }

        Emitter.Play();

    }

    public void play(out SoundEmitter emitter)
    {
        emitter = null;
        if (!soundManager.CanPlaySound(data)) return;

        SoundEmitter Emitter = soundManager.Get();

        Emitter.Initialize(data);
        Emitter.transform.position = position;
        Emitter.transform.SetParent(SoundManager.Instance.transform, true);
        Emitter.OnSpatialBlended(SpatialBlend);

        if (randomPitch)
        {
            Emitter.RandomizePitch();
        }

        if (data.IsFrequent)
        {
            soundManager.FrequentSoundEmitters.Enqueue(Emitter);
        }

        Emitter.Play();

        emitter = Emitter;

    }

}
