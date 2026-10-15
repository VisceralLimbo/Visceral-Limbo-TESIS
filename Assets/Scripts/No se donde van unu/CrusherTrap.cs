
using System.Collections.Generic;
using UnityEngine;

public class CrusherTrap : MonoBehaviour
{
    private enum CrusherState
    {
        WaitingAtTop,
        MovingDown,
        WaitingAtBottom,
        MovingUp
    }

    [Header("Movement")]
    [SerializeField, Min(0f)] private float dropDistance = 4f;
    [SerializeField, Min(0.01f)] private float moveDownSpeed = 5f;
    [SerializeField, Min(0.01f)] private float moveUpSpeed = 4f;
    [SerializeField, Min(0f)] private float waitAtTop = 2f;
    [SerializeField, Min(0f)] private float waitAtBottom = 0.7f;

    [Header("Damage")]
    [SerializeField, Min(0f)] private float damage = 20f;
    [SerializeField, Min(0.01f)] private float damageInterval = 1f;
    [SerializeField] private PlayerContext playerContext;

    // es bastante parecido al spike en todo aspecto

    private static readonly Dictionary<Health_Component, float>
        lastDamageTime = new Dictionary<Health_Component, float>();

    private static float nextClearTime;
    private const float ClearInterval = 5f;

    private readonly HashSet<Collider> occupants = new HashSet<Collider>();

    private Vector3 topPosition;
    private Vector3 bottomPosition;

    private CrusherState currentState;
    private float stateTimer;
    private bool initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ClearCache()
    {
        lastDamageTime.Clear();
        nextClearTime = 0f;
    }

    private void Start()
    {
        // guardo pos inicial
        topPosition = transform.position;
        bottomPosition = topPosition + Vector3.down * dropDistance;

        currentState = CrusherState.WaitingAtTop;
        stateTimer = waitAtTop;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
            return;

        HandleMovement();
        HandleDamage();
        ClearStaticDictionary();
    }

    private void HandleMovement()
    {
        switch (currentState)
        {
            case CrusherState.WaitingAtTop:
                stateTimer -= Time.deltaTime;

                if (stateTimer <= 0f)
                    currentState = CrusherState.MovingDown;

                break;

            case CrusherState.MovingDown:
                MoveTowardsPosition(bottomPosition, moveDownSpeed);

                if (transform.position == bottomPosition)
                {
                    currentState = CrusherState.WaitingAtBottom;
                    stateTimer = waitAtBottom;
                }

                break;

            case CrusherState.WaitingAtBottom:
                stateTimer -= Time.deltaTime;

                if (stateTimer <= 0f)
                    currentState = CrusherState.MovingUp;

                break;

            case CrusherState.MovingUp:
                MoveTowardsPosition(topPosition, moveUpSpeed);

                if (transform.position == topPosition)
                {
                    currentState = CrusherState.WaitingAtTop;
                    stateTimer = waitAtTop;
                }

                break;
        }
    }

    private void MoveTowardsPosition(Vector3 target, float speed)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );
    }

    private void HandleDamage()
    {
        // aplico daño cuando baja y emientras esta abajo
        if (currentState != CrusherState.MovingDown &&
            currentState != CrusherState.WaitingAtBottom)
        {
            return;
        }

        occupants.RemoveWhere(col => col == null);

        DamageScore damageScore = new DamageScore()
        {
            Attacker = playerContext,
            DamageAmount = damage,
            FactionID = FactionID.LimboTrap
        };

        foreach (Collider col in occupants)
        {
            bool canTakeDamage =
                col.TryGetComponent(out IDamageable _) ||
                col.TryGetComponent(out Health_Component _);

            if (!canTakeDamage)
                continue;

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

    private void OnCollisionEnter(Collision collision)
    {
        occupants.Add(collision.collider);
    }

    private void OnCollisionStay(Collision collision)
    {
        occupants.Add(collision.collider);
    }

    private void OnCollisionExit(Collision collision)
    {
        occupants.Remove(collision.collider);
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
}
