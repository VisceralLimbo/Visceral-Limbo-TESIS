using UnityEngine;
using System;
using System.Data;

public class Boss_HealthComp : Health_Component
{
    // se asigna en el spawner
    [HideInInspector] public BossHealthBarUI bossUIHandler;
    // nombre del jefe
    public string BossName = "CORPUS";

    [SerializeField] private LocalEventBusComponent _eventEmitter;

    [SerializeField] private string _StartEnemyEvent, _EndEnemyEvent;

    [SerializeField] LocalEventBusComponent _EventBus;

    [SerializeField] string _HealthStat, _DefenseStat, _DamageReductionStat;

    private Combat_UI_Manager Ref_CombatUI;

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

            if (Combat_UI_Manager._Instance != null)
            {
                Combat_UI_Manager._Instance.DisplayWin(true);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
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

        if (Ref_CombatUI == null)
        {
            Ref_CombatUI = Combat_UI_Manager._Instance;
        }

        StatsManager _StatMan = _Context.Stats;
           

        if (_StatMan == null)
        {
            _StatMan = _Context?.GetComponent<StatsManager>();
            if (_StatMan == null)
            {
                _StatMan = _Context?.GetComponentInChildren<StatsManager>();
            }
        }


        if (_StatMan != null)
        {
            _StatMan.OnStatChanged += UpdateStats;

            MaxHealth = Context.Stats.GetFloatStatValue(_HealthStat);
            Defense = _StatMan.GetFloatStatValue(_DefenseStat);
            DamageReduction = _StatMan.GetFloatStatValue(_DamageReductionStat);
        }


    }

    protected override void InternalDamage(float damage, Vector3? KnockbarDir, float force, DamageScore Score = null)
    {

        if (CurrentHealth - damage <= 0)
        {
            // frenar spawners
            if (_EventBus != null)
            {
                Debug.LogWarning("Mori,desactivando EnemySpawners");
                _EventBus.TriggerEvent(_EndEnemyEvent);
                _EventBus.TriggerEvent("KillEnemies");

            }
        }

        else if (CurrentHealth - damage < MaxHealth * 0.5f)
        {
            // iniciar spawners
            if(_EventBus != null)
            {
                _EventBus.TriggerEvent(_StartEnemyEvent);

            }
        }
        

        base.InternalDamage(damage, KnockbarDir, force, Score);

    }


    private void UpdateStats(string StatID, float Value)
    {

        MaxHealth = Context.Stats.GetFloatStatValue(_HealthStat);
        Defense = Context.Stats.GetFloatStatValue(_HealthStat);
        DamageReduction = Context.Stats.GetFloatStatValue(_HealthStat);
    }
}