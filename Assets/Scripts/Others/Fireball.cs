using UnityEngine;

public class Fireball : MonoBehaviour, IParriable
{
    [SerializeField] float damage;
    [SerializeField] float Health;
    [SerializeField] float Speed;
    [SerializeField] GameObject Owner;
    [SerializeField] PlayerContext OwnerContext;
    [SerializeField] bool _Parried;
    [SerializeField] ParticleSystem fireballParticles;
    [SerializeField] Rigidbody _RB;


    public void Initiliaze(PlayerContext NewOwnerContext)
    {
        OwnerContext = NewOwnerContext;
        Owner = OwnerContext.PlayerGameObject;
        _RB = this.GetComponent<Rigidbody>();
        fireballParticles.Play();
        SetOwner(Owner,OwnerContext);
    }

    public void Update()
    {
        Health -= Time.deltaTime;
        this.transform.position += this.transform.forward * Speed * Time.deltaTime;

        if (Health < 0) 
        {
            Destroy(this.gameObject);
        }
    }



    public void parried(DamageScore DMScore = null, Vector3 Direction = default)
    {
        print("i got parried");

        if (Direction == Vector3.zero)
        {
            Debug.LogError("<Color=blue> Visceral Error: No direction for parry</Color>");
        }
        if(this.transform == null || this.gameObject == null)
        {
            return;
        }


        print("rotate" + Direction);
        this.transform.forward = Direction;
        if (!_Parried)
        {
            Speed *= 1.2f;
            damage = damage * 2;
            _Parried = true;
            SetOwner(DMScore.Attacker.gameObject, DMScore.Attacker);
        }
     

    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject == Owner) return;
        if (collision.gameObject.GetComponent<PlayerContext>() == OwnerContext) return;
        if (collision.gameObject.GetComponent<Visceral_SkillLogic>()) return;

        if (collision.gameObject.TryGetComponent(out Health_Component HPComp))
        {
            Vector3 dir = collision.gameObject.transform.position - this.transform.position;

            DamageScore DamageDT = new DamageScore();
            DamageDT.Attacker = OwnerContext;
            DamageDT.DamageAmount = damage;
            DamageDT.Victim = collision.gameObject.GetComponent<PlayerContext>();
            DamageDT.ElementalDamage = ElementType.Physical;
            DamageDT.FactionID = FactionID.LimboMonster1;
            if (_Parried) DamageDT.AddTag(ScoreFlags.Parried);

            if (HPComp.Context == null) { HPComp.SimpleDamage(damage); return; }
            HPComp.TakeDamageWithKnockback(dir.normalized, 0, DamageDT);
            

        }

        //this.gameObject.SetActive(false);
        Destroy(this.gameObject);
    }


    public void SetOwner(GameObject NewOwner, PlayerContext Context)
    {
        var col = this.GetComponent<Collider>();
        //desactivamos el ignore de colisiones del viejo owner
        if (Owner != null)
        {
            Physics.IgnoreCollision(col, OwnerContext.PlayerTransform.root.GetComponentInChildren<Collider>(), false);
        }

        Owner = NewOwner;
        OwnerContext = Context;

        //activamos el ignore de colisiones del nuevo owner
        Physics.IgnoreCollision(col, OwnerContext.PlayerTransform.root.GetComponentInChildren<Collider>());
    }
}

