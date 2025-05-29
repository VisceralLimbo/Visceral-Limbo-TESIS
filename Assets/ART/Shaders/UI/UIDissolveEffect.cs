using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDissolveEffect : MonoBehaviour
{
    [SerializeField] private float dissolveDuration = 1.5f;
    private Material _material;
    private float _timer = 0f;

    void Start()
    {
        var tmp = GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            // Clona el material para no afectar a otros textos
            _material = Instantiate(tmp.fontMaterial);
            tmp.fontMaterial = _material;
        }
    }

    void Update()
    {
        if (_material != null)
        {
            _timer += Time.deltaTime;
            float amount = Mathf.Clamp01(_timer / dissolveDuration);
            _material.SetFloat("_DissolveAmount", amount);
        }
    }
}
