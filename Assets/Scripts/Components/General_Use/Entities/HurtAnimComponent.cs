using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtAnimComponent : MonoBehaviour,IKnockback
{
    [Header("References")]
    [SerializeField] AnimatorHandler _AnimHandler;
    [SerializeField] Health_Component _HPComp;
    [Space]
    [Header("Variables")]
    private float ForceOfHit;
    [SerializeField] float KnockBackResistance;
    [SerializeField] string _AnimatorKey;
    [SerializeField] string _HurtTriggerParam;
    [SerializeField] string _HurtDirectionX;
    [SerializeField] string _HurtDirectionY;


    private Vector2 DirectionOfHit;

    private void Start()
    {
        if(_HPComp == null)
        {
            _HPComp = GetComponent<Health_Component>();
            if (_HPComp == null)
            {
                _HPComp = GetComponentInParent<Health_Component>();
            }
        }
    

        _HPComp.OnKnockbackTaken += ApplyKnockBack;

        if(_AnimHandler== null)
        {
            _AnimHandler = GetComponent<AnimatorHandler>();
            if (_AnimHandler == null)
            {
                _AnimHandler = GetComponentInParent<AnimatorHandler>();
            }
        }   
    }


    public void ApplyKnockBack(Vector3 KnockbackDir, float Force)
    {
        print("kNOCKBACKED");
        DirectionOfHit = KnockbackDir;
        ForceOfHit = Force;
        ForceOfHit = ForceOfHit / KnockBackResistance;

        if (_AnimHandler != null)
        {
            _AnimHandler.SetParameter(_AnimatorKey, _HurtDirectionX,
                                      AnimatorControllerParameterType.Float, DirectionOfHit.x);
            _AnimHandler.SetParameter(_AnimatorKey, _HurtDirectionY,
                              AnimatorControllerParameterType.Float, DirectionOfHit.y);
            _AnimHandler.SetParameter(_AnimatorKey, _HurtTriggerParam, AnimatorControllerParameterType.Trigger);

        }
    }
}
