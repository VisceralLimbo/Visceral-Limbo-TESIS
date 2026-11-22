using UnityEngine;

public class RainDropSound : MonoBehaviour
{
    public AudioSource audioSource;
    public float soundCooldown = 0.5f;

    private float nextSoundTime = 0f;

    void OnParticleCollision(GameObject other)
    {
        if (Time.time < nextSoundTime)
            return;

        nextSoundTime = Time.time + soundCooldown;

        audioSource.PlayOneShot(audioSource.clip);
    }
}
