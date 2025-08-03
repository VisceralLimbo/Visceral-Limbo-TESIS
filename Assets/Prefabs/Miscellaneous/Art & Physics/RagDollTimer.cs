using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagDollTimer : MonoBehaviour
{
    [SerializeField] float _RagDollDuration;
    [SerializeField] Rigidbody _RootRigid;
    [SerializeField] RagdollBoneRetarget[] _ragdollBoneRetargeters;
    public Rigidbody RootRigid { get { return _RootRigid; } }


    private void Start()
    {
        _ragdollBoneRetargeters = GetComponentsInChildren<RagdollBoneRetarget>(true);
        StartCoroutine(DestroySelf());

        if(_ragdollBoneRetargeters.Length > 0)
        {
            foreach(var rag in _ragdollBoneRetargeters)
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
