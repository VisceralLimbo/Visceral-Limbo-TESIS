using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class SpeedItemLogic : ItemLogic
{
    //las variables que va a afectar el item xd mas claro no se (te mato pato 3 horas viendo el PlayerBase pensando que eso era la velocidad porque me dijiste que SI ERA) 
    [SerializeField] private float walkSpeedBoost = 5f;
    [SerializeField] private float crouchSpeedBoost = 3f;
    [SerializeField] private float airSpeedBoost = 4f;

    //ref de Player_Movement
    private Player_Movement player;

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
                player = context.GetComponentInChildren<Player_Movement>();
                OnPickUp();
            }
        }
    }

    public override void OnPickUp()
    {
        Inventory.AddItemStack(_ItemDefinition);
        Destroy(this.gameObject);

        if (player == null)
        {
            return;
        }

        // aplico velocidad apenas agarro
        player.AddSpeed(walkSpeedBoost, crouchSpeedBoost, airSpeedBoost);
    }

    public override void AddStack()
    {
        if (ItemStacks == 0)
        {
            var statManager = _Context.Stats;

            StatModifierFloat statMod = new StatModifierFloat
            {
                ModifierValueFloat = walkSpeedBoost,
                ModType = ModifierType.flat,
                EffectName = "SpeedItemLogicBoost",
                Source = this
            };
            statManager.UpdateFloatStatValue("WalkSpeed", statMod);

            ItemStacks++;
        }
        else if (ItemStacks >= 1)
        {
            ItemStacks++;
            var statManager = _Context.Stats;

            StatModifierFloat statMod = new StatModifierFloat
            {
                ModifierValueFloat = walkSpeedBoost * ItemStacks,
                ModType = ModifierType.flat,
                EffectName = "SpeedItemLogicBoost",
                Source = this
            };
            statManager.UpdateFloatStatValue("WalkSpeed", statMod);
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
