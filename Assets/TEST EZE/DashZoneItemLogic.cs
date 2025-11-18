using UnityEngine;
using System;

public class DashZoneItemLogic : ItemLogic
{
    // refe al dash para suscribirme al evento nuevo
    private Dash_Skill _playerDashSkill;

    // prefab de la zona de daño
    [SerializeField] private GameObject _DamageZonePrefab;

    [Header("config")]
    // config de las cosas
    [SerializeField] private float _ZoneDuration = 2f; // tiempo
    [SerializeField] private float _DamagePerTick = 10f; // daño
    [SerializeField] private float _DamageTickRate = 0.5f; // cada cuanto tiempo recibe el daño

    [Header("rastro")]
    [SerializeField] private float _TraceSpacing = 1f; // el espacio entre los rastros
    [SerializeField] private float _DashDistanceFactor = 1f; // aseguro q siga la distancia del dash

    [SerializeField] SoundData _PickUpSound;

    private void OnTriggerEnter(Collider other)
    {
        if (IsInventoryMaster) return;

        if (other.CompareTag("Player"))
        {
            if (other.transform.parent.TryGetComponent(out PlayerContext context))
            {
                _Context = context;
                Inventory = context.Inventory;
                OnPickUp();
            }
        }
    }

    private void GetReferences()
    {
        if (_Context == null) return;

        if (_playerDashSkill == null)
        {
            _playerDashSkill = _Context.GetComponentInChildren<Dash_Skill>();
        }
    }

    public override void Register(InventoryManager inventory, PlayerContext context)
    {
        base.Register(inventory, context);
        GetReferences();

        if (_playerDashSkill != null)
        {
            // me suscribo al nuevo evento
            _playerDashSkill.OnDashTraced += PlaceDamageZone;
        }
    }

    public override void Unregister()
    {
        if (_playerDashSkill != null)
        {
            // me desuscribo :V
            _playerDashSkill.OnDashTraced -= PlaceDamageZone;
        }
        base.Unregister();
    }

    // se llama siempre q se haga un dash
    private void PlaceDamageZone(Vector3 finalDashVector)
    {
        // valor actualizado del stat
        float finalDamage = _DamagePerTick; // fallback

        if (_Context != null && _Context.Stats != null)
        {
            finalDamage = _Context.Stats.GetFloatStatValue("DashDamageZone");
            if (finalDamage < 0) finalDamage = _DamagePerTick;
        }

        // pos
        Vector3 startPosition = _Context.PlayerTransform.position;

        // longitud
        float rawDashDistance = finalDashVector.magnitude;

        // el factor para reducir el rnago 
        float totalDistance = rawDashDistance * _DashDistanceFactor;

        // norm
        Vector3 traceDirection = finalDashVector.normalized;

        // calculo los puntos gracias gemini
        int numPoints = Mathf.CeilToInt(totalDistance / Mathf.Max(0.01f, _TraceSpacing));

        for (int i = 0; i < numPoints; i++)
        {
            // lo q llevo recorrido
            float currentDistance = i * _TraceSpacing;

            // si me pase de largo break
            if (currentDistance > totalDistance) break;

            // calculo pos
            Vector3 spawnPosition = startPosition + (traceDirection * currentDistance);

            // instancio
            GameObject damageZone = Instantiate(_DamageZonePrefab, spawnPosition, Quaternion.identity);

            if (damageZone.TryGetComponent(out DamageZone zoneScript))
            {
                zoneScript.SetupZone(_Context, finalDamage, _DamageTickRate, _ZoneDuration);
            }
        }
    }

    public override void OnPickUp()
    {
        Inventory.AddItemStack(_ItemDefinition);
        SoundManager.Instance.CreateSound().WithSoundData(_PickUpSound).WithPosition(this.transform.position).play();
        Destroy(this.gameObject);
    }

    public override void AddStack() 
    {

        if (ItemStacks == 0)
        {
            var statManager = _Context.Stats;

            StatModifierFloat statMod = new StatModifierFloat // stat modificador de daño base de la zona
            {
                ModifierValueFloat = _DamagePerTick,
                ModType = ModifierType.flat,
                EffectName = "DashDamageLogic",
                Source = this
            };
            statManager.UpdateFloatStatValue("DashDamageZone", statMod);
            ItemStacks++;
        }
        else if (ItemStacks >= 1)
        {
            ItemStacks++;
            var statManager = _Context.Stats;
            StatModifierFloat statMod = new StatModifierFloat
            {
                ModifierValueFloat = _DamagePerTick * ItemStacks,
                ModType = ModifierType.flat,
                EffectName = "DashDamageLogic",
                Source = this
            };
            statManager.UpdateFloatStatValue("DashDamageZone", statMod);

            //quiza podria tambien mejorar el tiempo agarrar un stack pero idk
        }

    }
    public override void RemoveStack() { }
    public override void OnDrop() { }
}
