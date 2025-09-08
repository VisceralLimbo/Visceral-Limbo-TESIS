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
    [SerializeField] Slider _ChargeSlider;
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

    public override void VS_Initialize()
    {

        DialogueManager.instance.OnDialogueStart += sheateWeapon;
        DialogueManager.instance.OnDialogueEnd += UnsheateWeapon;
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
            AttackHandle(); //ataque
            
        }
        else if((_Anim != null && !_HasFinishedAttack))
        {
            FinishAttack();
        }
    }

    private void AnimatorSelector()
    {
        // ya no se hace camerashake cuando se pega
        Vector2 value = _PlayerInputs.Movement;
        if(value == Vector2.left)
        {
            CurrentCombo = AttackDictionary["Left"];


        }
        else if(value == Vector2.right)
        {
            CurrentCombo = AttackDictionary["Right"];

        }
        else if(value == Vector2.up)
        {
            CurrentCombo = AttackDictionary["Up"];

        }
        else if(value == Vector2.down)
        {
            CurrentCombo = AttackDictionary["Down"];

        }
        else
        {
            CurrentCombo = AttackDictionary["Left"];
        }
    } // funcion de seleccion de animaciones de ataque

    private void AttackHandle()
    {
        if (swordTrail != null) swordTrail.emitting = true;
        if (swordTrail2 != null) swordTrail2.emitting = true;


        if (_ComboCounter >= CurrentCombo.Length) // nos excedimos de combo
        { 
            _ComboCounter = 0;
        }

        _AnimHandler.TryGetAnimator("Weapon", out Animator WeaponAnim); // obtener el animator
        WeaponAnim.runtimeAnimatorController = CurrentCombo[_ComboCounter]._AnimatorOV; //override de animaciones
        WeaponAnim.speed = AttackSpeedMod; // velocidad de ataque
        _Anim = WeaponAnim;

        //set de trigger
        _AnimHandler.SetParameter("Weapon", "StartAttack", AnimatorControllerParameterType.Trigger);
        //_AnimHandler.ResetAllTriggers("Weapon");
        print("attack trigger!");

        float animationLength = 0f;
        foreach (var clip in WeaponAnim.runtimeAnimatorController.animationClips)
        {
            if (clip.name == "Attack")
            {
                animationLength = clip.length / AttackSpeedMod;
                break;
            }
        }

        // trails
        float trailDeactivateTime = Mathf.Max(animationLength - 1f, 0.45f); //cuando se desactiva el trail antes del final de la animacion
        Invoke(nameof(StopTrails), trailDeactivateTime);


        // daño y knockbar
        _Weapon.Damage = CurrentCombo[_ComboCounter].Damage;
        _Weapon.KnockBack = CurrentCombo[_ComboCounter].KnockBack;
        _Weapon.Attacking();

        // nos desplazamos una animacion
        _ComboCounter++;

        //lock de ejecucion de evento
        _HasFinishedAttack = false;

        //ejemplo de funcionamiento del Sound manager
        SoundManager.Instance.CreateSound() //creamos sonido
                    .WithSoundData(soundData[0]) //con data de audio (variable setteada)
                    .WithPosition(this.transform.position) //con posicion en custom (si no es 0,0,0)
                    .WithSpatialBlend(soundData[0].SpatialBlend) // con blendeo espacial
                    .WithRandomPitch(true) // con pitch de sonido (default -0.05 a 0.05)
                    .play(); // tocamos el sonido
    }


    public void FinishAttack()
    {
        if (_Anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f)
        {
            _Weapon.StopAttacking();
            //_Anim.SetTrigger("AttackTrigger");
            //_Anim.ResetTrigger("ChargeRelease");
            _AnimHandler.SetParameter("Weapon", "AttackTrigger", AnimatorControllerParameterType.Trigger);

            if (swordTrail != null) swordTrail.emitting = false;
            if (swordTrail != null) swordTrail2.emitting = false;

            //unlock de funcion
            _HasFinishedAttack = true;
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
        if (swordTrail != null) swordTrail.emitting = false;
        if (swordTrail2 != null) swordTrail2.emitting = false;
    }


    #endregion
}
