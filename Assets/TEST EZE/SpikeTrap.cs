using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("dmg")]
    [SerializeField] private float damage = 10f;         // dmg de la trampa
    [SerializeField] private float damageInterval = 1f;  // cd del dmg

    [Header("sube y baja")]
    [SerializeField] private float moveHeight = 1f;   // cuanto suben y bajan
    [SerializeField] private float moveSpeed = 1f;      // velocidad del movimiento
    [SerializeField] private bool freezeMovement = false; // booleano para congelarlos (a modo de test, lo activan en el inspector)

    [Header("player")]
    [SerializeField] private PlayerContext playerContext; // el contexto del player

    private Vector3 startPos;
    //diccionario que respeta cada nextdamage de cada collider que entra
    private Dictionary<Health_Component, float> lastDamageTime = new Dictionary<Health_Component, float>();

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        if (freezeMovement) return; // bool on se congelan donde estan 

        // movimiento arriba y abajo
        float newY = startPos.y + Mathf.Sin(Time.time * moveSpeed) * moveHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    private void OnTriggerStay(Collider other)
    {
        // solo hacen dmg si estan arriba
        if (transform.position.y <= startPos.y) return;

        if (other.TryGetComponent(out Health_Component HPComp))
        {
            // chequeo cooldown individual
            float lastTime = 0f;
            lastDamageTime.TryGetValue(HPComp, out lastTime);

            if (Time.time < lastTime + damageInterval) return;

            PlayerContext victimCtx = HPComp.Context;
            if (victimCtx != null)
            {
                // ctrl c ctrl v de la trampa de fuego o barril ahrw
                DamageScore dmg = new DamageScore();
                dmg.Attacker = playerContext; // el playercontext
                dmg.Victim = victimCtx;
                dmg.DamageAmount = damage;
                dmg.ElementalDamage = ElementType.Physical; //supongo physical xd
                dmg.FactionID = FactionID.LimboTrap;

                // hago dmg con el knonckback y de el score
                HPComp.TakeDamageWithKnockback(Vector3.zero, 0, dmg);
            }
            else
            {
                HPComp.SimpleDamage(damage);
            }

            // se actualiza individualmente
            lastDamageTime[HPComp] = Time.time;
        }
    }
}
