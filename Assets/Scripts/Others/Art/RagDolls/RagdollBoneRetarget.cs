using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollBoneRetarget : MonoBehaviour
{
    [SerializeField] Transform VisualBone;
    [SerializeField] Vector3 Offset;
    [SerializeField] Rigidbody _RB;
    [SerializeField] bool change;

    private void Awake()
    {
        _RB= GetComponent<Rigidbody>();
        if(change == false)
        {

        }
    }

    private void OnEnable()
    {
        _RB = GetComponent<Rigidbody>();
        Offset = VisualBone.position - this.transform.position;
    }
    private void Start()
    {
        Offset = VisualBone.position - this.transform.position;
    }

    public void ActivateRagDoll()
    {
        this.enabled= true;
        _RB.isKinematic = false;
        Offset = VisualBone.position - this.transform.position;
        this.gameObject.SetActive(true);
        change = true;
    }


    private void LateUpdate()
    {
        if (change)
        {
            VisualBone.transform.position = transform.position + Offset;
            VisualBone.transform.rotation = transform.rotation;

        }
        else
        {
            this.enabled = false;
        }

    }

}
