using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickBaseBehavior : Visceral_SkillLogic
{
    //USERCONTEXT

    [Header("References")]
    [SerializeField] Player_CameraController _camContext;
    [SerializeField] AnimatorOverrideController _ANCO; //not used currently
    [SerializeField] Animator _Anim; //not used currently

    [Space]
    [Header("Variables")]
    [SerializeField] float KickRange, KickRadius,KickStrenght,KickDamage;
    [SerializeField] Vector3 PlayerDirector;

    [Space]
    [Header("Miscellaneous")]
    [SerializeField] bool DrawGizmos;

    public override void Initialize(Visceral_AbilitySO data, Visceral_SkillManager Skmanager, PlayerContext UserContext = null)
    {
        base.Initialize(data, Skmanager, UserContext);
        _Anim = _UserContext.PlayerGameObject.transform.root.GetComponentInChildren<Animator>();
        _camContext = _UserContext.PlayerGameObject.transform.root.GetComponentInChildren<Player_CameraController>();

    }


    public override void ActivateSkill()
    {
        StartCoroutine(LockSkill());
    }

    IEnumerator LockSkill()
    {


        //direccion de la camara
        //Vector3 KickDirection = _camContext.transform.forward;
        //Vector3 KickOrigin = _UserContext.PlayerTransform.position + KickDirection * KickRange;

        //transform.position = KickOrigin;
        //almacenamos la direccion global para proyectiles
        //PlayerDirector = KickDirection.normalized;

        print("Kicking ");

        Ray rayct = new Ray(_camContext.transform.position, _camContext.transform.forward);

        if (!Physics.Raycast(rayct, out RaycastHit HitInfo, KickRange))
            yield break;

        GameObject HitOBJ = HitInfo.collider.gameObject;

        // evito pegarme a mi mismo
        if (HitInfo.collider.GetComponentInParent<PlayerContext>() == _UserContext)
            yield break;

        print(HitOBJ.name);

        Health_Component HPComp = HitOBJ.GetComponentInParent<Health_Component>();

        if (HPComp == null)
        {
            HPComp = HitOBJ.GetComponentInChildren<Health_Component>();
        }

        if (HPComp != null)
        {
            DamageScore DMG = new DamageScore()
            {
                DamageAmount = KickDamage,
                Attacker = _UserContext,
                ElementalDamage = ElementType.Physical,
                FactionID = _UserContext.faction,
            };

            Vector3 Dir = HPComp.transform.position - _UserContext.PlayerTransform.position;
            Dir.y = 0f;
            Dir.Normalize();

            print(HPComp.name + " " + HitOBJ.name);
            HPComp.TakeDamageWithKnockback(Dir, KickStrenght, DMG);
        }

        yield break;
    }


    private void OnDrawGizmos()
    {
        if (_UserContext == null || DrawGizmos == false) return;

        Gizmos.color = Color.blue;


        Gizmos.DrawLine(_camContext.transform.position,_camContext.transform.position + _camContext.transform.forward * KickRange);
    }

}
