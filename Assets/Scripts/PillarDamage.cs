using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PillarDamage : MonoBehaviour
{
    [Header("Configuracion de Danio")]
    [SerializeField] float _initialDamage = 20f; // Danio cuando aparece
    [SerializeField] float _dotDamage = 5f;      // Danio por tick
    [SerializeField] float _damageTickRate = 0.5f;
    [SerializeField] float _pillarDuration = 3f;
    [SerializeField] LayerMask _PlayerMask;
    [SerializeField] VisualEffect _PilarVisual;

    // Trackers separados para logica de Burst vs DoT
    private HashSet<Health_Component> _receivedInitialDamage = new HashSet<Health_Component>();
    private HashSet<Health_Component> _hitThisTick = new HashSet<Health_Component>();

    [SerializeField] PlayerContext _creatorContext;
    private Collider _damageCollider;

    public void Initialize(float duration, PlayerContext context)
    {
        _pillarDuration = duration;
        _damageCollider = GetComponent<Collider>();
        _creatorContext = context;
        if(_PilarVisual != null )
        {
            _PilarVisual.Play();
        }

        StartCoroutine(HandlePillarLifetime());
    }

    private IEnumerator HandlePillarLifetime()
    {
        float timer = 0f;

        while (timer < _pillarDuration)
        {
            _hitThisTick.Clear(); // Limpiamos solo los hits de ESTE tick para re-escanear el area

            Collider[] collidersInArea = Physics.OverlapBox(
                _damageCollider.bounds.center,
                _damageCollider.bounds.extents,
                Quaternion.identity,
                _PlayerMask
            );

            foreach (Collider col in collidersInArea)
            {
                ProcessHit(col);
            }

            // Esperamos el tickrate antes de volver a aplicar DoT
            yield return new WaitForSeconds(_damageTickRate);
            timer += _damageTickRate;
        }
        _PilarVisual.Stop();

        Destroy(gameObject);
    }

    private void ProcessHit(Collider other)
    {
        // Failsafe: Evitamos que el creador del pilar (el Jefe) se haga daño a si mismo
        if (_creatorContext != null && other.gameObject == _creatorContext.PlayerGameObject) return;

        // Priorizamos la interfaz IDamageable
        if (other.TryGetComponent(out IDamageable idamage))
        {
            if (idamage.GetHealthComponent(out Health_Component iHealth))
            {
                ApplyDamage(other, iHealth);
            }
        }
        else if (other.TryGetComponent(out Health_Component hpComp))
        {
            ApplyDamage(other, hpComp);
        }
    }

    private void ApplyDamage(Collider other, Health_Component healthComp)
    {
        // Si el enemigo ya recibio daño en este tick exacto (ej. tiene varios colliders), lo ignoramos
        if (_hitThisTick.Contains(healthComp)) return;

        // Failsafe extra de contexto para aliados/creador
        if (healthComp.Context != null && healthComp.Context == _creatorContext) return;

        _hitThisTick.Add(healthComp);

        // Determinamos si aplicamos el burst inicial o el danio sobre tiempo
        bool isInitialHit = !_receivedInitialDamage.Contains(healthComp);
        float damageToApply = isInitialHit ? _initialDamage : _dotDamage;

        if (isInitialHit)
        {
            _receivedInitialDamage.Add(healthComp);
        }

 
        DamageScore damageDT = new DamageScore
        {
            Attacker = _creatorContext,
            DamageAmount = damageToApply,
            Victim = healthComp.Context, 
            ElementalDamage = ElementType.Fire,
            FactionID = FactionID.LimboMonster1
        };

        if (healthComp.Context == null)
        {
            healthComp.SimpleDamage(damageToApply);
        }
        else
        {
            healthComp.TakeDamageWithKnockback(Vector3.zero, 0, damageDT);
        }
    }
}

