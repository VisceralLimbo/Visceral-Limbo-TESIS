using UnityEngine;
using System;

public class Boss_HealthComp : Health_Component
{
    // se asigna en el spawner
    [HideInInspector] public BossHealthBarUI bossUIHandler;
    // nombre del jefe
    public string BossName = "CORPUS";

    [SerializeField] private LocalEventBusComponent _eventEmitter;

    [SerializeField] private string _StartEnemyEvent, _EndEnemyEvent;

    [SerializeField] LocalEventBusComponent _EventBus;

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

        if(_EventBus == null)
        {
            _EventBus = GetComponentInParent<LocalEventBusComponent>();
        }
    }

    protected override void InternalDamage(float damage, Vector3? KnockbarDir, float force, DamageScore Score = null)
    {
        if(CurrentHealth - damage < MaxHealth / 0.5f)
        {
            // iniciar spawners
            if(_EventBus != null)
            {
                _EventBus.TriggerEvent(_StartEnemyEvent);

            }
        }
        else if (CurrentHealth - damage <= 0)
        {
            // frenar spawners
            if(_EventBus != null)
            {
                _EventBus.TriggerEvent(_EndEnemyEvent);

            }
        }


        base.InternalDamage(damage, KnockbarDir, force, Score);

    }

}