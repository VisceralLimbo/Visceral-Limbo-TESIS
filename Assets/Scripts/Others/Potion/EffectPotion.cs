using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectPotion : MonoBehaviour
{
    [SerializeField] float _amplitude = 0.25f;
    [SerializeField] float _speed = 1f;

    Vector3 _startpos;

    private void Start()
    {
        _startpos = transform.position;
    }

    private void Update()
    {
        float MoveY = Mathf.Sin(Time.time * _speed) * _amplitude;
        transform.position = _startpos + new Vector3(0f, MoveY, 0f);
    }
}
