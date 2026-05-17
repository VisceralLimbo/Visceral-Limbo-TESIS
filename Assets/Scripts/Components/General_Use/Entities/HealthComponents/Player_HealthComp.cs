using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player_HealthComp : Health_Component
{
    [SerializeField] SoundData _LowHP;

    [Header("Stats")]
    [SerializeField] StatIdentifier _MaxHealthID,_BaseDefenseID,_BaseDamageReduction,_BaseDamageInvulnerability;

    [SerializeField] private GameObject _damageIndicatorPrefab;

    private Vector3 lastHitPosition;

    private void Start()
    {
       
        // setteamos que la salud máxima es la del stat system.
        MaxHealth = _Context.Stats.GetFloatStatValue(_MaxHealthID);

        //suscribimos a stats cuando se cambian
        _Context.Stats.OnStatChanged += UpdateStatValues;

        CurrentHealth = MaxHealth;

        _RB = GetComponent<Rigidbody>();
        _Context = GetComponentInParent<PlayerContext>();

        OnDamaged += updateHealthBar;
        OnHealed += updateHealthBar;
        OnDamaged += SpawnDamageIndicator;
    }

    public override void HealHP(float ExtraHP, bool OverHeal = false)
    {
        base.HealHP(ExtraHP, OverHeal);

        if (CurrentHealth > MaxHealth * 0.3F && Emit != null && Emit.isActiveAndEnabled)
        {
            SoundManager.Instance?.ReturnToPool(Emit);
            Emit = null;
            return;
        }
    }

    public override void SimpleDamage(float Damage)
    {
        base.SimpleDamage(Damage);
    }

    public override void SimpleDamage(Tuple<Vector3, float, float> MyTuple)
    {
        base.SimpleDamage(MyTuple);
    }

    public override void TakeDamage(DamageScore DamageDT)
    {
        base.TakeDamage(DamageDT);
    }

    public override void TakeDamageWithKnockback(Vector3 Direction, float knockback, DamageScore DamageDT)
    {
        base.TakeDamageWithKnockback(Direction, knockback, DamageDT);
    }

    SoundEmitter Emit;
    protected override void InternalDamage(float damage, Vector3? KnockbarDir, float force, DamageScore Score = null)
    {
        if (KnockbarDir.HasValue)
        {
            lastHitPosition = _Context.PlayerTransform.position + KnockbarDir.Value;
        }

        if (CurrentHealth - damage < MaxHealth * 0.3f && SoundManager.Instance != null)
        {
            if (Emit == null || !Emit.isActiveAndEnabled)
            {
                SoundManager.Instance.CreateSound()
                    .WithSoundData(_LowHP)
                    .WithRandomPitch(true)
                    .WithPosition(_Context.PlayerTransform.position)
                    .play(out SoundEmitter Emitter);
                Emit = Emitter;
            }
        }
        else if(Emit != null && CurrentHealth >= MaxHealth * 0.3f || CurrentHealth <= 0)
        {
            SoundManager.Instance.ReturnToPool(Emit);
             Emit = null;
        }
            base.InternalDamage(damage, KnockbarDir, force, Score);


    }


    private void updateHealthBar()
    {
        Combat_UI_Manager._Instance.UpdatePlayerHealthBar(CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0f)
        {
            // si no uso la chance de gulag lo mando ahi
            if (GameManager.Instance != null && !GameManager.Instance.AlreadyUseGulag)
            {
                ActiveGulag();
            }
            // si la uso no tiene gulag 
            else
            {

                // activo la lose screen
                if (Combat_UI_Manager._Instance != null)
                    Combat_UI_Manager._Instance.DisplayLose(true);

                // apago movimiento
                Player_Movement mov = _Context.GetComponentInChildren<Player_Movement>();
                if (mov != null)
                {
                    mov.enabled = false;
                }

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                // freezeo
                Time.timeScale = 0f; 
            }
        }
    }

    private void ActiveGulag()
    {
        GameManager.Instance.DeathPosition(this.transform.position);

        // busco el mov para tirar el tp cuando activo el gulag
        Player_Movement mov = _Context.GetComponentInChildren<Player_Movement>();
        if (mov != null)
        {
            Vector3 posGulag = new Vector3(5000f, 5000f, 5000f);
            mov.SetCharacterPosition(posGulag, true);
        }

        // revivo al player para el gulag 
        CurrentHealth = MaxHealth;
        Died = false;
        updateHealthBar();
    }

    public void VictoryGulag()
    {
        Player_Movement mov = _Context.GetComponentInChildren<Player_Movement>();
        if (mov != null)
        {
            // vuelvo a donde mori
            mov.SetCharacterPosition(GameManager.Instance.posAntesDeMorir, true);

            // reinicio vida y doy invulnerabilidad un toque para que no le peguen apenas se tepea
            CurrentHealth = MaxHealth;
            Died = false;
            updateHealthBar();
            StartCoroutine(TempInvulnerability(3f));
        }
    }

    private IEnumerator TempInvulnerability(float tiempo)
    {
        float originalInv = DamageInvulnerability;
        DamageInvulnerability = 999f;
        yield return new WaitForSeconds(tiempo);
        DamageInvulnerability = originalInv;
    }

    private void UpdateStatValues(StatIdentifier statID, float values)
    {
        switch (statID)
        {
            case var _ when statID == _MaxHealthID:
                MaxHealth = values;
                Combat_UI_Manager._Instance.UpdatePlayerHealthBar(CurrentHealth, MaxHealth, false);
                break;

            case var _ when statID == _BaseDefenseID:
                Defense = values;
                break;

            case var _ when statID == _BaseDamageReduction:
                DamageReduction = values;
                break;

            case var _ when statID == _BaseDamageInvulnerability:
                DamageInvulnerability = values;
                break;
        }
    }


    private void SpawnDamageIndicator()
    {
        if (_damageIndicatorPrefab == null) return;

        GameObject indicator = Instantiate(_damageIndicatorPrefab, Combat_UI_Manager._Instance.transform);
        indicator.SetActive(true);
        indicator.transform.localScale = Vector3.one;
        indicator.GetComponent<DamageIndicator>().SetDamageLocation(lastHitPosition, _Context.PlayerTransform);
    }

    private void OnDestroy()
    {
        if (Emit != null)
        {
            Emit.Stop();
            Emit = null;
        }
    }
}
