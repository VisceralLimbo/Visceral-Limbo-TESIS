using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct InputStruct //struct usado para recibir datos de inputs
{
    public Vector2 LookDelta;
}


public class Player_CameraController : Visceral_Script
{
    private Vector3 _EulerAngles;

    [Range(0f, 1f)]
    public float sensitivity = 0.4f;

    [SerializeField] private Transform _CameraAnchor;

    [SerializeField] private Player_Base _Base;
    public override void VS_InitializeWithParameters(params object[] a)
    {
        if(a == null || a.Length == 0)
        {
            Debug.LogWarning(this.name + " initialized with no parameters");
        }

        _CameraAnchor= (Transform)a[0]; // casteo explicito - pato

        transform.position = _CameraAnchor.position;
        transform.eulerAngles = _EulerAngles = _CameraAnchor.eulerAngles; //doble set

        //_Base = GetComponentInParent<Player_Base>();
        //var _health = _Base.GetComponentInChildren<Health_Component>();
        //_health.OnDeath += DetachParent;

    }
    /// <summary>
    /// actualizar la posicion de la camara
    /// </summary>
    /// <param name="Target"> nueva posicion</param>
    public void UpdatePosition(Transform Target)
    {
        this.transform.position = Target.position;
    }

    /// <summary>
    /// actualizar la rotacion de la camara
    /// </summary>
    /// <param name="inputData"></param>
    public void UpdateRotation(InputStruct inputData)
    {
        _EulerAngles += new Vector3(-inputData.LookDelta.y, inputData.LookDelta.x) * sensitivity;
        _EulerAngles.x = Mathf.Clamp(_EulerAngles.x, -80, 80);
        transform.eulerAngles = _EulerAngles;
    }

    private void DetachParent()
    {
        this.transform.parent = null;
    }
}
