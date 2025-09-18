using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthDrop : MonoBehaviour
{
    //script del dropd de vida

    [SerializeField] private float healAmount = 25f; //para q lo cambien en el inspector o lo setean a una q quieran
    [SerializeField] private float lifetime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifetime); // desaparece dsp del tiempo si no lo agarran
    }

    private void OnTriggerEnter(Collider other)
    {
        //chequeo q la faction sea player para no curar enemigos y solo al player. 
        var health = other.GetComponent<Health_Component>();
        if (health != null && health.Context?.faction == FactionID.Player)
        {
            //lo curo
            health.HealHP(healAmount);

            if (HealthFullscreenEffect.Instance != null)
            {
                HealthFullscreenEffect.Instance.PlayEffect(7f); 
            }
            Destroy(gameObject);
        }
    }
}
