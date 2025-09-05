using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{

    [Header("Variables")]
    [SerializeField] float _moveSpeed = 1;
    [SerializeField] float lifeTime = 1;
    [SerializeField] string _MessageOverload; // el mensaje a utilizar
    Color _ColorOverload; // el color del nuevo mensaje 

    [Space]
    [Header("References")]
    public GameObject _Prefab; // prefab a spawnear

    [SerializeField] TextMeshProUGUI _textmesh;
    [SerializeField] CanvasGroup _canvasGroup;

    private void Start()
    {
        _textmesh = GetComponentInChildren<TextMeshProUGUI>();
        _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if(_textmesh != null)
        {
            _textmesh.color = _ColorOverload;
            _textmesh.text = _MessageOverload;
        } 
    }
    private void LateUpdate()
    {
        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180, 0);
    }

    /// <summary>
    /// Funcion para setear texto del mensaje
    /// </summary>
    /// <param name="message"></param>
    /// <param name="color"></param>
    public void SetText(string message, Color color)
    {
        //debido a que esta funcion puede ser llamada antes de que se termine de inicializar el
        // tmpro, es preferible guardar el texto como variable y cargarlo más adelante cuando
        // estemos seguros de que si existe - pato
       _MessageOverload = message;
        _ColorOverload = color;
    }
    private void Update()
    {
        transform.position += Vector3.up * _moveSpeed * Time.deltaTime;

        _canvasGroup.alpha -= Time.deltaTime / lifeTime;

        if (_canvasGroup.alpha <= 0)
            Destroy(gameObject);
    }
}
