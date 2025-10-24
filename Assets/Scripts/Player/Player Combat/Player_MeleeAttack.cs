using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Player_MeleeAttack : Visceral_Script
{
    [Header("References")]
    [SerializeField] AnimatorHandler _AnimHandler;
    [SerializeField] Visceral_WeaponBase _Weapon;

    private Animator _Anim;
    [Space]

    [Header("Sounds")]
    [SerializeField] SoundData[] soundData;

    [Space]
    [Header("Attack Animations")]
    public SwordTestSO[] Left_SwordAttacks; // las animaciones que queremos que corran cuando nos movemos a la izquierda
    public SwordTestSO[] Right_SwordAttacks;// las animaciones que queremos que corran cuando nos movemos a la derecha
    public SwordTestSO[] UP_SwordAttacks; // las animaciones que queremos que corran cuando nos movemos hacia adelante
    public SwordTestSO[] Down_SwordAttacks;// las animaciones que queremos que corran cuando nos movemos hacia atras

    //diccionario de posibles animaciones
    Dictionary<string, SwordTestSO[]> AttackDictionary = new Dictionary<string, SwordTestSO[]>();

    //diccionario de attack Clips
    Dictionary<int, AnimationClip> AttackClipInfo = new Dictionary<int, AnimationClip>();


    SwordTestSO[] CurrentCombo; // la serie de animaciones que tenemos que ejecutar

    [Space]
    [Header("Variables")]
    [SerializeField] int _ComboCounter; // el combo actual, para cyclear entre animaciones
    [SerializeField] bool _HasFinishedAttack = true; // lock | unlock de ataque, para que el evento de atacar solo se ejecute cuando se termina la animacion
    bool _PlaySound = true; // controla si podemos tocar un sonido

    [Space]
    [Header("Daño multiplicador")]
    public float AttackDamageMod = 1f; // base es 1

    /// <summary>
    /// valor que cambia la velocidad de animacion de ataque, valor 1 = normal
    /// </summary>
    [SerializeField]private float AttackSpeedMod = 1f;

    [Space] // cositas para el item de esquivar y dd
    [Header("mods de item")]
    public bool IsEvasionBoostActive = false; //booleano para saber si toco shift / esquivo
    private const float EVASION_MULTIPLIER = 2.0f; // multi fijo del 2x (SIEMPRA VA A SER POR DOS) a menos q digan q lo quieren de mas pero me parece roto ahrw

    [Header("refe del context")]
    [SerializeField] private PlayerContext _PlayerContext;

    [Header("cambio de material")]
    [SerializeField] private MeshRenderer _SwordMeshRenderer; // mesh de la espada
    [SerializeField] private Material _NormalMaterial; //mat normal
    [SerializeField] private Material _BoostMaterialGold; //mat dorado para el dd
    // me traje el efecto de lucas en el bleed a aca ya q manejo el dorado aca
    [SerializeField] private Color _BleedColor = new Color(1f, 0.1f, 0.1f, 1f); // color rojo para el bleed 
    [SerializeField] private float _BleedBlendValue = 0.04f; // fresnelgradientblend para el item del bleed
    private int _activeBoostSources = 0; // cuento los efectos activos (evade, dd al 30%, etc)
    private bool _isBleedEffectActive = false; //bool para saber si el efecto esta activo o no

  


    InputMovement _PlayerInputs;

    [Header("Shader Settings")]
    [SerializeField] private Renderer _SwordRenderer;
    [SerializeField] private string shaderFloatName = "_FresnelGradientBlend";
    [SerializeField] private float chargeThreshold = 1.5f;
    [SerializeField] private float blendSpeed = 10f;

    [SerializeField] private List<TrailRenderer> _swordTrails = new List<TrailRenderer>();
    [SerializeField] private List<TrailRenderer> _bleedSwordTrails = new List<TrailRenderer>();

    // variable para la dir del ataque (como hacian con el cs antes)
    private Vector3 _AttackDir = Vector3.zero;

    public override void VS_Initialize()
    {
        if(DialogueManager.instance != null)
        {
            DialogueManager.instance.OnDialogueStart += sheateWeapon;
            DialogueManager.instance.OnDialogueEnd += UnsheateWeapon;
        }
    }



    private void Start()
    {
        //armado de diccionario
        AttackDictionary["Left"] = Left_SwordAttacks;
        AttackDictionary["Right"] = Right_SwordAttacks;
        AttackDictionary["Up"] = UP_SwordAttacks;
        AttackDictionary["Down"] = Down_SwordAttacks;

        // trails normales off
        foreach (var trail in _swordTrails)
        {
            if (trail != null)
            {
                trail.emitting = false;
                trail.Clear();
            }
        }

        // trails de sangre off
        foreach (var trail in _bleedSwordTrails)
        {
            if (trail != null)
            {
                trail.emitting = false;
                trail.Clear();
            }
        }

        _AnimHandler.TryGetAnimator("PlayerWeapon", out _Anim);

        
    }


    public override void VS_Runlogic(params object[] a)
    {

        if (a == null || a.Length == 0)
        {
            Debug.LogError("PlayerAttackScript recieving null parameters");
            return;
        }
        RunData((InputMovement)a[0]);
    }

    public void RunData(InputMovement PlayerInputs)
    {
        _PlayerInputs = PlayerInputs;
        if (PlayerInputs.SustainedLeftMouseClick)
        {
            //terminamos de realizar un ataque?
            if (_HasFinishedAttack)
            {
                _HasFinishedAttack = false;
                AnimatorSelector(); // selector de animaciones
                StartCoroutine(AttackHandle());
            }
        
            
        }
    }

    private void AnimatorSelector()
    {
        // ya no se hace camerashake cuando se pega
        Vector2 value = _PlayerInputs.Movement;
        if(value == Vector2.left)
        {
            CurrentCombo = AttackDictionary["Left"];
            _AttackDir = new Vector3(1, 0, 0); // der (tiene mas sentido asi para mi pero si quieren pongan el menos adelante y le cambian direccion)
        }
        else if(value == Vector2.right)
        {
            CurrentCombo = AttackDictionary["Right"];
            _AttackDir = new Vector3(-1, 0, 0); // izq

        }
        else if(value == Vector2.up)
        {
            CurrentCombo = AttackDictionary["Up"];
            _AttackDir = new Vector3(0, -1, 0);  //abajo
        }
        else if(value == Vector2.down)
        {
            CurrentCombo = AttackDictionary["Down"];
            _AttackDir = new Vector3(0, 1, 0); // arriba

        }
        else
        {
            CurrentCombo = AttackDictionary["Left"];
            _AttackDir = new Vector3(-1, 0, 0); // der
        }
    } // funcion de seleccion de animaciones de ataque

    private IEnumerator AttackHandle()
    {
        // llamo al efecto de la cam dependiendo el animselector para q siga el tipo de golpe  (izq,der,etc)
        Camera.main.GetComponent<CameraFollowSword>()?.DoHitEffect(_AttackDir);

        // agarro el multiplicador desde las estadisticas
        float currentDamageMultiplier = 1f; // el default
        float flatDamageBoost = 0f;         // nuevo para el progreso q compro

        if (_PlayerContext != null && _PlayerContext.Stats != null)
        {
            //DamageMultiplier es lo seteado en el float stat list 
            currentDamageMultiplier = _PlayerContext.Stats.GetFloatStatValue("DamageMultiplier");

            // el statid que el playercontext va a actualizar
            flatDamageBoost = _PlayerContext.Stats.GetFloatStatValue("BaseDamageFlatBoost");
        }

        // aplico el dd si la evasion esta true
        if (IsEvasionBoostActive)
        {
            // si esta en true se usa el x2 ignorando los multis que puedas llegar a tener por el otro item de 30% de vida == dd
            currentDamageMultiplier = EVASION_MULTIPLIER;

            // despues del golpe queda en false
            IsEvasionBoostActive = false;
            Debug.Log("Golpe de evasion x2 aplicado"); // test
            // elimino registro
            UnregisterEvasionBoost();
        }

        if (_ComboCounter >= CurrentCombo.Length) // nos excedimos de combo
        { 
            _ComboCounter = 0;
        }

        //obtenemos el ataque actual
        var currentAttack = CurrentCombo[_ComboCounter];

        // (dmg del so * el multiplicador del statsmanager) + el q compro
        float finalCalculatedDamage = (currentAttack.Damage * currentDamageMultiplier) + flatDamageBoost;

        // seteamos variables de daño y knockback
        _Weapon.Damage = finalCalculatedDamage; // uso el calculado de arriba
        _Weapon.KnockBack = currentAttack.KnockBack;

        //aceleramos / slowdown de animacion
        _Anim.speed = AttackSpeedMod * TimeDilationManager.GlobalTimeScale;


        // obtenemos el hash.
        int attackHash = currentAttack.HashedID;

        //seteamos triggers
        _AnimHandler.SetParameter("PlayerWeapon", "AttackID", AnimatorControllerParameterType.Int, attackHash);
        _AnimHandler.SetParameter("PlayerWeapon", "Attack", AnimatorControllerParameterType.Trigger);


        // CÁLCULOS CORRECTOS
        float startTime = (float)currentAttack.StartDealingDamageFrame / currentAttack.AnimFrameRate;
        float endTime = (float)currentAttack.EndDealingDamageFrame / currentAttack.AnimFrameRate;
        float totalDuration = (float)currentAttack.AnimationLenght / currentAttack.AnimFrameRate;

        float waitBeforeDamage = startTime;
        float damageWindowDuration = endTime - startTime;
        float remainingTime = totalDuration - endTime;


       

        yield return new WaitForSeconds(waitBeforeDamage / AttackSpeedMod);

        // activo los trails q necesito
        if (_isBleedEffectActive)
        {
            // los de sangrado
            foreach (var trail in _bleedSwordTrails)
            {
                if (trail != null) trail.emitting = true;
            }

            // desactivo y limpio noramles
            foreach (var trail in _swordTrails)
            {
                if (trail != null)
                {
                    trail.emitting = false;
                    trail.Clear();
                }
            }
        }
        else // sin sangrado
        {
            // actvio normales
            foreach (var trail in _swordTrails)
            {
                if (trail != null) trail.emitting = true;
            }

            // desactivo y limpio sangrado
            foreach (var trail in _bleedSwordTrails)
            {
                if (trail != null)
                {
                    trail.emitting = false;
                    trail.Clear();
                }
            }
        }

        //
        //indicamos al arma que comienze a realizar daño
        _Weapon.Attacking();

        // trails
        float trailDeactivateTime = Mathf.Max(currentAttack.AnimationLenght - 1f, 0.45f); //cuando se desactiva el trail antes del final de la animacion
        // chau al invoke
        //Invoke(nameof(StopTrails), trailDeactivateTime);


        //tocamos el sonido del swing
        if (_PlaySound && !_HasFinishedAttack)
        {
            //ejemplo de funcionamiento del Sound manager
            SoundManager.Instance.CreateSound() //creamos sonido
                        .WithSoundData(soundData[0]) //con data de audio (variable setteada)
                        .WithPosition(this.transform.position) //con posicion en custom (si no es 0,0,0)
                        .WithSpatialBlend(soundData[0].SpatialBlend) // con blendeo espacial
                        .WithRandomPitch(true) // con pitch de sonido (default -0.05 a 0.05)
                        .play(); // tocamos el sonido

            _PlaySound = false;
        }



      
        yield return new WaitForSeconds(damageWindowDuration /AttackSpeedMod);

        //ahora que paso el tiempo de daño, desactivamos el daño
        _Weapon.StopAttacking();

        // fuerzo el stop
        StopTrails();

        //frenamos hasta que finalize la ejecucion
        // reducimos un poco el cooldown para hacer mas smooth el ataque, basicamente que
        // no tenga tiempo de volver a idle
     
        yield return new WaitForSeconds((remainingTime / AttackSpeedMod));

        FinishAttack();

     
    }


    public void FinishAttack()
    {
        //_Anim.SetTrigger("AttackTrigger");
        //_Anim.ResetTrigger("ChargeRelease");
        //_AnimHandler.SetParameter("Weapon", "AttackTrigger", AnimatorControllerParameterType.Trigger);

        //foreach (var trail in _swordTrails)
        //{
        //    if (trail != null) trail.emitting = false;
        //}

        _PlaySound = true;
        _ComboCounter++;
        _HasFinishedAttack = true;
    }

    // registro boost
    public void RegisterBoostSource()
    {
        _activeBoostSources++;
        UpdateMaterialEffect();
    }

    // elimino registro
    public void UnregisterBoostSource()
    {
        _activeBoostSources = Mathf.Max(0, _activeBoostSources - 1);
        UpdateMaterialEffect();
    }

    // se llama al inicio del dash. FUNC SOLO PARA EL DASH
    public void RegisterEvasionBoost()
    {
        _activeBoostSources++;
        UpdateMaterialEffect();
    }

    // se llama cuando el ataque usa el efecto
    public void UnregisterEvasionBoost()
    {
        // contador para evitar el spam de dash y rompa el mat
        if (_activeBoostSources > 0)
        {
            _activeBoostSources--;
        }

        UpdateMaterialEffect();
    }

    // efecto sangrado
    public void SetBleedEffectActive(bool isActive)
    {
        if (_isBleedEffectActive == isActive) return;

        _isBleedEffectActive = isActive;
        UpdateMaterialEffect();

        // limpio y activo trails para forzarlo y q no se cambien solo si hago el primer ataque
        if (isActive)
        {
            foreach (var trail in _swordTrails)
            {
                if (trail != null)
                {
                    trail.emitting = false;
                    trail.Clear();
                }
            }
        }
        else
        {
            foreach (var trail in _bleedSwordTrails)
            {
                if (trail != null)
                {
                    trail.emitting = false;
                    trail.Clear();
                }
            }
        }
    }

    // updateo el material para aplicar el cambio de mesh
    private void UpdateMaterialEffect()
    {
        if (_SwordMeshRenderer == null) return;

        if (_activeBoostSources > 0)
        {
            //priorizo el dorado
            _SwordMeshRenderer.material = _BoostMaterialGold;
        }
        else if (_isBleedEffectActive)
        {
            // sangrado
            // uso el base pero meto lo de sangre
            _SwordMeshRenderer.material = _NormalMaterial;
            Material mat = _SwordMeshRenderer.material;

            if (mat.HasProperty("_FresnelGradientBlend"))
            {
                // blend del sangrado
                mat.SetFloat("_FresnelGradientBlend", _BleedBlendValue);
            }

            if (mat.HasProperty("_Color"))
            {
                // color rojo
                mat.SetColor("_Color", _BleedColor);
            }
        }
        else
        {
            // si no hay nada queda el normbal (ningun boost)
            _SwordMeshRenderer.material = _NormalMaterial;
        }
    }
    #region Misc
    void sheateWeapon()
    {
        _Weapon.gameObject.SetActive(false);
        print("sheating weapon");
    }

    void UnsheateWeapon()
    {
        _Weapon.gameObject.SetActive(true);
        print("unsheating weapon");
    }


    private void StopTrails()
    {
        // desactivo normales
        foreach (var trail in _swordTrails)
        {
            if (trail != null)
            {
                trail.emitting = false;
                trail.Clear();
            }
        }
        // desactivo sangrados
        foreach (var trail in _bleedSwordTrails)
        {
            if (trail != null)
            {
                trail.emitting = false;
                trail.Clear();
            }
        }
    }


    #endregion
}
