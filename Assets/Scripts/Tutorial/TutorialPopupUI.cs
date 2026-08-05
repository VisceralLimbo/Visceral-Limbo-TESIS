using UnityEngine;
using TMPro;

public class TutorialPopupUI : MonoBehaviour
{
    [SerializeField] private GameObject canvas;
    [SerializeField] private TextMeshProUGUI text;

    public void Show(string message)
    {
        if (canvas == null || text == null)
            return;

        canvas.SetActive(true);
        text.text = message;
    }

    public void Hide()
    {
        if (canvas == null)
            return;

        canvas.SetActive(false);
    }
}
