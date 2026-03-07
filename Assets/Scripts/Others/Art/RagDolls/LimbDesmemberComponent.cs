using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbDesmemberComponent : MonoBehaviour, IDamageable
{

    [Header("References")]
    [Tooltip("EL prefab de la parte desmembrada, esta parte será instanciada en tiempo real para crear la ilusion de desmembramiento")]
    [SerializeField] GameObject _LimbDismemberPrefab;

    [Tooltip("El RagdollTimer, usado para comunicar datos al LimbDesmember")]
    [SerializeField] RagDollTimer _RagTimer;

    [Tooltip("El HealthComponent del NPC")]
    [SerializeField] Health_Component _HPComp;

    [Header("Variables")]
    [Tooltip("La salud de la extremidad, al llegar a cero, es desmembrada")]
    [SerializeField] float _LimbHealth;

    [Tooltip("whether the limb can be dismembered or not")]
    [SerializeField] bool _IsDismemberable;

    [Tooltip("Modificador de daño de la extremidad, es un multiplicador al daño entrante ANTES DE APLICAR DEFENSAS")]
    [Range(0,5)]
    [SerializeField] float _LimbDamageReduction = 1.0f;

    [Tooltip("El CharacterJoint del Limb")]
    [SerializeField] CharacterJoint[] _joint;

    [Tooltip("El rigidbody del Limb")]
    [SerializeField] Rigidbody[] _Rb;

    [Tooltip("El Collider del Limb")]
    [SerializeField] Collider[] _Col;

    Vector3 _KnockbackDir= Vector3.zero;
    float _KnockbackForce = 0f;

    private void Start()
    {
        if (_joint.Length <= 0) _joint = GetComponentsInChildren<CharacterJoint>();
        if (_Rb.Length <= 0) _Rb = GetComponentsInChildren<Rigidbody>();
        if(_Col.Length <= 0) _Col= GetComponentsInChildren<Collider>();
    }
    public void DismemberLimb()
    {
        if(_IsDismemberable == false) return;
        if (_LimbHealth > 0) return;

        // 1. Destruir las uniones (CharacterJoint) para que el hueso quede libre
        if (_joint.Length > 0)
        {
            foreach(var Joint in _joint)
            {
                Destroy(Joint);
            }
        }

        // 2. Destruir el Rigidbody para quitarle la masa y la gravedad
        if (_Rb.Length > 0)
        {
            foreach (var Rb in _Rb)
            {
                Destroy(Rb);
            }
        }

        // 3. Destruir el Collider para que no choque con otras partes del cuerpo al encogerse

        if (_Col.Length > 0)
        {
            foreach (var Col in _Col)
            {
                Destroy(Col);
            }
        }

        // 4. Aplicar el "Cero Seguro" (prácticamente invisible pero matemáticamente estable)
        this.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        if(_LimbDismemberPrefab != null)
        {
            GameObject Gibbedlimb = Instantiate
            (_LimbDismemberPrefab, this.transform.position, this.transform.rotation);

            Rigidbody rb = Gibbedlimb.GetComponent<Rigidbody>();
            rb.AddForce(_KnockbackDir * _KnockbackForce, ForceMode.Impulse);

        }
    }




    void IDamageable.SimpleDamage(float Damage)
    {
        _LimbHealth -= Damage;
        Damage *= _LimbDamageReduction;
 
        _HPComp.SimpleDamage(Damage);
    }

    void IDamageable.TakeDamage(DamageScore DamageDT)
    {
        _LimbHealth -= DamageDT.DamageAmount;
        DamageDT.DamageAmount *= _LimbDamageReduction;

        _HPComp.TakeDamage(DamageDT);
    }

    void IDamageable.TakeDamageWithKnockback(Vector3 KnockbackDir, float KnockbackForce, DamageScore DamageDT)
    {
        print("damaged limb: " + this.name);

        _LimbHealth -= DamageDT.DamageAmount;
        DamageDT.DamageAmount *= _LimbDamageReduction;

        _KnockbackDir = KnockbackDir.normalized;
        _KnockbackForce = KnockbackForce;


        _HPComp.TakeDamageWithKnockback(KnockbackDir, KnockbackForce, DamageDT);
    }

    public bool GetHealthComponent(out Health_Component HPComp)
    {

        if(_HPComp == null)
        {
            HPComp = null;
            return false;
        }
        HPComp = _HPComp;
        return true;
    }
}
