using UnityEngine;
using TMPro;
using System.Collections;

public class BloodEchoesUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI bloodEchoesText;
    private Coroutine animateCoroutine;
    private const float AnimationDuration = 0.5f; // lo q dura la anim

    private void OnEnable()
    {
        BloodEchoesManager.OnBloodEchoesChanged += UpdateUI;
        RefreshUI();
    }

    private void OnDisable()
    {
        BloodEchoesManager.OnBloodEchoesChanged -= UpdateUI;
    }

    // ahora recibe el valor total y el cambio (deltaAmount).
    private void UpdateUI(float currentValue, float deltaAmount)
    {
        if (bloodEchoesText != null && deltaAmount != 0) // se anima si hay cambio
        {
            // paro animaciones anteriores por las dudas
            if (animateCoroutine != null)
            {
                StopCoroutine(animateCoroutine);
            }

            // saco primero si fue una suma o resta
            string simbol;
            if (deltaAmount > 0)
            {
                simbol = "+"; // si es >0 va un +
            }
            else
            {
                simbol = "-"; // si no entonces es <0 y va un - 
            }

            animateCoroutine = StartCoroutine(AnimateBloodEchoes(BloodEchoesManager.PreviousBloodEchoes,currentValue,simbol));
        }
        else if (bloodEchoesText != null)
        {
            // si no hay se actualiza de una
            bloodEchoesText.text = currentValue.ToString("F0");
        }
    }

    // ahora para refreshear el valor actual
    public void RefreshUI()
    {
        // actualizo instantaneo
        if (bloodEchoesText != null)
        {
            bloodEchoesText.text = BloodEchoesManager.BloodEchoes.ToString("F0");
        }
    }

    private IEnumerator AnimateBloodEchoes(float startValue, float endValue, string simbol)
    {
        float timer = 0f;
        string echoesText = " Blood Echoes"; // el texto

        while (timer < AnimationDuration)
        {
            // lerp para suavizado
            float animatedValue = Mathf.Lerp(startValue, endValue, timer / AnimationDuration);

            // actualizo el texto
            bloodEchoesText.text = simbol + Mathf.Round(animatedValue).ToString("F0") + echoesText;

            timer += Time.deltaTime;
            yield return null;
        }

        // solo va a el + si fue positivo 
        string finalSimbol = (BloodEchoesManager.BloodEchoes > 0 && BloodEchoesManager.BloodEchoes > BloodEchoesManager.PreviousBloodEchoes) ? "+" : "";

        // si es 0 no va a haber signo
        if (BloodEchoesManager.BloodEchoes == 0)
        {
            finalSimbol = "";
        }

        bloodEchoesText.text = finalSimbol + endValue.ToString("F0") + echoesText;
        animateCoroutine = null;
    }
}
