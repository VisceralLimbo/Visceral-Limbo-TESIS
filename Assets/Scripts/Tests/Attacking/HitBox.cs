using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : Visceral_Script
{
    [SerializeField] private Visceral_WeaponBase _WeaponOwner;
    [SerializeField] private Collider _collider;

    private void Start()
    {
        _WeaponOwner = GetComponentInParent<Visceral_WeaponBase>();
        _WeaponOwner.AddWeaponCollider(this);
        _collider = GetComponent<Collider>();

    }

    public void activateCollider()
    {
        _collider.enabled = true;
    }

    public void DeactivateCollider()
    {
        _collider.enabled = false;
        TaggedColliders.Clear();
    }


    private List<Collider> TaggedColliders = new List<Collider>();
    private void OnTriggerEnter(Collider other)
    {
        if (_WeaponOwner != null)
        {
            if (other.TryGetComponent(out Health_Component HPComp) && !TaggedColliders.Contains(other))
            {
                _WeaponOwner.NotifyHit(other, HPComp);
                TaggedColliders.Add(other);
            }
        }
    }




}

//
// creado por patricio malvasio maddalena 
// 2/5/2025
//
// descripcion: script usado para detectar colisiones y enviar a un script de tipo arma
