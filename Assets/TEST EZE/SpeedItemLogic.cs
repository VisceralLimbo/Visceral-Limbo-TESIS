using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedItemLogic : ItemLogic
{
    //las variables que va a afectar el item xd mas claro no se (te mato pato 3 horas viendo el PlayerBase pensando que eso era la velocidad porque me dijiste que SI ERA)
    // lmao, lo siento xd - pato
    [SerializeField] private float walkSpeedBoost = 5f;
    [SerializeField] private float crouchSpeedBoost = 3f;
    [SerializeField] private float airSpeedBoost = 4f;

    //el trigger nada mas q decir :p 
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

    public override void OnPickUp()
    {
        Inventory.AddItemStack(_ItemDefinition);
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
            statManager.UpdateFloatStatValue("WalkSpeed", statMod);

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
    }
    public override void Register(InventoryManager inventory, PlayerContext Context)
    {
        base.Register(inventory, Context);
    }
}
