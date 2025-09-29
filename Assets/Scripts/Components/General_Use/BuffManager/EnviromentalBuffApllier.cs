using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnviromentalBuffApllier : MonoBehaviour
{
    [SerializeField] BuffSO _buffToApply;



    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent.TryGetComponent(out BuffManager _BFManager))
        {
            print("aplicando buff");
            _BFManager.AddNewBuff(_buffToApply.BuffID, _buffToApply,1);
        }
    }
}
