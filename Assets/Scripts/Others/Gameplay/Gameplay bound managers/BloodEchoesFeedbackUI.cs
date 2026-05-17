using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class BloodEchoesFeedbackUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI feedbackText;

    // tiempo para q se muestre y desaparezca
    private const float DisplayDuration = 2f;
    private Coroutine displayCoroutine;
    public Image bloodechoes;

    private void Awake()
    {

        //q empiece apagado
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
            bloodechoes.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        // se suscribe al evento cuando se actualizan los echoes
        BloodEchoesManager.OnBloodEchoesChanged += ShowFeedback;
    }

    private void OnDisable()
    {
        // se desuscribe para evitar bugs
        BloodEchoesManager.OnBloodEchoesChanged -= ShowFeedback;
    }

    // se llama cada q se cambian los bloodechoes
    private void ShowFeedback(float currentBloodEchoes, float deltaAmount)
    {
        // se muestra si hubo cambio
        if (deltaAmount == 0 || feedbackText == null) return;

        // asigno el color y el signo +|-
        if (deltaAmount > 0)
        {
            // si es ganancia verde y +
            feedbackText.color = Color.green;
            feedbackText.text = $"+{deltaAmount.ToString("F0")}";
        }
        else // si es gasto rojo y -
        {
            feedbackText.color = Color.red;
            feedbackText.text = $"{deltaAmount.ToString("F0")}";
        }

        bloodechoes.gameObject.SetActive(true);
        feedbackText.gameObject.SetActive(true);

        // paro corrutina por si quedo algo
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }

        // inicio corrutina
        displayCoroutine = StartCoroutine(HideFeedbackAfterDelay());
    }

    // desactivo dsp del tiempo
    private IEnumerator HideFeedbackAfterDelay()
    {
        yield return new WaitForSeconds(DisplayDuration);
        if (feedbackText != null)
        {
            bloodechoes.gameObject.SetActive(false);
            feedbackText.gameObject.SetActive(false);
        }
    }
}
