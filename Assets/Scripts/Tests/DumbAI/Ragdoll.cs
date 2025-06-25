using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    [SerializeField] Animator animator;
    Rigidbody[] rigibodies;

    private void Start()
    {
        rigibodies = transform.GetComponentsInChildren<Rigidbody>();
        SetEnable(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            SetEnable(true);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            SetEnable(false);
        }
    }
    private void SetEnable(bool enabled)
    {
        bool isKinematic = !enabled;
        foreach (Rigidbody rigibody in rigibodies)
        {
            rigibody.isKinematic = isKinematic;
        }
        animator.enabled = !enabled;
    }


}
