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
    SwordTestSO[] CurrentCombo; // la serie de animaciones que tenemos que ejecutar

    [Space]
    [Header("Variables")]
    [SerializeField]int _ComboCounter; // el combo actual, para cyclear entre animaciones
    [SerializeField] bool _HasFinishedAttack = true; // lock | unlock de ataque, para que el evento de atacar solo se ejecute cuando se termina la animacion
    bool _PlaySound = true
        ; // controla si podemos tocar un sonido

    /// <summary>
    /// valor que cambia la velocidad de animacion de ataque, valor 1 = normal
    /// </summary>
    public float AttackSpeedMod = 1f;


    InputMovement _PlayerInputs;

    [Header("Shader Settings")]
    [SerializeField] private Renderer _SwordRenderer;
    [SerializeField] private string shaderFloatName = "_FresnelGradientBlend";
    [SerializeField] private float chargeThreshold = 1.5f;
    [SerializeField] private float blendSpeed = 10f;

    [SerializeField] private TrailRenderer swordTrail;
    [SerializeField] private TrailRenderer swordTrail2;

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

        if (swordTrail != null) swordTrail.emitting = false;
        if (swordTrail != null) swordTrail2.emitting = false;

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

        if (PlayerInputs.SustainedLeftMouseClick && _HasFinishedAttack)
        {
            print("perform attack");
            _HasFinishedAttack = false;
            AnimatorSelector(); // selector de animaciones
            StartCoroutine(AttackHandle());
            
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

        if (swordTrail != null) swordTrail.emitting = true;
        if (swordTrail2 != null) swordTrail2.emitting = true;



        if (_ComboCounter >= CurrentCombo.Length) // nos excedimos de combo
        { 
            _ComboCounter = 0;
        }

        //obtenemos el ataque actual
        var currentAttack = CurrentCombo[_ComboCounter];

        // seteamos variables de daño y knockback
        _Weapon.Damage = currentAttack.Damage;
        _Weapon.KnockBack = currentAttack.KnockBack;

        //aceleramos / slowdown de animacion
        _Anim.speed = AttackSpeedMod;


        // obtenemos el hash.
        int attackHash = currentAttack.HashedID;

        //seteamos triggers
        _AnimHandler.SetParameter("PlayerWeapon", "AttackID", AnimatorControllerParameterType.Int, attackHash);
        _AnimHandler.SetParameter("PlayerWeapon", "Attack", AnimatorControllerParameterType.Trigger);


        //
        //indicamos al arma que comienze a realizar daño
        _Weapon.Attacking();

        // trails
        float trailDeactivateTime = Mathf.Max(currentAttack.AnimationLenght - 1f, 0.45f); //cuando se desactiva el trail antes del final de la animacion
        Invoke(nameof(StopTrails), trailDeactivateTime);


        if (_PlaySound)
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
      



        //frenamos hasta que finalize la ejecucion
        // reducimos un poco el cooldown para hacer mas smooth el ataque, basicamente que
        // no tenga tiempo de volver a idle
        float AttackCooldown = (currentAttack.AnimationLenght / AttackSpeedMod) *0.9f;

        yield return new WaitForSeconds(AttackCooldown);

        FinishAttack();

     
    }


    public void FinishAttack()
    {
            _Weapon.StopAttacking();
            //_Anim.SetTrigger("AttackTrigger");
            //_Anim.ResetTrigger("ChargeRelease");
            //_AnimHandler.SetParameter("Weapon", "AttackTrigger", AnimatorControllerParameterType.Trigger);

            if (swordTrail != null) swordTrail.emitting = false;
            if (swordTrail != null) swordTrail2.emitting = false;
        _PlaySound = true;
        _HasFinishedAttack = true;
        _ComboCounter++;
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
        if (swordTrail != null) swordTrail.emitting = false;
        if (swordTrail2 != null) swordTrail2.emitting = false;
    }


    #endregion
}
