using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BleedItemLogic : ItemLogic
{
    // danio base por tick
    [SerializeField] private float baseBleedDamage = 2f;
    // duracion del sangrado
    [SerializeField] private float bleedDuration = 5f;
    // tickrate
    [SerializeField] private float bleedTickRate = 1f;

    // refe de la espada
    private SwordTest _Sword;
    private ParticleSystem swordParticles;

    private Player_MeleeAttack _playerMeleeAttack;
    private Player_MeleeAttack PlayerMeleeAttackComponent{get
        {
            if (_playerMeleeAttack == null && _Context != null)
            {
                _playerMeleeAttack = _Context.GetComponent<Player_MeleeAttack>();
            }
            return _playerMeleeAttack;
        }
    }

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

    public override void Register(InventoryManager inventory, PlayerContext context)
    {
        base.Register(inventory, context);
        //agarro el swordtest
        _Sword = _Context.GetComponentInChildren<SwordTest>();

        if (_Sword != null)
        {
            // busco particulas la espada
            swordParticles = _Sword.GetComponentInChildren<ParticleSystem>(true);
            swordParticles.Play();
        }
    }

    public override void Unregister()
    {
        // notifico efecto off
        PlayerMeleeAttackComponent?.SetBleedEffectActive(false);

        // paro particulas (de igual manera el bleed nunca se desactiva una vez agarrado pero lo dejo seteado igual)
        if (swordParticles != null)
        {
            swordParticles.Stop();
            swordParticles.gameObject.SetActive(false);
        }

        base.Unregister();
    }

    public override void OnPickUp()
    {
        Inventory.AddItemStack(_ItemDefinition);
        Destroy(this.gameObject);
    }

    public override void OnDrop()
    {
    }

    public override void AddStack()
    {
        if (ItemStacks == 0)
        {
            var statManager = _Context.Stats;

            StatModifierFloat statMod = new StatModifierFloat // stat modificador de danio de sangrado
            {
                ModifierValueFloat = baseBleedDamage,
                ModType = ModifierType.flat,
                EffectName = "BleedItemLogic",
                Source = this
            };
            statManager.UpdateFloatStatValue("Bleed", statMod);
            ItemStacks++;
        }
        else if (ItemStacks >= 1)
        {
            ItemStacks++;
            var statManager = _Context.Stats;
            StatModifierFloat statMod = new StatModifierFloat // stat modificador de danio de sangrado
            {
                ModifierValueFloat = baseBleedDamage * ItemStacks,
                ModType = ModifierType.flat,
                EffectName = "BleedItemLogic",
                Source = this
            };
            statManager.UpdateFloatStatValue("Bleed", statMod);
        }

        // pase todo el efecto a playerattack quiza deberiamos tener un manager para todos los efectos son bastantes..
        PlayerMeleeAttackComponent?.SetBleedEffectActive(true);

        // particulas ya no estan en pickup
        if (swordParticles != null)
        {
            swordParticles.gameObject.SetActive(true);
            swordParticles.Play();
        }
    }

    public override void RemoveStack()
    {
        base.RemoveStack();
    }

    // func para obtener los valores del sangrado de swordtest
    public float GetBleedDamage() => baseBleedDamage * ItemStacks;
    public float GetBleedDuration() => bleedDuration;
    public float GetBleedTickRate() => bleedTickRate;
}
