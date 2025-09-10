using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] float _moveSpeed = 1f;   // velocidad de subida
    [SerializeField] float lifeTime = 1.5f;   // duración antes de desaparecer

    [SerializeField] string _MessageOverload = "Text";
    Color _ColorOverload = Color.white;

    [Header("References")]
    [SerializeField] TextMeshPro _textmesh;

    private float _timeElapsed = 0f;
    private Color _initialColor;

    private void Awake()
    {
        if (_textmesh == null)
            _textmesh = GetComponentInChildren<TextMeshPro>(true);
    }

    private void Start()
    {
        if (_textmesh != null)
        {
            _textmesh.text = _MessageOverload;
            _textmesh.color = _ColorOverload;
            _initialColor = _textmesh.color;
        }
    }

    public void SetText(string message, Color color)
    {
        _MessageOverload = message;
        _ColorOverload = color;

        if (_textmesh != null)
        {
            _textmesh.text = _MessageOverload;
            _textmesh.color = _ColorOverload;
            _initialColor = _ColorOverload;
        }
    }

    private void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180f, 0);
        }
    }

    private void Update()
    {
        _timeElapsed += Time.deltaTime;
        float t = _timeElapsed / lifeTime;

        // Movimiento hacia arriba mientras existe
        transform.position += Vector3.up * _moveSpeed * Time.deltaTime;

        // Fade out (alpha de 1 ? 0)
        if (_textmesh != null)
        {
            Color c = _initialColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            _textmesh.color = c;
        }

        if (_timeElapsed >= lifeTime)
            Destroy(gameObject);
    }
}