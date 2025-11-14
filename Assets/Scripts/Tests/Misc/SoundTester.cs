using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTester : MonoBehaviour
{

    [SerializeField] SoundData SoundTest;
    [SerializeField] float timeBetweenSounds;

    [SerializeField] bool withTransform, WithRandomPitch;

    private void Start()
    {
        StartCoroutine(SoundOff());
    }

    IEnumerator SoundOff()
    {
        yield return new WaitForSeconds(timeBetweenSounds);
        SoundManager.Instance.CreateSound()
       .WithSoundData(SoundTest)
       .WithPosition(this.transform.position)
       .WithRandomPitch(WithRandomPitch)
       .WithSpatialBlend(SoundTest.SpatialBlend,SoundTest.MinimunSoundDistance,SoundTest.MaximunSoundDistance)
       .play();
    }
 
}
