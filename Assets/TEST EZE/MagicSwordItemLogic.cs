using UnityEngine;
using System;
using System.Collections;

public class MagicSwordItemLogic : ItemLogic
{
    [SerializeField] private GameObject _MagicProjectilePrefab; // prefab del proyectil
    private Transform _SpawnPoint;                              // spawn para el proyectil
    [SerializeField] private float _ProjectileSpeed = 15f;      // velocidad
    [SerializeField] private float _BaseDamage = 20f;           // daño
    [SerializeField] private float _MagicCooldownTime = 15f;    // cd del ataque
    private float _NextFireTime = 0f; // cuando se puede usar de nuevo

    // refe del ataque
    private Player_MeleeAttack _playerMeleeAttack;

    [SerializeField] SoundData _PickUpSound;

    private void OnTriggerEnter(Collider other)
    {
        if (IsInventoryMaster) return;

        if (other.CompareTag("Player"))
        {
            if (other.transform.parent.TryGetComponent(out PlayerContext context))
            {
                _Context = context;
                Inventory = context.Inventory;
                OnPickUp();
            }
        }
    }

    private void GetReferences()
    {
        if (_Context == null) return;

        _playerMeleeAttack = _Context.GetComponent<Player_MeleeAttack>();

        // busco el spawnpoint
        Transform rootTransform = _Context.transform;

        _SpawnPoint = rootTransform.Find("SpawnPointMagicAttack");
    }

    public override void Register(InventoryManager inventory, PlayerContext context)
    {
        base.Register(inventory, context);
        GetReferences();

        if (_playerMeleeAttack != null)
        {
            // suscribo al evento de la espada
            _playerMeleeAttack.OnMeleeAttackCompleted += ShootProjectile;
        }
    }

    public override void Unregister()
    {
        if (_playerMeleeAttack != null)
        {
            _playerMeleeAttack.OnMeleeAttackCompleted -= ShootProjectile;
        }

        base.Unregister();
    }

    private void ShootProjectile()
    {
        // valor actualizado del stat
        float finalDamage = _BaseDamage;

        if (_Context != null && _Context.Stats != null)
        {
            finalDamage = _Context.Stats.GetFloatStatValue("MagicAttack");
            if (finalDamage < 0) finalDamage = _BaseDamage;
        }

        // chequeo el cd
        if (Time.time < _NextFireTime)
        {
            return;
        }

        // lo reinicio
        _NextFireTime = Time.time + _MagicCooldownTime;

        Vector3 spawnPos;
        Vector3 shootDirection;

        // donde lo spawneo
        if (_SpawnPoint != null)
        {
            spawnPos = _SpawnPoint.position;
            shootDirection = _SpawnPoint.forward;
        }
        else
        {
            spawnPos = _Context.PlayerTransform.position + Vector3.up * 1.5f;
            shootDirection = _Context.PlayerTransform.forward;
        }

        GameObject projectileGO = Instantiate(_MagicProjectilePrefab, spawnPos, Quaternion.LookRotation(shootDirection));

        if (projectileGO.TryGetComponent(out ProjectileLogic pLogic))
        {
            pLogic.SetupProjectile(_Context, finalDamage, _ProjectileSpeed, shootDirection);
        }
    }

    public override void OnPickUp()
    {
        Inventory.AddItemStack(_ItemDefinition);
        SoundManager.Instance.CreateSound().WithSoundData(_PickUpSound).WithPosition(this.transform.position).play();
        Destroy(this.gameObject);
    }

    public override void AddStack() 
    {
        if (ItemStacks == 0)
        {
            var statManager = _Context.Stats;

            StatModifierFloat statMod = new StatModifierFloat // stat modificador de daño base del proyectil
            {
                ModifierValueFloat = _BaseDamage,
                ModType = ModifierType.flat,
                EffectName = "MagicAttackSword",
                Source = this
            };
            statManager.UpdateFloatStatValue("MagicAttack", statMod);
            ItemStacks++;
        }
        else if (ItemStacks >= 1)
        {
            ItemStacks++;
            var statManager = _Context.Stats;
            StatModifierFloat statMod = new StatModifierFloat
            {
                ModifierValueFloat = _BaseDamage * ItemStacks,
                ModType = ModifierType.flat,
                EffectName = "MagicAttackSword",
                Source = this
            };
            statManager.UpdateFloatStatValue("MagicAttack", statMod);

            //quiza podria tambien mejorar el tiempo de cd xd idk
        }
    }
    public override void RemoveStack() { }
    public override void OnDrop() { }
}