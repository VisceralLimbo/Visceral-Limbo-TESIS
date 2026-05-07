using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : Visceral_Script
{
    [SerializeField] private Visceral_WeaponBase _WeaponOwner;
    [SerializeField] private Collider _collider;
    [SerializeField] private PlayerContext _Context;

    private void Start()
    {
        _WeaponOwner = GetComponentInParent<Visceral_WeaponBase>();
        _WeaponOwner.AddWeaponCollider(this);
        _collider = GetComponent<Collider>();

        _Context = this.GetComponentInParent<PlayerContext>();
    }

    public void activateCollider()
    {
        _collider.enabled = true;
    }

    public void DeactivateCollider()
    {
        _collider.enabled = false;
        TaggedColliders.Clear();
        TaggedHealth.Clear();
    }


    private HashSet<Collider> TaggedColliders = new HashSet<Collider>();
    private HashSet<Health_Component> TaggedHealth = new HashSet<Health_Component>();
    private void OnTriggerEnter(Collider other)
    {
        if (_WeaponOwner != null)
        {

            other.TryGetComponent(out IDamageable Idamage);

            // vamos a priorizar la Interfaz Damageable,
            // porque significa que implementa algo especial
            // a la hora de recibir daño
            if (Idamage != null && !TaggedColliders.Contains(other))
            {
                if(Idamage.GetHealthComponent(out Health_Component HPComp) == true 
                                              && !TaggedHealth.Contains(HPComp))
                {
                    print("Hit box hit" + other.name);

                    if(HPComp.Context != _Context)
                    {
                        _WeaponOwner.NotifyHit(other, Idamage);
                        TaggedColliders.Add(other);
                        TaggedHealth.Add(HPComp);
                    } 
                }
            }

            // fallback de daño
            else if(other.TryGetComponent(out Health_Component HPComp)
                                         && !TaggedColliders.Contains(other)
                                         && !TaggedHealth.Contains(HPComp))
            {
               if(HPComp.Context != _Context)
                {
                    _WeaponOwner.NotifyHit(other, HPComp);
                    TaggedColliders.Add(other);
                    TaggedHealth.Add(HPComp);
                }         
            }
        }
    }
}

//
// creado por patricio malvasio maddalena 
// 2/5/2025
//
// descripcion: script usado para detectar colisiones y enviar a un script de tipo arma
