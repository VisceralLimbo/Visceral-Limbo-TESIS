using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BleedItemLogic : ItemLogic, I_OnHitItem
{




    [Tooltip("Bleed Scriptable Object Data")]
    [SerializeField] BuffSO _BleedSO;

    [Space]
    // refe de la espada
    private SwordTest _Sword;
    private ParticleSystem swordParticles;

    [SerializeField] SoundData _PickUpSound;

    private Player_MeleeVisualsComponent _playerMeleeAttack;
    private Player_MeleeVisualsComponent PlayerMeleeVisualComponent{get
        {
            if (_playerMeleeAttack == null && _Context != null)
            {
                _playerMeleeAttack = _Context.GetComponent<Player_MeleeVisualsComponent>();
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
        PlayerMeleeVisualComponent?.SetBleedVisuals(false);

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
        SoundManager.Instance.CreateSound().WithSoundData(_PickUpSound).WithPosition(this.transform.position).play();
        Destroy(this.gameObject);
    }

    public override void OnDrop()
    {
    }

    public override void AddStack()
    {

        // pase todo el efecto a playerattack quiza deberiamos tener un manager para todos los efectos son bastantes..
        PlayerMeleeVisualComponent?.SetBleedVisuals(true);

        // particulas ya no estan en pickup
        if (swordParticles != null)
            {
                swordParticles.gameObject.SetActive(true);
                swordParticles.Play();
            }

        ItemStacks += 1;
     
    }

    public override void RemoveStack()
    {
        base.RemoveStack();
    }




    public void OnProcEffect(PlayerContext Inflictor, DamageScore DMS, Health_Component VictimHP)
    {
        if(Inflictor == null || DMS == null || VictimHP == null)
        {
            Debug.Log("Couldnt inflict bleed Inflictor = " 
                + Inflictor.name 
                + " DamageScore " 
                + DMS + " HealthComponent " 
                + VictimHP.name);
            return;
        }

        // asumimos que golpeamos a algo correcto

        //1) ADQUIRIMOS EL BUFFMANAGER DE LA VICTIMA
        if(DMS.Victim != null)
        {
                if (DMS.Victim.BuffManager != null)
                {
                    BuffManager buffManager = DMS.Victim.BuffManager;
                    buffManager.AddNewBuff(_BleedSO.BuffID, _BleedSO, ItemStacks, Inflictor);
                }
        }
   

      

    }
}
