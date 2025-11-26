using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEmittingComponent : MonoBehaviour
{

    [SerializeField] SoundData Sound;
    [SerializeField] float _TimeBetweenSounds;
    [SerializeField] bool _PlayOnAwake;
    bool ShouldLoop;

    private void Start()
    {

    }


}
