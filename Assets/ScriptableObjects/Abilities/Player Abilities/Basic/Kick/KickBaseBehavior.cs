using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickBaseBehavior : Visceral_SkillLogic
{
    //USERCONTEXT

    [Header("References")]
    [SerializeField] GameObject _kickGameObject;
    [SerializeField] Player_CameraController _camContext;
    [SerializeField] AnimatorOverrideController _ANCO; //not used currently
    [SerializeField] Animator _anim; //not used currently

    [Space]
    [Header("Variables")]
    [SerializeField] float _KickRange, _KickRadius,_KickStrenght,_KickDamage;
    [SerializeField] string _AnimatorKey;
    [SerializeField] string _AnimationKey;
    [SerializeField] Vector3 _PlayerDirector;

    [Space]
    [Header("Miscellaneous")]
    [SerializeField] bool DrawGizmos;

    HashSet<Health_Component> _healthComponents = new HashSet<Health_Component>();
    RaycastHit[] _raycastResults = new RaycastHit[20];

    public override void Initialize(Visceral_AbilitySO data, Visceral_SkillManager Skmanager, PlayerContext UserContext = null)
    {
        base.Initialize(data, Skmanager, UserContext);
        
        _camContext = _UserContext.PlayerGameObject.transform.root.GetComponentInChildren<Player_CameraController>();

        var animHandler = UserContext.gameObject.GetComponent<AnimatorHandler>();

        if(animHandler.TryGetAnimator(_AnimatorKey, out Animator anim))
        {
            _anim = anim;
            _kickGameObject = anim.gameObject;
        }
    }


    public override void ActivateSkill()
    {
        print("Kicking");
        StartCoroutine(LockSkill());
    }

    IEnumerator LockSkill()
    {
        if (_anim != null)
        {
            _kickGameObject.SetActive(true);
            _anim.SetTrigger(_AnimationKey);
        }

        _healthComponents.Clear();
        Ray rayct = new Ray(_camContext.transform.position, _camContext.transform.forward);


        int hits = Physics.SphereCastNonAlloc(rayct, _KickRadius, _raycastResults, _KickRange);

         if(hits <= 0)
         {
            yield break;
         }

         DamageScore DMG = new DamageScore()
         {
                DamageAmount = _KickDamage,
                Attacker = _UserContext,
                ElementalDamage = ElementType.Physical,
                FactionID = _UserContext.faction,
         };

        for(int i = 0; i < hits; i++)
        {
            var hit = _raycastResults[i];
            Vector3 Dir = hit.collider.transform.position - _UserContext.PlayerTransform.position;
            Dir.y = 0f;
            Dir.Normalize();

            if (DamageDispatcher.ProcessSingleHit(hit.collider, ref DMG, Dir, _KickStrenght, _healthComponents, false))
            {
                print(hit.collider.name + " " + hit.collider.gameObject.name);
            }
            else
            {
                print("Couldnt hit " + hit.collider.name + " " + hit.collider.gameObject.name);
            }

        }


        yield break;
    }


    private void OnDrawGizmos()
    {
        if (_UserContext == null || DrawGizmos == false) return;

        Gizmos.color = Color.blue;


        Gizmos.DrawLine(_camContext.transform.position,_camContext.transform.position + _camContext.transform.forward * _KickRange);
    }

}
