using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagDollTimer : MonoBehaviour
{
    [SerializeField] AnimatorHandler _AnimHandler;
    [SerializeField] Health_Component _Hp;
    [SerializeField] float _RagDollDuration;
    [SerializeField] Rigidbody _RootRigid;
    [SerializeField] RagdollBoneRetarget[] _ragdollBoneRetargeters;
    [SerializeField]
    LimbDesmemberComponent[] _limbDesmemberComponents;
    [SerializeField] string AnimatorKey;

    public Rigidbody RootRigid { get { return _RootRigid; } }


    private void Start()
    {
        _ragdollBoneRetargeters = GetComponentsInChildren<RagdollBoneRetarget>(true);

        if(_AnimHandler == null)
        {
            _AnimHandler = GetComponent<AnimatorHandler>();
            if(_AnimHandler == null)
            {
                _AnimHandler  = GetComponentInChildren<AnimatorHandler>();
            }
        }

        if(_Hp == null)
        {
            _Hp = GetComponentInParent<Health_Component>();

        }

        if(_Hp != null)
        {
            _Hp.OnDeath += StartRagdolling;
        }

        if(_limbDesmemberComponents.Length <= 0)
        {
            _limbDesmemberComponents = GetComponentsInChildren<LimbDesmemberComponent>();
        }
    }

    private void StartRagdolling()
    {
        this.transform.parent = null;
        StartCoroutine(DestroySelf());

        _AnimHandler.TryGetAnimator(AnimatorKey, out Animator Anim);
        Anim.speed = 1;
        Anim.enabled = false;

        if (_ragdollBoneRetargeters.Length > 0)
        {
            foreach (var rag in _ragdollBoneRetargeters)
            {
                rag.ActivateRagDoll();
            }
        }

        if(_limbDesmemberComponents.Length > 0)
        {
            foreach(var Limb in _limbDesmemberComponents)
            {
                Limb.DismemberLimb();
            }
        }
    }

    IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(_RagDollDuration);

        Destroy(this.gameObject);
    }
}
