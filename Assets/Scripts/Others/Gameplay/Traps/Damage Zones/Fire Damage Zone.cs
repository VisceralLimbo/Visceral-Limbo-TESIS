using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class FireDamageZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] ParticleSystem[] _Particles;
    [SerializeField] GameObject _Prefab;
    public GameObject Prefab { get { return _Prefab; } }
    [SerializeField] VisualEffect[] _VisualEffects;
    [SerializeField] PlayerContext _Context;

    [Header("Pool Object - Opcional")]
    [SerializeField] PoolObjectComponent _PoolComponent;
    bool IsPooled;

    [Header("Variables")]
    [Tooltip("duracion del area de fuego, dejar en -1 o menos para ignorar duracion")]
    [SerializeField] float _Duration;

    [SerializeField] float _Damage;
    [SerializeField] float _DamageInterval;
    [SerializeField] float _FireRadius;
    [Tooltip("El centro del colider de daño")]
    [SerializeField] Transform _DamageOriginPoint;
    [SerializeField] LayerMask _Mask;
    [SerializeField] bool _DebugDraw;
    Collider[] Hits = new Collider[100];
    


    HashSet<Health_Component> DamagedComponents = new HashSet<Health_Component>();

    private void Awake()
    {
        if (_PoolComponent == null) IsPooled = false;
    }

    public void Initialize(PlayerContext Context)
    {
        _Context = Context;
    }

    private void Start()
    {
        if (_PoolComponent == null)
        {
            if(this.TryGetComponent(out PoolObjectComponent PoolComp))
            {
                _PoolComponent = PoolComp;
                IsPooled = true;
            }
            else
            {
                IsPooled = false;
            }
        }
        else
        {
            IsPooled = true;
        }
        if(_Particles.Length <= 0)
        {
            _Particles = GetComponentsInChildren<ParticleSystem>();
        }

        if(_VisualEffects.Length <= 0)
        {
            _VisualEffects = GetComponentsInChildren<VisualEffect>();
        }

        ShowVisuals();
    }

    private void ShowVisuals()
    {
        if(_Particles.Length > 0)
        {
            foreach (ParticleSystem particle in _Particles)
            {
                particle.Play();
            }
        }
    
        if(_VisualEffects.Length > 0)
        {
            foreach(var visual in _VisualEffects)
            {
                visual.Play();
            }
        }
    }

    float _DamagePulse;
    private void Update()
    {
        if(_Duration > -1)
        {
            UpdateLifetime();
        }

        if(_DamagePulse < _DamageInterval)
        {
            _DamagePulse += Time.deltaTime * TimeDilationManager.GlobalTimeScale;
            return;
        }
        else
        {
            _DamagePulse = 0;
            DamagedComponents.Clear();
            CheckColisions();

        }


    }


    float _LifetimePulse;
    private void UpdateLifetime()
    {
        if(_LifetimePulse < _Duration)
        {
            _LifetimePulse += Time.deltaTime * TimeDilationManager.GlobalTimeScale;
            return;
        }
        else
        {
            if (IsPooled && _PoolComponent != null)
            {
                _PoolComponent.ReleaseToPool();
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
    }

    private void CheckColisions()
    {
        int NumberOfHits = Physics.OverlapSphereNonAlloc(_DamageOriginPoint.position, _FireRadius, Hits, _Mask);

        if(NumberOfHits > 0)
        {
            for(int i = 0; i < NumberOfHits; i++)
            {
                if (Hits[i].TryGetComponent(out IDamageable IDamage))
                {
                    IDamage.GetHealthComponent(out Health_Component HPComp);

                    if(IDamage.GetPlayerContext(out PlayerContext TargetContext))
                    {
                        if(_Context != null && _Context != TargetContext)
                        {   
                            if (DamagedComponents.Add(HPComp))
                            {
                                ProcessDamage(HPComp, IDamage);
                            }
                        }
                    }

                }
            }
        }
   
    }

    private void ProcessDamage(Health_Component HPComp, IDamageable IDamage)
    {
        if (HPComp == null || IDamage == null) return;
        if(_Context == null)
        {
            IDamage.SimpleDamage(_Damage);
            return;
        }

        DamageScore DMS = new DamageScore
        {
            Attacker = _Context,
            DamageAmount = _Damage,
            ElementalDamage = ElementType.Fire,
            FactionID = _Context.faction, 
        };

        IDamage.TakeDamage(DMS);
    }

    private void OnEnable()
    {
        _LifetimePulse = 0;
        DamagedComponents.Clear();
        _DamagePulse = 0;

        ShowVisuals();
    }


    private void OnDrawGizmos()
    {
        if (!_DebugDraw)
        {
            return;
        }
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(_DamageOriginPoint.position, _FireRadius);
    }

}
