using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class EvasionDamageItemLogic : ItemLogic
{
    private Player_MeleeAttack _playerAttack;
    private Dash_Skill _playerDashSkill; // ref del dash
    // NO ESTA TOMANDO STACKS PORQUE NO SIENTO QUE TENGA QUE. el jugador no tiene ninguna penalizacion por spamear shift no tiene sentido que pueda stackar daño y hacer un x5 solo por tocar shift tampoco un x2 pero bueno :v

    [SerializeField] SoundData _PickUpSound;

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
        if (_Context == null) return;

        _playerAttack = _Context.GetComponent<Player_MeleeAttack>();

        if (_playerDashSkill == null)
        {
            _playerDashSkill = _Context.GetComponentInChildren<Dash_Skill>();
        }

        if (_playerAttack == null || _playerDashSkill == null)
        {
            Debug.LogError("PORQUE NO ME TOMA LOS COMPONENTES LA REPU"); //fixeado je
        }
    }


    public override void Register(InventoryManager inventory, PlayerContext context)
    {
        base.Register(inventory, context);
        GetReferences();

        if (_playerDashSkill != null)
        {
            // me suscribo al nuevo evento en el dash
            _playerDashSkill.OnDashActivated += ActivateDamageBoost;
            Debug.Log("me suscribi al evento de dash");
        }
    }

    public override void Unregister()
    {
        if (_playerDashSkill != null)
        {
            _playerDashSkill.OnDashActivated -= ActivateDamageBoost;
        }

        if (_playerAttack != null)
        {
            _playerAttack.IsEvasionBoostActive = false;
        }

        base.Unregister();
    }

    // se llama siempre q se haga un dash 
    private void ActivateDamageBoost()
    {
        if (_playerAttack != null)
        {
            if (!_playerAttack.IsEvasionBoostActive)
            {
                // solo registro la primera
                _playerAttack.RegisterEvasionBoost();
            }

            // activo daño para el proximo golpe
            _playerAttack.IsEvasionBoostActive = true;
            Debug.Log("TENGO DOUBLE DAMAGE EN EL PROXIMO ATAQUE x2");
        }
    }

    public override void OnPickUp()
    {
        Inventory.AddItemStack(_ItemDefinition);
        SoundManager.Instance.CreateSound().WithSoundData(_PickUpSound).WithPosition(this.transform.position).play();
        Destroy(this.gameObject);
    }

    public override void AddStack(){} // no hay stacks para este me peleo con cualquiera, por ende no lo pongo en el float stat list
    public override void RemoveStack(){} 
    public override void OnDrop() { }
}
