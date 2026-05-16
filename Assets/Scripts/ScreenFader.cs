using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator FadeOut()
    {
        float t = 0;

        Color c = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;

            c.a = Mathf.Lerp(0, 1, t / fadeDuration);
            fadeImage.color = c;

            yield return null;
        }
    }

    public IEnumerator FadeIn()
    {
        float t = 0;

        Color c = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;

            c.a = Mathf.Lerp(1, 0, t / fadeDuration);
            fadeImage.color = c;

            yield return null;
        }
    }
}
