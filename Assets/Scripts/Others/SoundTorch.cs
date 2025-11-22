using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTorch : MonoBehaviour
{
    [SerializeField] private SoundData _SoundTorch;


    private void Start()
    {
        SoundManager.Instance.CreateSound().WithSoundData(_SoundTorch).WithPosition(this.transform.position).WithSpatialBlend(1f, 1f, 50f).play();
    }
}
