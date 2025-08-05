using UnityEngine;
using UnityEngine.UI;

public class FadeAndDestroy : MonoBehaviour
{
    public float duration = 2f;
    private Image image;
    private float timer;

    void Start()
    {
        image = GetComponent<Image>();
        timer = duration;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        float alpha = Mathf.Clamp01(timer / duration);
        Color c = image.color;
        c.a = alpha;
        image.color = c;

        if (timer <= 0)
            Destroy(gameObject);
    }
}

