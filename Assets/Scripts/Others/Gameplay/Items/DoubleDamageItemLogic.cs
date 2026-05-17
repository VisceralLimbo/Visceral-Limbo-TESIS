using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DoubleDamageItemLogic : ItemLogic
{
    [SerializeField] private float damageMultiplierBoost = 2f;
    [SerializeField] private StatIdentifier DAMAGE_STAT_NAME;
    private const string EFFECT_NAME = "DoubleDamageItemLogicBoost";

    [SerializeField] Player_MeleeVisualsComponent _PlayerVisuals;

    // guardo el acumulado del item
    private float _totalBoostValue = 0f;
    private bool _isBoostActive = false;


    [SerializeField] SoundData _PickUpSound;

    private Player_MeleeVisualsComponent PlayerMeleeVisualsComponent{get
        {
            if (_PlayerVisuals == null && _Context != null)
            {
                _PlayerVisuals = _Context.GetComponent<Player_MeleeVisualsComponent>();
            }
            return _PlayerVisuals;
        }
    }

    private Player_HealthComp _playerHealthComp;

    private Player_HealthComp PlayerHealthComponent{get
        {
            if (_playerHealthComp == null && _Context != null)
            {
                _playerHealthComp = _Context.GetComponentInChildren<Player_HealthComp>();
            }
            return _playerHealthComp;
        }
    }

    //triger de siempre 
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

    // se llama cada que la vida cambie (mas que nada por esto de q se cura y bla bla)
    private void CheckHealthForDamageBoost()
    {
        // chequeo
        if (PlayerHealthComponent == null || ItemStacks <= 0)
        {
            RemoveDamageModifier();
            return;
        }

        // vida actual del player y vida max
        float currentHP = PlayerHealthComponent.CurrentHealth;
        float maxHP = PlayerHealthComponent.MaxHealth;

        // calculo el 30% como el max puede cambiar por las posiones por eso chequeo el max con cada checkhealth 
        float hpThresholdPercentage = maxHP * 0.30f;

        if (currentHP < hpThresholdPercentage)
        {
            // si la vida pasa< el %30 se activa el dd (aplico)
            ApplyDamageModifier(_totalBoostValue);
        }
        else
        {
            // si se cura y pasa> 30% el umbral se desactiva (remuevo)
            RemoveDamageModifier();
        }
    }

    private void ApplyDamageModifier(float value)
    {
        // evito recalcular si esta activo y no cambio el acumulado
        if (_isBoostActive && _totalBoostValue == value) return;

        var statManager = _Context.Stats;
        StatModifierFloat damageMod = new StatModifierFloat
        {
            ModifierValueFloat = value,
            ModType = ModifierType.PercentMult,
            EffectName = EFFECT_NAME, // el nombre seria el ID
            Source = this
        };

        statManager.UpdateFloatStatValue(DAMAGE_STAT_NAME, damageMod);
        _isBoostActive = true;

        // registro boost activo
        _PlayerVisuals?.AddBoostVisualSource();
    }

    //remuevo
    private void RemoveDamageModifier()
    {
        if (!_isBoostActive) return;

        if (_Context != null && _Context.Stats != null)
        {
            // uso el nuevo removefloat
            _Context.Stats.RemoveFloatStatModifier(DAMAGE_STAT_NAME, EFFECT_NAME);
            _isBoostActive = false;

            // elimino registro
            _PlayerVisuals.RemoveBoostVisualSource();
        }
    }

    //stacks
    public override void AddStack()
    {
        ItemStacks++;

        // para que escale linealmente la potencia del boost,
        // hago esta cuenta matematica. donde el valor inicial es multiplicado por el escalado del stack
        // PARA CONTROLAR LA TASA DE ESCALADO, HAY QUE CAMBIAR EL 0.05F DONDE 1.05F REPRESENTA UN CRECIMIENTO DEL 105% DE POTENCIA
        if(ItemStacks > 1)
        {
            //this
            _totalBoostValue = damageMultiplierBoost * (1 * 0.05f * (ItemStacks - 1));
        }
        else
        {
            _totalBoostValue = damageMultiplierBoost;
        }

        if (_Context != null)
        {
            // chequea la vida con el mod acumulado
            CheckHealthForDamageBoost();
        }
    }

    //remove
    public override void RemoveStack()
    {
        //dejo el remover 1 stack (no funca (solo remueve un stack no todo), tengo que hacer el remover en el stat y statmananger 100%)

        //ItemStacks--;
        //ItemDefinitionSO.ItemStack--;

        //if (ItemStacks >= 1)
        //{
        //    _totalBoostValue = damageMultiplierBoost * ItemStacks;

        //    if (_isBoostActive)
        //    {
        //        // actualizo el mod con el acumulado reducido
        //        ApplyDamageModifier(_totalBoostValue);
        //    }
        //}
        //else // si se remueve el ultimo stack unregister
        //{
        //    _totalBoostValue = 0f;
        //    Unregister();
        //}
    }

    public override void Register(InventoryManager inventory, PlayerContext context)
    {
        base.Register(inventory, context);

        // me suscribo a los eventos de salud onhealed y ondamaged
        if (PlayerHealthComponent != null)
        {
            // subs
            PlayerHealthComponent.OnDamaged += CheckHealthForDamageBoost;
            PlayerHealthComponent.OnHealed += CheckHealthForDamageBoost;
        }

        // hago el chequeo de cambio de vida
        CheckHealthForDamageBoost();
    }

    public override void Unregister()
    {
        // me desuscribo
        if (PlayerHealthComponent != null)
        {
            PlayerHealthComponent.OnDamaged -= CheckHealthForDamageBoost;
            PlayerHealthComponent.OnHealed -= CheckHealthForDamageBoost;
        }

        // chequeo q se elimine el dorado al elminar registro
        if (_isBoostActive)
        {
            _PlayerVisuals?.AddBoostVisualSource();
        }

        // chequeo que se removio del statmanager
        RemoveDamageModifier();

        base.Unregister();
    }

    public override void OnPickUp()
    {
        Inventory.AddItemStack(_ItemDefinition);
        SoundManager.Instance.CreateSound().WithSoundData(_PickUpSound).WithPosition(this.transform.position).play();
        // chequeo
        if (_Context != null && ItemStacks > 0)
        {
            CheckHealthForDamageBoost();
        }

        Destroy(this.gameObject);
    }

    public override void OnDrop()
    {}
}
