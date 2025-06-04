using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletDumb : MonoBehaviour, IParriable
{
    [SerializeField] private float BulletSpeed,damage;
    [SerializeField] private float Damage;
    [SerializeField] Collider _Collider;
    [SerializeField] private float _Health;

    [SerializeField] private GameObject _OwnerGameObject;
    [SerializeField] private PlayerContext _OwnerContext;

    [SerializeField] bool Parried;


    private void Start()
    {
        _Collider = GetComponent<Collider>();
        _Collider.enabled = false;
        StartCoroutine(startDamage());
        Physics.IgnoreCollision(this._Collider,_OwnerContext.PlayerTransform.root.GetComponentInChildren<Collider>());
    }

    IEnumerator startDamage()
    {
        yield return new WaitForSeconds(0.01f);
        _Collider.enabled = true;

    }

    private void Update()
    {
        _Health -= Time.deltaTime; 
        this.transform.position += this.transform.forward * BulletSpeed * Time.deltaTime;

        if(_Health < 0)
        {
            Destroy(this.gameObject);
        }

    }
    public void SetOwner(GameObject Owner,PlayerContext Context)
    {
        //desactivamos el ignore de colisiones del viejo owner
        if(_OwnerContext != null)
        {
            Physics.IgnoreCollision(this._Collider, _OwnerContext.PlayerTransform.root.GetComponentInChildren<Collider>(), false);
        }

        _OwnerGameObject = Owner;
        _OwnerContext = Context;

        //activamos el ignore de colisiones del nuevo owner
        Physics.IgnoreCollision(this._Collider, _OwnerContext.PlayerTransform.root.GetComponentInChildren<Collider>());
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == _OwnerGameObject) return;
        if (other.GetComponent<PlayerContext>() == _OwnerContext) return;
        if (other.GetComponent<Visceral_SkillLogic>()) return;

        if(other.TryGetComponent(out Health_Component HPComp))
        {
            Vector3 dir = other.gameObject.transform.position - this.transform.position;

            DamageScore DamageDT = new DamageScore();
            DamageDT.Attacker = _OwnerContext;
            DamageDT.DamageAmount = damage;
            DamageDT.Victim = other.GetComponent<PlayerContext>();
            DamageDT.ElementalDamage = ElementType.Physical;
            DamageDT.FactionID = FactionID.LimboMonster1;
            if(Parried)DamageDT.AddTag(ScoreFlags.Parried);
           
            if(HPComp.Context == null) { HPComp.SimpleDamage(damage);return; }
            HPComp.TakeDamageWithKnockback(dir.normalized, 5, DamageDT);
   
        }

        Destroy(this.gameObject);
    }

    public void parried(DamageScore DMScore,Vector3 Direction = default)
    {
        if(Direction == Vector3.zero)
        {
            Debug.LogError("<Color=blue> Visceral Error: No direction for parry</Color>");
        }

        print("rotate" + Direction);
        this.transform.forward = Direction;
        //transform.Rotate(finalDir);
        _Health = 10;
        BulletSpeed = BulletSpeed * 2;
        damage = damage * 2;

        SetOwner(DMScore.Attacker.PlayerGameObject, DMScore.Attacker);
        Parried = true;

    }
}

//
// este script fue creado por patricio malvasio 2/5/2025
//
// este script es un prototipo de bala, 
// reemplazar mas adelante por un sistema mejor
//