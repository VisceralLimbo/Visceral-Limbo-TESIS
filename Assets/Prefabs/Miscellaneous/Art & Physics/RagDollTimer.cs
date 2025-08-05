using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagDollTimer : MonoBehaviour
{
    [SerializeField] float _RagDollDuration;
    [SerializeField] Rigidbody _RootRigid;
    [SerializeField] RagdollBoneRetarget[] _ragdollBoneRetargeters;
    [SerializeField] string AnimatorKey;

    public Rigidbody RootRigid { get { return _RootRigid; } }


    private void Start()
    {
        _ragdollBoneRetargeters = GetComponentsInChildren<RagdollBoneRetarget>(true);
        Health_Component HPComp = GetComponentInParent<Health_Component>();
        HPComp.OnDeath += StartRagdolling;
    }

    private void StartRagdolling()
    {
        AnimatorHandler animatorHandler = GetComponent<AnimatorHandler>();
        if(animatorHandler == null)
        {
            animatorHandler= GetComponentInParent<AnimatorHandler>();
            
        }
        if(animatorHandler.TryGetAnimator(AnimatorKey, out Animator anim))
        {
            anim.enabled = false;
        }



        StartCoroutine(DestroySelf());

        if (_ragdollBoneRetargeters.Length > 0)
        {
            foreach (var rag in _ragdollBoneRetargeters)
            {
                rag.ActivateRagDoll();
            }
        }
    }

    IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(_RagDollDuration);

        Destroy(this.gameObject);
    }
}
