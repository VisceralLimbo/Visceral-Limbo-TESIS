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
        base.Unregister();
    }

    public override void OnPickUp()
    {
        Inventory.AddItemStack(_ItemDefinition);

        if (swordParticles != null)
        {
            swordParticles.gameObject.SetActive(true);
            swordParticles.Play();
        }

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

        if (_Sword != null)
        {
            Transform espada4 = _Sword.GetComponentsInChildren<Transform>(true)
                           .FirstOrDefault(t => t.name == "Espada4");

            if (espada4 != null)
            {
                Renderer swordRenderer = espada4.GetComponent<Renderer>();
                if (swordRenderer != null)
                {
                    Material mat = swordRenderer.material;

                    if (mat.HasProperty("_FresnelGradientBlend"))
                        mat.SetFloat("_FresnelGradientBlend", 0.04f);

                    
                     if (mat.HasProperty("_Color"))
                    {
                        Color currentColor = mat.GetColor("_Color");
                        float intensity = currentColor.maxColorComponent;
                        Color baseColor = Color.red * intensity;
                        mat.SetColor("_Color", baseColor);
                    }
                }
            }
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
