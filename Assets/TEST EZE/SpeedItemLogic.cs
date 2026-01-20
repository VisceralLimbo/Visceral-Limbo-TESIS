using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedItemLogic : ItemLogic
{
    //las variables que va a afectar el item
    [SerializeField] private float walkSpeedBoost = 5f;
    [SerializeField] private float crouchSpeedBoost = 3f;
    [SerializeField] private float airSpeedBoost = 4f;

    private Player_Base _playerBase;

    // bool para el mov
    private bool _isMoving = false;

    private float currentAlpha;
    private float targetAlpha;

    [SerializeField] SoundData _PickUpSound;

    [Header("Transición del efecto")]
    [SerializeField] private float fadeSpeed = 3f;

    // getter para el base
    private Player_Base PlayerBaseComponent{get
        {
            if (_playerBase == null && _Context != null)
            {
                _playerBase = _Context.GetComponentInParent<Player_Base>();
            }
            return _playerBase;
        }
    }

    //trigger
    private void OnTriggerEnter(Collider other)
    {
        if (IsInventoryMaster) return;

        if (other.CompareTag("Player"))
        {
            if (other.transform.parent.TryGetComponent(out PlayerContext context))
            {
                _Context = context;
                Inventory = context.Inventory;
                //como player movement esta en player model y eso es hijo de player obj tengo q llamarlo asi   
                OnPickUp();
            }
        }
    }

    private void Update()
    {
        if (ItemStacks <= 0 || PlayerBaseComponent == null) return;

        // leo input de playerbase
        Vector2 movementInput = PlayerBaseComponent.CurrentMovementInput.Movement;
        bool currentlyMoving = movementInput.sqrMagnitude > 0.01f;

        
        if (currentlyMoving != _isMoving)
        {
            _isMoving = currentlyMoving;
            targetAlpha = _isMoving ? 1.0f : 0.0f;
        }

        // transición progresiva hacia targetAlpha
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

        // aplicar al efecto
        HealthFullscreenEffect.Instance?.SetWindAlpha(currentAlpha);
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

            StatModifierFloat statMod = new StatModifierFloat // stat modificador de movimiento
            {
                ModifierValueFloat = walkSpeedBoost,
                ModType = ModifierType.flat,
                EffectName = "SpeedItemLogicBoost",
                Source = this
            };
            statManager.UpdateFloatStatValue("BaseSpeed", statMod);

            StatModifierFloat airMod = new StatModifierFloat // stat modificador de aire
            {
                ModifierValueFloat = airSpeedBoost,
                ModType = ModifierType.flat,
                EffectName = "SpeedItemJumpLogicBoost",
                Source = this
            };
            statManager.UpdateFloatStatValue("AirSpeed",airMod);

            StatModifierFloat CrouchMod = new StatModifierFloat // stat modificador de agachado
            {
                ModifierValueFloat = crouchSpeedBoost,
                ModType = ModifierType.flat,
                EffectName = "SpeedItemCrouchLogicBoost",
                Source = this
            };
            statManager.UpdateFloatStatValue("CrouchSpeed", CrouchMod);
            ItemStacks++;
        }
        else if (ItemStacks >= 1)
        {
            ItemStacks++;
            var statManager = _Context.Stats;
            StatModifierFloat statMod = new StatModifierFloat // stat modificador de movimiento
            {
                ModifierValueFloat = walkSpeedBoost * ItemStacks,
                ModType = ModifierType.flat,
                EffectName = "SpeedItemLogicBoost",
                Source = this
            };
            statManager.UpdateFloatStatValue("WalkSpeed", statMod);

            StatModifierFloat airMod = new StatModifierFloat // stat modificador de aire
            {
                ModifierValueFloat = airSpeedBoost * ItemStacks,
                ModType = ModifierType.flat,
                EffectName = "SpeedItemJumpLogicBoost",
                Source = this
            };
            statManager.UpdateFloatStatValue("AirSpeed", airMod);

            StatModifierFloat CrouchMod = new StatModifierFloat // stat modificador de agachado
            {
                ModifierValueFloat = crouchSpeedBoost * ItemStacks,
                ModType = ModifierType.flat,
                EffectName = "SpeedItemCrouchLogicBoost",
                Source = this
            };
            statManager.UpdateFloatStatValue("CrouchSpeed", CrouchMod);
        }
    }

    public override void OnDrop() 
    {
    }
    public override void RemoveStack() 
    {
        base.RemoveStack();
    }
    public override void Unregister() 
    {
        // apago shader cuando elimino registro
        if (HealthFullscreenEffect.Instance != null)
        {
            HealthFullscreenEffect.Instance.SetWindAlpha(0.0f); // apagado
        }
        base.Unregister();
    }
    public override void Register(InventoryManager inventory, PlayerContext Context)
    {
        base.Register(inventory, Context);
        // ref a base lista
        var dummy = PlayerBaseComponent;
    }
}
