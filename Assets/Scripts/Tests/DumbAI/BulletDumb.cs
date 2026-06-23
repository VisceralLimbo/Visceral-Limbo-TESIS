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
        //_Collider.enabled = false;
        //StartCoroutine(startDamage());
        if(_OwnerContext != null)
        {
            Physics.IgnoreCollision(this._Collider, _OwnerContext.PlayerTransform.GetComponent<Collider>());
        }
    }

    IEnumerator startDamage()
    {
        yield return new WaitForSeconds(0.01f);
        _Collider.enabled = true;

    }

    private void Update()
    {
        _Health -= (TimeDilationManager.GlobalTimeScale*Time.deltaTime); 
        this.transform.position += this.transform.forward * BulletSpeed * (TimeDilationManager.GlobalTimeScale*Time.deltaTime);

        if(_Health < 0)
        {
            Destroy(this.gameObject);
        }

    }
    public void SetOwner(GameObject Owner,PlayerContext Context)
    {
        if(Owner == null)
        {
            return;
        }

        //desactivamos el ignore de colisiones del viejo owner
        if(_OwnerContext != null)
        {
            Physics.IgnoreCollision(this._Collider, _OwnerContext.PlayerTransform.GetComponent<Collider>(), false);
        }

        _OwnerGameObject = Owner;
        _OwnerContext = Context;

        //activamos el ignore de colisiones del nuevo owner
        Physics.IgnoreCollision(this._Collider, _OwnerContext.PlayerTransform.GetComponent<Collider>());
    }

    public void SetDamage(float NewDamage)
    {
        damage = NewDamage;
    }


    private HashSet<Collider> TaggedColliders = new HashSet<Collider>();
    private HashSet<Health_Component> TaggedHealth = new HashSet<Health_Component>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == _OwnerContext.PlayerGameObject) return;
        if (TaggedColliders.Contains(other)) return;


        // priorizamos el Idamageable
        if (other.TryGetComponent(out IDamageable Idamage))
        {
            if (Idamage.GetHealthComponent(out Health_Component IHealth) && !TaggedHealth.Contains(IHealth))
            {
                if (IHealth.Context != _OwnerContext)
                {
                    TaggedColliders.Add(other);
                    TaggedHealth.Add(IHealth);

                    Vector3 Dir = IHealth.Context.PlayerTransform.position - _OwnerContext.PlayerTransform.position;
                    Dir.y = 0;


                    DamageScore DamageDT = new DamageScore
                    {
                        Attacker = _OwnerContext,
                        DamageAmount = Damage,
                        Victim = IHealth.Context, // Puede ser null si es un prop, lo manejamos abajo
                        ElementalDamage = ElementType.Physical,
                        FactionID = FactionID.LimboMonster1
                    };
                    DamageDT.AddTag(ScoreFlags.Skill1Kill);

                    if (IHealth.Context == null)
                    {
                        IHealth.SimpleDamage(Damage);
                    }
                    else
                    {
                        IHealth.TakeDamageWithKnockback(Dir.normalized, 0f, DamageDT);
                    }
                }
                else
                {
                    return;
                }



            }


        }

        else if (other.TryGetComponent(out Health_Component HPComp))
        {
            if (HPComp.Context != _OwnerContext)
            {
                TaggedColliders.Add(other);
                TaggedHealth.Add(HPComp);

                Vector3 Dir = HPComp.Context.PlayerTransform.position - _OwnerContext.PlayerTransform.position;
                Dir.y = 0;


                DamageScore DamageDT = new DamageScore
                {
                    Attacker = _OwnerContext,
                    DamageAmount = Damage,
                    Victim = HPComp.Context, // Puede ser null si es un prop, lo manejamos abajo
                    ElementalDamage = ElementType.Physical,
                    FactionID = FactionID.LimboMonster1
                };
                DamageDT.AddTag(ScoreFlags.Skill1Kill);

                if (HPComp.Context == null)
                {
                    HPComp.SimpleDamage(Damage);
                }
                else
                {
                    HPComp.TakeDamageWithKnockback(Dir.normalized, 0f, DamageDT);
                }
            }
            else
            {
                return;
            }


        }
        Destroy(this.gameObject);
    }

    public void parried(DamageScore? DMScore,Vector3 Direction = default)
    {
        if(Direction == Vector3.zero)
        {
            Debug.LogError("<Color=blue> Visceral Error: No direction for parry</Color>");
        }
        if (!DMScore.HasValue)
        {
            Debug.LogError("<Color=blue> Visceral Error: No DamageScore found for parry" + this.name + "</Color>");
            return;
        }

        print("rotate" + Direction);
        this.transform.forward = Direction;
        //transform.Rotate(finalDir);
        if (!Parried)
        {
            _Health = 10;
            BulletSpeed = BulletSpeed * 2;
            damage = damage * 2;

            SetOwner(DMScore.Value.Attacker.PlayerGameObject, DMScore.Value.Attacker);
            Parried = true;
        }
       
       
        
    }
}

//
// este script fue creado por patricio malvasio 2/5/2025
//
// este script es un prototipo de bala, 
// reemplazar mas adelante por un sistema mejor
//