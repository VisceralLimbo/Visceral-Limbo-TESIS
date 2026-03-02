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

    [Tooltip("Modificador de daño de la extremidad, reduce el daño entrante, NOTA: POSIBLEMENTE SERÁ CONECTADO AL HEALTHComponent")]
    [SerializeField] float _LimbDamageReduction;

    [Tooltip("El CharacterJoint del Limb")]
    [SerializeField] CharacterJoint _joint;

    [Tooltip("El rigidbody del Limb")]
    [SerializeField] Rigidbody _Rb;

    [Tooltip("El Collider del Limb")]
    [SerializeField] Collider _Col;


    private void Start()
    {
        if (_joint == null) _joint = GetComponent<CharacterJoint>();
        if (_Rb == null) _Rb = GetComponent<Rigidbody>();
        if(_Col == null) _Col= GetComponent<Collider>();
    }
    public void DismemberLimb()
    {
        if (_LimbHealth > 0) return;

        // 1. Destruir las uniones (CharacterJoint) para que el hueso quede libre
        if (_joint != null)
        {
            Destroy(_joint);
        }

        // 2. Destruir el Rigidbody para quitarle la masa y la gravedad
        if (_Rb != null)
        {
            Destroy(_Rb);
        }

        // 3. Destruir el Collider para que no choque con otras partes del cuerpo al encogerse
     
        if (_Col != null)
        {
            Destroy(_Col);
        }

        // 4. Aplicar el "Cero Seguro" (prácticamente invisible pero matemáticamente estable)
        this.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        GameObject Gibbedlimb =Instantiate
            (_LimbDismemberPrefab,this.transform.position,this.transform.rotation);

       
    }




    void IDamageable.SimpleDamage(float Damage)
    {
        throw new System.NotImplementedException();
    }

    void IDamageable.TakeDamage(DamageScore DamageDT)
    {
        throw new System.NotImplementedException();
    }

    void IDamageable.TakeDamageWithKnockback(Vector3 KnockbackDir, float KnockbackForce, DamageScore DamageDT)
    {
        throw new System.NotImplementedException();
    }
}
