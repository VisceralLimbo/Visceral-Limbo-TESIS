using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 1;
    [SerializeField] float lifeTime = 1;
    TextMeshProUGUI _textmesh;
    CanvasGroup _canvasGroup;

    private void Awake()
    {
        _textmesh = GetComponentInChildren<TextMeshProUGUI>();
        _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }
    private void LateUpdate()
    {
        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180, 0);
    }
    public void SetText(string message, Color color)
    {
        _textmesh.text = message;
        _textmesh.color = color;
    }
    private void Update()
    {
        transform.position += Vector3.up * _moveSpeed * Time.deltaTime;

        _canvasGroup.alpha -= Time.deltaTime / lifeTime;

        if (_canvasGroup.alpha <= 0)
            Destroy(gameObject);
    }
}
