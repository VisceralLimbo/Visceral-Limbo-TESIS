using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagDollTimer : MonoBehaviour
{
    [SerializeField] float _RagDollDuration;
    [SerializeField] Rigidbody _RootRigid;
    public Rigidbody RootRigid { get { return _RootRigid; } }

    private void Start()
    {
        StartCoroutine(DestroySelf());
    }

    IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(_RagDollDuration);

        Destroy(this.gameObject);
    }
}
