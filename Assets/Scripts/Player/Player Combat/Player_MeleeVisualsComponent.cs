using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_MeleeVisualsComponent : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] private bool _IsBleedActive;
    public bool IsBleedActive { get { return _IsBleedActive; } }
    [SerializeField] private bool _IsBoostActive;
    public bool IsBoostActive { get { return _IsBoostActive; } }


    [Header("Mesh & Materials")]
    [SerializeField] private MeshRenderer _SwordMeshRenderer;
    [SerializeField] private Material _NormalMaterial;
    [SerializeField] private Material _BoostMaterialGold;

    [Header("Shader Properties")]
    [SerializeField] private Color _BleedColor = new Color(1f, 0.1f, 0.1f, 1f);
    [SerializeField] private float _BleedBlendValue = 0.04f;

    [Header("Trails")]
    [SerializeField] private List<TrailRenderer> _swordTrails;
    [SerializeField] private List<TrailRenderer> _bleedSwordTrails;

    [Header("Sounds")]
    [SerializeField] SoundData swingSound; // Movido aquí

    // Estados visuales (Ahora controlados por un BuffManager externo idealmente, pero mantenidos aquí por simpleza)
    private bool _isBleedEffectActive;
    private int _activeBoostSources;

    private void OnEnable()
    {
        // Nos suscribimos a la lógica
        PlayerEvents.OnStartAttacking += HandleAttackVisuals;
        PlayerEvents.OnEndAttacking   += StopAllTrails;
    }

    private void OnDisable()
    {
        PlayerEvents.OnStartAttacking -= HandleAttackVisuals;
        PlayerEvents.OnEndAttacking   -= StopAllTrails;
    }

    private void Start()
    {
        StopAllTrails();
    }

    private void HandleAttackVisuals()
    {
        // 1. Trails
        List<TrailRenderer> activeTrails = _isBleedEffectActive ? _bleedSwordTrails : _swordTrails;
        foreach (var trail in activeTrails)
        {
            if (trail != null) trail.emitting = true;
        }

        // 2. Sonido
        if (swingSound != null)
        {
            SoundManager.Instance.CreateSound()
                .WithSoundData(swingSound)
                .WithPosition(transform.position)
                .WithRandomPitch(true)
                .play();
        }
    }

    private void StopAllTrails()
    {
        foreach (var trail in _swordTrails) { if (trail != null) { trail.emitting = false; trail.Clear(); } }
        foreach (var trail in _bleedSwordTrails) { if (trail != null) { trail.emitting = false; trail.Clear(); } }
    }

    // --- MÉTODOS PARA ACTUALIZAR EL SHADER Y MATERIAL ---
    // (Cualquier script que maneje el dash o el status de sangrado ahora debe llamar a estas funciones de ACÁ, no del script de ataque)

    public void SetBleedVisuals(bool isActive)
    {
        _isBleedEffectActive = isActive;
        UpdateMaterial();
    }

    public void AddBoostVisualSource()
    {
        _activeBoostSources++;
        UpdateMaterial();
    }

    public void RemoveBoostVisualSource()
    {
        _activeBoostSources = Mathf.Max(0, _activeBoostSources - 1);
        UpdateMaterial();
    }

    private void UpdateMaterial()
    {
        if (_SwordMeshRenderer == null) return;

        if (_activeBoostSources > 0)
        {
            _SwordMeshRenderer.material = _BoostMaterialGold;
        }
        else if (_isBleedEffectActive)
        {
            _SwordMeshRenderer.material = _NormalMaterial;
            Material instancedMat = _SwordMeshRenderer.material; // Esto crea una instancia del material para no modificar el asset base

            if (instancedMat.HasProperty("_FresnelGradientBlend"))
                instancedMat.SetFloat("_FresnelGradientBlend", _BleedBlendValue);

            if (instancedMat.HasProperty("_Color"))
                instancedMat.SetColor("_Color", _BleedColor);
        }
        else
        {
            _SwordMeshRenderer.material = _NormalMaterial;
        }
    }




}
