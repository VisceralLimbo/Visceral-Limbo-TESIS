using UnityEngine;
using TMPro;

public class TutorialPopupUI : MonoBehaviour
{
    [SerializeField] private GameObject canvas;
    [SerializeField] private TextMeshProUGUI text;

    public void Show(string message)
    {
        canvas.SetActive(true);
        text.text = message;
    }

    public void Hide()
    {
        canvas.SetActive(false);
    }
}
