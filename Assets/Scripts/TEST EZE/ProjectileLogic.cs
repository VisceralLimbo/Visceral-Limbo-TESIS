using UnityEngine;

public class ProjectileLogic : MonoBehaviour
{
    private PlayerContext _SourceContext;
    private float _DamageAmount;
    private float _Speed;
    private Vector3 _Direction;

    // agarra toda la config cuando se instancia
    public void SetupProjectile(PlayerContext context, float damage, float speed, Vector3 direction)
    {
        _SourceContext = context;
        _DamageAmount = damage;
        _Speed = speed;
        _Direction = direction.normalized;
        // se rompe dsp de no chocar con nada en 5seg
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        transform.position += _Direction * _Speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // que no golpee al jugador ni al mismo projectil si en algun momento hacemos que reduzca el cd
        if (other.CompareTag("Player") || other.GetComponent<ProjectileLogic>() != null)
        {
            return;
        }

        // si golpea algo con componente de salud entra aca
        if (other.TryGetComponent(out Health_Component HPComp))
        {
            PlayerContext victimCtx = HPComp.Context;

            DamageScore dmg = new DamageScore
            {
                Attacker = _SourceContext,
                Victim = victimCtx,
                DamageAmount = _DamageAmount,
                ElementalDamage = ElementType.Magic,
                FactionID = _SourceContext.faction
            };

            if (victimCtx != null)
            {
                HPComp.TakeDamageWithKnockback(Vector3.zero, 0, dmg);
            }
            else
            {
                HPComp.SimpleDamage(_DamageAmount);
            }
        }

        // destruyo dsp de q choca
        Destroy(gameObject);
    }
}
