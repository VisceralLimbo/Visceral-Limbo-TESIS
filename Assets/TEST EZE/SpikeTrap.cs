using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("dmg")]
    [SerializeField] private float damage = 10f;         // dmg de la trampa
    [SerializeField] private float damageInterval = 1f;  // cd del dmg

    [Header("up and down")]
    [SerializeField] private float moveHeight = 1f;   // cuanto suben y bajan
    [SerializeField] private float moveSpeed = 1f;      // velocidad del movimiento
    [SerializeField] private bool freezeMovement = false; // booleano para congelarlos (a modo de test, lo activan en el inspector)

    [Header("player")]
    [SerializeField] private PlayerContext playerContext; // el contexto del player

    [SerializeField] private List<Transform> spikes;

    private List<Vector3> startPositions = new List<Vector3>();
    //diccionario que respeta cada nextdamage de cada collider que entra, ahora se conectan entre si para que cuando juntemos las trampas no se haga daño haciendo adadad
    private static Dictionary<Health_Component, float> lastDamageTime = new Dictionary<Health_Component, float>();

    private void Start()
    {
        // guardo la pos de cada pincho
        foreach (Transform spike in spikes)
        {
            if (spike != null) startPositions.Add(spike.localPosition);
        }
    }

    private void Update()
    {
        if (freezeMovement || spikes.Count == 0) return; // bool on se congelan donde estan 

        // calculo el movimiento
        float offsetY = Mathf.Sin(Time.time * moveSpeed) * moveHeight;

        for (int i = 0; i < spikes.Count; i++)
        {
            if (spikes[i] == null) continue;
            // muevo cada pincho
            spikes[i].localPosition = new Vector3(startPositions[i].x, startPositions[i].y + offsetY, startPositions[i].z);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // se hace daño si los pinchos cambiaron la pos osea etnan por encima
        if (spikes.Count > 0 && spikes[0].localPosition.y <= startPositions[0].y) return;

        if (other.TryGetComponent(out Health_Component HPComp))
        {
            // todas preguntan por el ultimo tick de daño para q no se overlapeen entre si
            if (lastDamageTime.TryGetValue(HPComp, out float lastTime))
            {
                if (Time.time < lastTime + damageInterval) return;
            }

            ApplyTrapDamage(HPComp);
            lastDamageTime[HPComp] = Time.time;
        }
    }

    private void ApplyTrapDamage(Health_Component HPComp)
    {
        PlayerContext victimCtx = HPComp.Context;
        if (victimCtx != null)
        {
            DamageScore dmg = new DamageScore();
            dmg.Attacker = playerContext;
            dmg.Victim = victimCtx;
            dmg.DamageAmount = damage;
            dmg.ElementalDamage = ElementType.Physical;
            dmg.FactionID = FactionID.LimboTrap;

            HPComp.TakeDamageWithKnockback(Vector3.zero, 0, dmg);
        }
        else
        {
            HPComp.SimpleDamage(damage);
        }
    }
}
