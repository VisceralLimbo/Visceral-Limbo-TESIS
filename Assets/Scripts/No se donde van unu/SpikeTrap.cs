
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("dmg")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float damageInterval = 1f;

    [Header("up and down")]
    [SerializeField] private float moveHeight = 1f;
    [SerializeField] private float moveSpeed = 1f;

    [Header("player")]
    [SerializeField] private PlayerContext playerContext;

    [SerializeField] private List<Transform> spikes = new List<Transform>();

    private Vector3[] startPositions;

    // colliders de la trampa
    private readonly HashSet<Collider> occupants = new HashSet<Collider>();

    // cooldwon compartido
    private static readonly Dictionary<Health_Component, float>
        lastDamageTime = new Dictionary<Health_Component, float>();

    // control para la limpieza
    private const float ClearInterval = 5f;
    private static float nextClearTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ClearCache()
    {
        lastDamageTime.Clear();
        nextClearTime = 0f;
    }

    private void Start()
    {
        InitializeSpikes();
    }

    private void Update()
    {
        HandleMovement();

        // limpio referencias
        occupants.RemoveWhere(x => x == null);

        // global no por cada trampa
        ClearStaticDictionary();

        // seteo
        if (spikes.Count == 0 ||
            startPositions == null ||
            startPositions.Length != spikes.Count ||
            spikes[0] == null ||
            occupants.Count == 0)
        {
            return;
        }

        // solo hace daño cuando esta arriba
        if (spikes[0].localPosition.y <= startPositions[0].y)
            return;

        DamageScore damageScore = new DamageScore()
        {
            Attacker = playerContext,
            DamageAmount = damage,
            FactionID = FactionID.LimboTrap
        };

        foreach (Collider col in occupants)
        {
            // chequeo
            bool canTakeDamage =
                col.TryGetComponent(out IDamageable _) ||
                col.TryGetComponent(out Health_Component _);

            if (!canTakeDamage)
                continue;

            // dispatcher controla el cd y la aplicacion de dmg
            DamageDispatcher.ProcessContinuousHit(
                col,
                ref damageScore,
                null,
                0,
                lastDamageTime,
                damageInterval,
                false
            );
        }
    }

    private void ClearStaticDictionary()
    {
        if (Time.unscaledTime < nextClearTime)
            return;

        nextClearTime = Time.unscaledTime + ClearInterval;

        List<Health_Component> deadComps = new List<Health_Component>();

        foreach (Health_Component hpComp in lastDamageTime.Keys)
        {
            if (hpComp == null)
                deadComps.Add(hpComp);
        }

        foreach (Health_Component hpComp in deadComps)
        {
            lastDamageTime.Remove(hpComp);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        occupants.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        occupants.Remove(other);
    }

    private void InitializeSpikes()
    {
        startPositions = new Vector3[spikes.Count];

        for (int i = 0; i < spikes.Count; i++)
        {
            if (spikes[i] != null)
                startPositions[i] = spikes[i].localPosition;
        }
    }

    private void HandleMovement()
    {
        if (spikes.Count == 0 ||
            startPositions == null ||
            startPositions.Length != spikes.Count)
        {
            return;
        }

        float offsetY = Mathf.Sin(Time.time * moveSpeed) * moveHeight;

        for (int i = 0; i < spikes.Count; i++)
        {
            if (spikes[i] == null)
                continue;

            spikes[i].localPosition = new Vector3(
                startPositions[i].x,
                startPositions[i].y + offsetY,
                startPositions[i].z
            );
        }
    }
}
