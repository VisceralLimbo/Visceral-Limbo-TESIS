using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player_HealthComp))]
public class PlayerBloodScreenFeedback : MonoBehaviour
{
    private Player_HealthComp playerHealth;

    void Start()
    {
        playerHealth = GetComponent<Player_HealthComp>();

        // Suscribirse al evento de daño
        if (playerHealth != null)
            playerHealth.OnDamaged += ShowBloodSplash;
    }

    private void ShowBloodSplash()
    {
        if (BloodScreenManager.Instance != null)
            BloodScreenManager.Instance.ShowRandomBloodSplash();
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnDamaged -= ShowBloodSplash;
    }
}
