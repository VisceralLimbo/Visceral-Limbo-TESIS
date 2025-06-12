using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exe_Skill2_ParryBehaviour : Visceral_SkillLogic
{
    //usercontext 

    [Header("References")]
    [SerializeField] Player_CameraController _camContext;
    [SerializeField] AnimatorOverrideController _ANCO;
    [SerializeField] Animator _Anim;

    [Space]
    [Header("Variables")]
    [SerializeField] float ParryRange,ParryRadius;
    [SerializeField] Vector3 PlayerDirector;
    [SerializeField] LayerMask ProyectileLayerMask;

    [Space]
    [Header("Miscellaneous")]
    [SerializeField] bool DrawGizmos;


    public override void Initialize(Visceral_AbilitySO data, Visceral_SkillManager Skmanager, PlayerContext UserContext = null)
    {
        base.Initialize(data, Skmanager, UserContext);
        _Anim = _UserContext.PlayerGameObject.transform.root.GetComponentInChildren<Animator>();

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
        _Anim.CrossFade("Exe_Skill2", 0.01f, 0);
        _Anim.ResetTrigger("Skill2Trigger");
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
         Vector3 ParryOrigin = _UserContext.PlayerTransform.position + ParryDirection * ParryRange + new Vector3(0,0.5f,0);

         //almacenamos la direccion global para proyectiles
         PlayerDirector = ParryDirection.normalized;

         //buscar proyectiles parriables
         Collider[] hits = Physics.OverlapSphere(ParryOrigin, ParryRadius, ProyectileLayerMask);

         //vemos cuantos hits podemos parriar
         foreach (var hit in hits)
         {
                
            if (hit.TryGetComponent<IParriable>(out var parriable))
            {
                    Debug.Log("Parry encontrado" + hit.gameObject.name);
                    var DMScore = new DamageScore { Attacker = _UserContext };
                    DMScore.AddTag(ScoreFlags.Skill2Kill);
                    DMScore.AddTag(ScoreFlags.Parried);
                    parriable.parried(DMScore, PlayerDirector);

                    HitStop.Stop(0.1f);
            }
         }

         //Catch! no terminamos la animacion del ataque
        while (_Anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.9f)
        {
            print(_Anim.GetCurrentAnimatorStateInfo(0).fullPathHash);
            
            yield return null;
        }
        print("Resseting trigger");
        _Anim.SetTrigger("Skill2Trigger");
        _Anim.speed = 1.0f;
   }


    private void OnDrawGizmos()
    {
        if (_UserContext == null || DrawGizmos == false) return;

        Gizmos.color = Color.blue;

        Vector3 ParryOrigin = _UserContext.PlayerTransform.position + _UserContext.PlayerTransform.forward * ParryRange +new Vector3(0, 0.5f, 0);

        Gizmos.DrawSphere(ParryOrigin, ParryRadius);
    }

}
