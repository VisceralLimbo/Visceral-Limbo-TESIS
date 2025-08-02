using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exe_Skill2_ParryBehaviour : Visceral_SkillLogic
{
    //usercontext 

    [Header("References")]
    [SerializeField] Player_CameraController _camContext;
    [SerializeField] AnimatorOverrideController _ANCO;
    [SerializeField] AnimatorHandler _AnimHandler;
    [SerializeField] Animator _Anim;
    [SerializeField] List<ParticleSystem> _effectParry;

    [Space]
    [Header("Variables")]
    [SerializeField] float ParryRange,ParryRadius;
    [SerializeField] Vector3 PlayerDirector;
    [SerializeField] LayerMask ProyectileLayerMask;
    private Coroutine _ParryCoroutine;

    [Space]
    [Header("Miscellaneous")]
    [SerializeField] bool DrawGizmos;
    [SerializeField] MonoBehaviour currentparry;


    public override void Initialize(Visceral_AbilitySO data, Visceral_SkillManager Skmanager, PlayerContext UserContext = null)
    {
        base.Initialize(data, Skmanager, UserContext);
        _AnimHandler = _UserContext.gameObject.GetComponent<AnimatorHandler>();
        _AnimHandler.TryGetAnimator("PlayerWeapon",out Animator value);
        _Anim = value;
    }

    public override void ActivateSkill()
    {
       _camContext = _UserContext.PlayerGameObject.transform.root.GetComponentInChildren<Player_CameraController>();

        if (_camContext == null)
        {
            Debug.LogError("<color=blue> Visceral Error: no camera found in Player </color>");
            return;
        }

        _Anim.speed = SkillSpeedMod;
        _Anim.runtimeAnimatorController = _ANCO;
        _AnimHandler.SetParameter("PlayerWeapon", "StartSkill2", AnimatorControllerParameterType.Trigger);   
        StartCoroutine(LockSkill());


        SoundManager.Instance.CreateSound()
            .WithSoundData(_SoundDataList[0])
            .WithRandomPitch(true)
            .WithPosition(_UserContext.PlayerTransform.position)
            .play();
    }

   IEnumerator LockSkill()
   {
        //catch! la animacion actual NO es la del Parry
        while (!_Anim.GetNextAnimatorStateInfo(0).IsName("Exe_Skill2"))
        {
            yield return null;
        }

         //direccion de la camara
         Vector3 ParryDirection = _camContext.transform.forward;
         Vector3 ParryOrigin = _UserContext.PlayerTransform.position + ParryDirection * ParryRange + new Vector3(0,1,0);

         //almacenamos la direccion global para proyectiles
         PlayerDirector = ParryDirection.normalized;

         //buscar proyectiles parriables
         Collider[] hits = Physics.OverlapSphere(ParryOrigin, ParryRadius, ProyectileLayerMask);

        if(hits.Length > 0)
        {
            foreach(ParticleSystem ps in _effectParry)
            {
                ps.transform.position = ParryOrigin + ParryDirection + transform.up;
                ps.Play();
            }
        }

         //vemos cuantos hits podemos parriar
         foreach (var hit in hits)
         {
            if(hit == null)
            {
                continue;
            }

            print("hit");
            if (hit.TryGetComponent<IParriable>(out var parriable))
            {
                    Debug.Log("Parry encontrado" + hit.gameObject.name);
                    var DMScore = new DamageScore { Attacker = _UserContext };
                    DMScore.AddTag(ScoreFlags.Skill2Kill);
                    DMScore.AddTag(ScoreFlags.Parried);       
                    HitStop.Stop(0.4f);

                if(_ParryCoroutine != null)
                {
                    StopCoroutine(_ParryCoroutine);
                }

                MonoBehaviour parriableMB = hit.GetComponent<MonoBehaviour>();
                currentparry = parriableMB;
                _ParryCoroutine = StartCoroutine(ParryCoroutine(parriable,parriableMB, DMScore));
            }
         }

         //Catch! no terminamos la animacion del ataque
        while (_Anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.3f)
        {
            //print("animation not finished: " +_Anim.GetCurrentAnimatorStateInfo(0).ToString() + _Anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
            yield return null;
        }

        print("Resseting trigger");
        _AnimHandler.SetParameter("PlayerWeapon", "Skill2Trigger", AnimatorControllerParameterType.Trigger);
        _Anim.speed = 1.0f;
        //_AnimHandler.ResetAllTriggers("PlayerWeapon");
   }

    IEnumerator ParryCoroutine(IParriable parriable,MonoBehaviour MBRef,DamageScore DMS)
    {
        if(MBRef == null)
        {
            yield break;
        }

        if(parriable == null)
        {
            yield break;
        }
        int watchdog = 50;
        while (Time.deltaTime != 1 && watchdog > 0)
        {
            Vector3 ParryDirection = _camContext.transform.forward;
            Vector3 ParryOrigin = _UserContext.PlayerTransform.position + ParryDirection * ParryRange + new Vector3(0, 1, 0);

            //almacenamos la direccion global para proyectiles
            PlayerDirector = ParryDirection.normalized;

            if (parriable == null)
            {
                yield break;
            }


            if (MBRef == null)
            {
                yield break;
            }

            parriable.parried(DMS, PlayerDirector);
            watchdog--;
            print(watchdog);
            yield return null;
        }

    }



    private void OnDrawGizmos()
    {
        if (_UserContext == null || DrawGizmos == false) return;

        Gizmos.color = Color.blue;

        Vector3 ParryOrigin = _UserContext.PlayerTransform.position + _UserContext.PlayerTransform.forward * ParryRange +new Vector3(0, 0.5f, 0);

        Gizmos.DrawSphere(ParryOrigin, ParryRadius);
    }

}
