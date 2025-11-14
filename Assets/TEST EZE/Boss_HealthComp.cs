using UnityEngine;
using System;

public class Boss_HealthComp : Health_Component
{
    // se asigna en el spawner
    [HideInInspector] public BossHealthBarUI bossUIHandler;
    // nombre del jefe
    public string BossName = "CORPUS";

    // llamo desde el sppawner dsp de q se instancia el boss para conectar vida con la ui
    public void ConnectBossUI(BossHealthBarUI uiHandler)
    {
        if (uiHandler == null) return;

        bossUIHandler = uiHandler;

        // se suscribe a los eventos
        OnDamaged += UpdateBossHealthUI;
        OnHealed += UpdateBossHealthUI;
        OnDeath += HideBossUI;

        // inicio y mustro barra de vida
        bossUIHandler.InitializeBossBar(this, BossName);
    }

    // notif cada q recibe daño
    private void UpdateBossHealthUI()
    {
        if (bossUIHandler != null)
        {
            bossUIHandler.UpdateHealthUI();
        }
    }

    // se llama cuando muere el boss
    private void HideBossUI()
    {
        if (bossUIHandler != null)
        {
            // limpio y apago ui
            bossUIHandler.HideBossBar();

            // limpio las suscripciones de antes
            OnDamaged -= UpdateBossHealthUI;
            OnHealed -= UpdateBossHealthUI;
            OnDeath -= HideBossUI;
        }
    }

    private void Start()
    {
        if(bossUIHandler == null)
        {
            bossUIHandler = FindObjectOfType<Combat_UI_Manager>().GetComponentInChildren<BossHealthBarUI>(true);
        }


        if (bossUIHandler != null)
        {
            ConnectBossUI(bossUIHandler);
        }
    
    }
}