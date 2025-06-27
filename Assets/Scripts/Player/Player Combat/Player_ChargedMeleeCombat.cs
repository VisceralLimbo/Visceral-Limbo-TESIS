using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class Player_ChargedMeleeCombat : Visceral_Script
{
    [Header("Sounds")]
    [SerializeField] SoundData[] soundData;
    [Space]

    public SwordTestSO[] SwordAttacks;
    [SerializeField] float _ChargeAmount,_MaximumCharge,_MinimunToAttack;
    int _ComboCounter;

    [SerializeField] Animator _Anim;
    [SerializeField] Visceral_WeaponBase _Weapon;
    [SerializeField] Slider _ChargeSlider;
    /// <summary>
    /// valor que cambia la velocidad de animacion de ataque, valor 1 = normal
    /// </summary>
    public float AttackSpeedMod = 1f;

    /// <summary>
    /// valor que cambia la velocidad de recarga de ataque
    /// </summary>
    public float RechargeRate;

    InputMovement PlayerInputs;

    [Header("Shader Settings")]
    [SerializeField] private Renderer _SwordRenderer;
    [SerializeField] private string shaderFloatName = "_FresnelGradientBlend";
    [SerializeField] private float chargeThreshold = 1.5f;
    [SerializeField] private float blendSpeed = 10f;

    [SerializeField] private TrailRenderer swordTrail;
    [SerializeField] private TrailRenderer swordTrail2;

    public override void VS_Initialize()
    {
      

        //var PlBS=GetComponent<Player_Base>();

        //PlBS.OnPlayerSkillUse += ResetAnimation;

        DialogueManager.instance.OnDialogueStart += sheateWeapon;
        DialogueManager.instance.OnDialogueEnd += UnsheateWeapon;

    }

    private void Start()
    {
        if (swordTrail != null) swordTrail.emitting = false;
        if (swordTrail != null) swordTrail2.emitting = false;
    }

    public override void VS_Runlogic(params object[] a)
    {
        if (a == null || a.Length == 0)
        {
            Debug.LogError("PlayerChargeAttackScript recieving null parameters");
            return;
        }
        PlayerInputs = (InputMovement)a[0];

        CancelInvoke(nameof(FinishAttack));

        if (PlayerInputs.SustainedLeftMouseClick && !_Anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            _ChargeAmount += Time.deltaTime; 
            if(_ChargeAmount> _MaximumCharge)
            {
                _ChargeAmount= _MaximumCharge;
            }

            _Anim.SetFloat("ChargeMod", 1);
            _Anim.SetTrigger("ChargeUp");
            _Anim.ResetTrigger("Skill1Trigger");

            var shakestrenght= _ChargeAmount / _MaximumCharge * 0.15f ;

            CameraShake.instance.ShakeCamera(0.05f, shakestrenght);
        }
        else
        {
            _ChargeAmount -= Time.deltaTime;
            _Anim.SetFloat("ChargeMod", -1);
            if (_ChargeAmount <= 0) 
            {
                _ChargeAmount = 0;
                _Anim.SetTrigger("CancelAttack");
                _Anim.ResetTrigger("ChargeUp"); _Anim.ResetTrigger("ChargeRelease"); _Anim.ResetTrigger("AttackTrigger");
            } 
        }

        if(_ChargeAmount > _MinimunToAttack && PlayerInputs.ReleasedLeftMouseClick)
        {
            Attack();
            _ChargeAmount = 0;
            

        }

        Invoke(nameof(FinishAttack), 0);

        UpdateUI();

        UpdateShaderBlend();
    }

    private void UpdateShaderBlend()
    {
        if (_SwordRenderer == null) return;

        float currentValue = _SwordRenderer.material.GetFloat(shaderFloatName);
        float targetValue = (_ChargeAmount >= chargeThreshold) ? 1f : 0f;
        float newValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * blendSpeed);

        _SwordRenderer.material.SetFloat(shaderFloatName, newValue);
    }



    private void Attack()
    {
        
        if (swordTrail != null) swordTrail.emitting = true;
        if (swordTrail2 != null) swordTrail2.emitting = true;


        
        _Anim.runtimeAnimatorController = SwordAttacks[_ComboCounter]._AnimatorOV;
        _Anim.speed = AttackSpeedMod;
        _Anim.SetTrigger("ChargeRelease");
        _Anim.ResetTrigger("ChargeUp");
        _Anim.Play("Attack", 0, 0);

        
        float animationLength = 0f;
        foreach (var clip in _Anim.runtimeAnimatorController.animationClips)
        {
            if (clip.name == "Attack")
            {
                animationLength = clip.length / AttackSpeedMod; 
                break;
            }
        }

        
        float trailDeactivateTime = Mathf.Max(animationLength - 1f, 0.45f); //cuando se desactiva el trail antes del final de la animacion
        Invoke(nameof(StopTrails), trailDeactivateTime);

        
        _Weapon.Damage = SwordAttacks[_ComboCounter].Damage;
        _Weapon.KnockBack = SwordAttacks[_ComboCounter].KnockBack;
        _Weapon.Attacking();

        
        _ComboCounter++;
        if (_ComboCounter + 1 > SwordAttacks.Length)
        {
            _ComboCounter = 0;
        }


        //ejemplo de funcionamiento del Sound manager
        SoundManager.Instance.CreateSound() //creamos sonido
                    .WithSoundData(soundData[0]) //con data de audio (variable setteada)
                    .WithPosition(this.transform.position) //con posicion en custom (si no es 0,0,0)
                    .WithSpatialBlend(soundData[0].SpatialBlend) // con blendeo espacial
                    .WithRandomPitch(true) // con pitch de sonido (default -0.05 a 0.05)
                    .play(); // tocamos el sonido
    }

    private void StopTrails()
    {
        if (swordTrail != null) swordTrail.emitting = false;
        if (swordTrail2 != null) swordTrail2.emitting = false;
    }


    public void FinishAttack()
    {
        if(_Anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack") && _Anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f)
        {
            _Weapon.StopAttacking();
            _Anim.SetTrigger("AttackTrigger");
            _Anim.ResetTrigger("ChargeRelease");
            if (swordTrail != null) swordTrail.emitting = false;
            if (swordTrail != null) swordTrail2.emitting = false;
            Debug.Log("finishing attack");
        }

    }

    private void ResetAnimation()
    {
        _ComboCounter= 0;
        _Anim.runtimeAnimatorController = SwordAttacks[_ComboCounter]._AnimatorOV;
        _Anim.ResetTrigger("AttackTrigger"); _Anim.ResetTrigger("ChargeRelease"); _Anim.ResetTrigger("ChargeUp");
        _Anim.Play("Idle", 0, 0);
    }

    private void UpdateUI()
    {
        if(_ChargeAmount <= 0)
        {
            _ChargeSlider.gameObject.SetActive(false);
        }
        else
        {
            _ChargeSlider.value = _ChargeAmount / _MaximumCharge;
            _ChargeSlider.gameObject.SetActive(true);
        }
    }


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
}
