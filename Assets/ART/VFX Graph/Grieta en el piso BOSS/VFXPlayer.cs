using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXPlayer : MonoBehaviour
{
    [SerializeField] ParticleSystem[] _Particles;

    private void Start()
    {
        if (_Particles == null) _Particles = GetComponentsInChildren<ParticleSystem>();
    }

    public void PlayAllParticles()
    {
        foreach (ParticleSystem particle in _Particles)
        {
            particle.Play();
        }
    }

    public void StopAllParticles()
    {

        foreach (ParticleSystem particle in _Particles)
        {
            particle.Stop();
        }
    } 

    public void PlaySingleParticle(int ParticleID)
    {
        if(ParticleID > _Particles.Length - 1)
        {
            Debug.LogError("Visceral Error: VFX Player recieved a ParticleID higher than Index");
            return;
        }
        else if(ParticleID < 0)
        {
            Debug.LogError("Visceral Error: VFX Player recieved a ParticleID lower than 0");
            return;
        }
        else
        {
            _Particles[ParticleID].Play();
        }
    }


}
