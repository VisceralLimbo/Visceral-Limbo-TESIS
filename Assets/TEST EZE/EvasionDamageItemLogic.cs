using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class EvasionDamageItemLogic : ItemLogic
{
    [SerializeField] SoundData _PickUpSound;

    [SerializeField] BuffSO _BuffSO;

    [SerializeField] BuffManager _BuffManager;

    //trigger de siempre
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

    //agarro todas las refs q neceisot
    private void GetReferences()
    {
        print("Registering stuff");
        if (_Context == null) return;
        PlayerEvents.OnPlayerUsedUtilitySkill += ActivateDamageBoost;
        _BuffManager = _Context.BuffManager;
        print("Passed Registration");
    }


    public override void Register(InventoryManager inventory, PlayerContext context)
    {
        base.Register(inventory, context);
        _Context = context;
        GetReferences();  
    }

    public override void Unregister()
    {

        PlayerEvents.OnPlayerUsedUtilitySkill -= ActivateDamageBoost;
        base.Unregister();
    }

    // se llama siempre q se haga un dash 
    private void ActivateDamageBoost()
    {
        if (_BuffManager != null)
        {
            // pasamos el buffo del Item
            _BuffManager.AddNewBuff(_BuffSO.BuffID, _BuffSO, ItemStacks, _Context);

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
        ItemStacks++;
    } 
    // no hay stacks para este me peleo con cualquiera
    // , por ende no lo pongo en el float stat list
    //MENTIRA!!!! TE DECLARO LA GUERRA WEON!!, pato
    public override void RemoveStack()
    {
        ItemStacks--;
        if(ItemStacks <= 0 && IsInventoryMaster)
        {
            Unregister();
            Destroy(this.gameObject);
        }
    } 
    public override void OnDrop() 
    {
        RemoveStack();
    }
}
