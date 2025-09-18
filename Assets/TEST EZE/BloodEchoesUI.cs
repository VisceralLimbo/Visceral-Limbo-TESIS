using UnityEngine;
using TMPro;

public class BloodEchoesUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI bloodEchoesText;

    private void OnEnable()
    {
        BloodEchoesManager.OnBloodEchoesChanged += UpdateUI;
        UpdateUI(BloodEchoesManager.BloodEchoes); // updateo el valor siempre q se active por las dudas
    }

    private void OnDisable()
    {
        BloodEchoesManager.OnBloodEchoesChanged -= UpdateUI;
    }

    public void UpdateUI(float currentValue)
    {
        if (bloodEchoesText != null)
            bloodEchoesText.text = currentValue.ToString();
    }

    // funcion para q llamen en cualquier otro lugar que quieran que se actualicen
    public void RefreshUI()
    {
        UpdateUI(BloodEchoesManager.BloodEchoes);
    }
}
