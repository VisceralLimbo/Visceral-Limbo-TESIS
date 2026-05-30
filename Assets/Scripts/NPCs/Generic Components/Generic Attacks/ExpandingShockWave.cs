using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ShockwaveConfig
{
    public enum WaveDamageCheck
    {
        GroundDamageOnly,
        AirDamageOnly,
        AllDamage
    }

    [Header("Variables de configuracion")]
    [Tooltip("El tamaño del anillo a saltar, determina que tan preciso tiene que ser el salto")]
    public float _WaveThickness;
    public LayerMask _DamageMask;
    public WaveDamageCheck _DamageCheck;

    [Tooltip("La velocidad de expansion del anillo")]
    public float _ExpansionSpeed;

    [Tooltip("El radio máximo del anillo")]
    public float _MaxRadius;
    public float _Damage;
    public float _Knockback;

    [Tooltip("Cada cuanto tiempo puedo volver a hacer daño a objetivos ya dañados, menor a -1 es igual a nunca resetear")]
    public float _DamageResetTimer;

    [Tooltip("El poder de knockback en Y exclusivo")]
    public float _KnockbackYStrenght;

    [Tooltip("Visual: Si el elemento visual escala solo en X,Z o en todos los ejes con el tiempo")]
    public bool _ExpandOnY;
    public GameObject _VisualElement;
}


public class ExpandingShockWave : MonoBehaviour
{
    [Header("Configuration & Variables")]
    [SerializeField] ShockwaveConfig _Config;
    [SerializeField] PlayerContext _OwnerContext;
    [SerializeField] GameObject _Visuals;

    [SerializeField] private float _CurrentRadius;
    [Space]
    [Header("Debug")]
    [SerializeField] bool DrawGizmos;
    

    HashSet<Health_Component> IDamagedEntities = new HashSet<Health_Component>();
    HashSet<Collider> IDamagedColliders = new HashSet<Collider>();

    Collider[] Cols = new Collider[200];

    public void Initialize(ShockwaveConfig Config, PlayerContext OwnerContext, Vector3 StartingPosition)
    {
        _Config = Config;
        _OwnerContext = OwnerContext;
        this.transform.position = StartingPosition;
    }

    public void PerformShockwave(Vector3 StartingPosition)
    {
        print("Performing Shockwave");
        _CurrentRadius = 0f;

        this.transform.position = StartingPosition;
        this.enabled = true;

        if(_Visuals != null)
        {
            _Visuals?.SetActive(true);
        }
    }

    public void Start()
    {
        if(_Config._VisualElement != null && _Visuals == null)
        {
            _Visuals = Instantiate(_Config._VisualElement,this.transform);
        }

    }

    private void Update()
    {
        // step 1) aumentar radio del Wave
        _CurrentRadius += _Config._ExpansionSpeed * (Time.deltaTime * TimeDilationManager.GlobalTimeScale);

        // step 2) escalamos el objeto, por temas de visuales, algunos efectos visuales NO quieren escalar en Y.
        if (_Config._ExpandOnY)
        {
            transform.localScale = Vector3.one * (_CurrentRadius * 2f);
        }
        else
        {
            transform.localScale = new Vector3(_CurrentRadius * 2f, 1f, _CurrentRadius * 2f);
        }

        // step 3) calculamos si impactamos contra algo
        int Hits = Physics.OverlapSphereNonAlloc(this.transform.position, _CurrentRadius, Cols, _Config._DamageMask);

        if(_OwnerContext == null)
        {
            Debug.LogError("[Visceral Error] ExpandingShockWave: Null Owner Context!");
            return;
        }

        // por cada hit reportado
        for(int i = 0; i < Hits; i++)
        {
            // step 4) agarramos el Collider correspondiente

            Collider Hit = Cols[i];

            print(Hit.name);

            // tratamos de obtener el Idamageable
            if (Hit.TryGetComponent(out IDamageable IDamage))
            {
                // si el Context == owner -> continue
                if (IDamage.GetPlayerContext(out PlayerContext Context))
                {
                    if(Context != null && Context == _OwnerContext) 
                    {
                        continue;
                    }
                }

                // else, procesamos el hit
                if(IDamage.GetHealthComponent(out Health_Component HPComp) && HPComp != null && !IDamagedEntities.Contains(HPComp)) 
                {
                    print("Hit.name " + Context + " " + HPComp);
                    ProcessHit(Hit, IDamage, Context, HPComp);
                }
            }
        }

        if(_CurrentRadius > _Config._MaxRadius)
        {
            this.enabled = false;
            if(_Visuals != null)
            {
                _Visuals?.SetActive(false);
            }
        }

        if(_Config._DamageResetTimer <= -1)
        {
            return;
        }
        ResetDamageTimer();

    }

    float _DamagePulse = 0;
    private void ResetDamageTimer()
    {
        if(_DamagePulse <= _Config._DamageResetTimer)
        {
            _DamagePulse += Time.deltaTime * TimeDilationManager.GlobalTimeScale;
        }
        else
        {
            IDamagedEntities.Clear();
            IDamagedColliders.Clear();
            _DamagePulse = 0;
        }

    }

    private void ProcessHit(Collider Hit, IDamageable IDamage, PlayerContext Context, Health_Component HPComp)
    {

        // calculamos la distancia al target desde nuestro centro
        float distanceToTarget = Vector3.Distance(transform.position, Hit.ClosestPoint(transform.position));

        // si la distancia al hit es menor a Radio - Thickness -> esquivamos el ataque
        if (distanceToTarget < _CurrentRadius - _Config._WaveThickness) return;



        // switch deteccion de danio dependiendo de enum
        switch (_Config._DamageCheck)
        {
            case ShockwaveConfig.WaveDamageCheck.GroundDamageOnly:
                if (Context == null || Context.KCCMotor.GroundingStatus.IsStableOnGround)
                {
                    DealDamage(IDamage,HPComp);
                }
                break;
            case ShockwaveConfig.WaveDamageCheck.AirDamageOnly:

                if (Context == null || !Context.KCCMotor.GroundingStatus.IsStableOnGround)
                {
                    DealDamage(IDamage, HPComp);
                }

                break;
            case ShockwaveConfig.WaveDamageCheck.AllDamage:
                DealDamage(IDamage, HPComp);

                break;
        }

    }

    private void DealDamage(IDamageable IDamage, Health_Component HPCOMP)
    {
        // Failsafe por si acaso
        if (HPCOMP == null) return;

        IDamagedEntities.Add(HPCOMP);

        DamageScore DMS = new DamageScore()
        {
            Attacker = _OwnerContext,
            DamageAmount = _Config._Damage,

            // Prevenimos error si la onda la genero una trampa de mapa (sin owner)
            FactionID = _OwnerContext != null ? _OwnerContext.faction : FactionID.LimboMonster1
        };

        
        Vector3 pushDir = HPCOMP.transform.position - this.transform.position;
        pushDir.y = 0;
        pushDir.Normalize();
        pushDir.y = _Config._KnockbackYStrenght;

        // Si existe la interfaz la usamos, sino usamos el componente directo
        if (IDamage != null)
        {
            IDamage.TakeDamageWithKnockback(pushDir, _Config._Knockback, DMS);
        }
        else
        {
            HPCOMP.TakeDamageWithKnockback(pushDir, _Config._Knockback, DMS);
        }



    }

    private void OnDrawGizmos()
    {
        if (!DrawGizmos) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _CurrentRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Mathf.Max(0, _CurrentRadius - _Config._WaveThickness));
    }
}
