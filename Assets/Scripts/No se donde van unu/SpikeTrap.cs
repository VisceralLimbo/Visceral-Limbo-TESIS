using System;
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

    [Header("player")]
    [SerializeField] private PlayerContext playerContext; // el contexto del player

    [SerializeField] private List<Transform> spikes = new List<Transform>();

    private Vector3[] startPositions;

    // lista de colliders dentro del pincho
    private HashSet<Collider> occupants = new HashSet<Collider>();
    //diccionario que respeta cada nextdamage de cada collider que entra, ahora se conectan entre si para que cuando juntemos las trampas no se haga daño haciendo adadad
    private static Dictionary<Health_Component, float> lastDamageTime = new Dictionary<Health_Component, float>();

    private void Start()
    {
        InitializeSpikes();
    }

    private void Update()
    {
        HandleMovement();

        // Si están bajo tierra o no hay nadie, no procesamos daño
        if (spikes[0].localPosition.y <= startPositions[0].y || occupants.Count == 0) return;

        occupants.RemoveWhere(x => x == null);

        foreach (Collider Col in occupants)
        {
            if(Col.TryGetComponent(out IDamageable Dmg))
            {
                DamageScore DMS = new DamageScore()
                {
                    Attacker = this.playerContext,
                    DamageAmount = damage,
                    FactionID = FactionID.LimboTrap,
                };

                // Delegamos al dispatcher. el se encarga de checkear el diccionario y el cooldown.
                DamageDispatcher.ProcessContinuousHit(Col, ref DMS, null, 0, lastDamageTime, damageInterval, false);
            }
            else if (Col.TryGetComponent(out Health_Component HPComp))
            {
                DamageScore DMS = new DamageScore()
                {
                    Attacker = this.playerContext,
                    DamageAmount = damage,
                    FactionID = FactionID.LimboTrap,
                };

                // Delegamos al dispatcher. el se encarga de checkear el diccionario y el cooldown.
                DamageDispatcher.ProcessContinuousHit(Col, ref DMS, null, 0, lastDamageTime, damageInterval, false);
            }
        }

        ClearStaticDictionary();
    }

    float clearTime = 5f, ClearPulse;

    /// <summary>
    /// Prevencion de memory leak por uso de diccionario static.
    /// </summary>
    private void ClearStaticDictionary()
    {
        if(clearTime > ClearPulse)
        {
            ClearPulse += Time.deltaTime * TimeDilationManager.GlobalTimeScale;
            return;
        }

        List<Health_Component> DeadComps = new List<Health_Component>(lastDamageTime.Keys.Count);

        foreach(var HPComp in lastDamageTime.Keys)
        {
            if(HPComp == null)
            {
                DeadComps.Add(HPComp);
            }
        }

        foreach(var HPComp in DeadComps)
        {
            lastDamageTime.Remove(HPComp);
        }

    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private void ClearCache()
    {
        if(lastDamageTime != null)
        {
            lastDamageTime.Clear();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        occupants.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (occupants.Contains(other))
        {
            occupants.Remove(other);
        }
    }
    private void InitializeSpikes()
    {
        if (spikes.Count == 0) return;

        // array del tamaño de los pinches
        startPositions = new Vector3[spikes.Count];

        for (int i = 0; i < spikes.Count; i++)
        {
            if (spikes[i] != null)
            {
                startPositions[i] = spikes[i].localPosition;
            }
        }
    }


    private void HandleMovement()
    {
        if (spikes.Count == 0 || startPositions == null || startPositions.Length != spikes.Count) return;

        float offsetY = Mathf.Sin(Time.time * moveSpeed) * moveHeight;

        for (int i = 0; i < spikes.Count; i++)
        {
            if (spikes[i] == null) continue;
            spikes[i].localPosition = new Vector3(startPositions[i].x, startPositions[i].y + offsetY, startPositions[i].z);
        }
    }
}
