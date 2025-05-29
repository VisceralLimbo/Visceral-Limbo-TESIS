using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDissolveEffect : MonoBehaviour
{
    public float lifetime = 3f;
    public float moveDistance = 50f;

    private float timer = 0f;
    private Vector2 startPos;
    private Vector2 targetPos;
    private TextMeshProUGUI text;
    private RectTransform rectTransform;
    private Color originalColor;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        text = GetComponentInChildren<TextMeshProUGUI>();

        startPos = rectTransform.anchoredPosition;
        targetPos = startPos + new Vector2(0, moveDistance);
        originalColor = text.color;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / lifetime;

        // Mover hacia arriba
        rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);

        // Fade out alpha
        if (text != null)
        {
            Color newColor = originalColor;
            newColor.a = Mathf.Lerp(1f, 0f, t);
            text.color = newColor;
        }

        // Destruir al final
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
