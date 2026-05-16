using UnityEngine;
using TMPro;
using System.Collections;
using System;

public class BloodEchoesUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI bloodEchoesText;
    private Coroutine animateCoroutine;
    private const float AnimationDuration = 0.5f; // lo q dura la anim
    private const float TotalVisibilityTime = 2f; // tiempo del aumento o disminucion
    [SerializeField] SoundData _soundAddCoins;
    private void Awake()
    {
        // apago texto
        if (bloodEchoesText != null)
        {
            bloodEchoesText.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        // Para asegurarse que aparezcan desactivados al inicio del juego
        if (bloodEchoesText != null)
        {
            bloodEchoesText.gameObject.SetActive(false);
        }
    }

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
            if (deltaAmount > 0)
                SoundAddCoins();
            //activo
            bloodEchoesText.gameObject.SetActive(true);

            // paro animaciones anteriores por las dudas
            if (animateCoroutine != null)
            {
                StopCoroutine(animateCoroutine);
            }

            // ya no se manda simbolo aca
            animateCoroutine = StartCoroutine(AnimateBloodEchoes(BloodEchoesManager.PreviousBloodEchoes,currentValue));
        }
        else if (bloodEchoesText != null)
        {
            // si no hay se actualiza de una
            bloodEchoesText.text = currentValue.ToString("F0");
        }
    }

    private void SoundAddCoins()
    {
        SoundManager.Instance.CreateSound().WithSoundData(_soundAddCoins).play();
    }

    // ahora para refreshear el valor actual
    public void RefreshUI(bool shouldShow = true)
    {
        // actualizo instantaneo
        if (bloodEchoesText != null)
        {
            // paso el shouldshow
            bloodEchoesText.gameObject.SetActive(shouldShow);
            bloodEchoesText.text = BloodEchoesManager.BloodEchoes.ToString("F0");
        }
    }

    private IEnumerator AnimateBloodEchoes(float startValue, float endValue)
    {
        float timer = 0f;
        string echoesText = " ECOS DE SANGRE"; // el texto

        while (timer < AnimationDuration)
        {
            // lerp para suavizado
            float animatedValue = Mathf.Lerp(startValue, endValue, timer / AnimationDuration);

            // actualizo el texto
            bloodEchoesText.text = Mathf.Round(animatedValue).ToString("F0") + echoesText;

            timer += Time.deltaTime;
            yield return null;
        }

        // valor sin signo osea el total
        bloodEchoesText.text = endValue.ToString("F0") + echoesText;

        // tiempo extra para que quede en pantalla y no se vaya apenas se suma o resta
        float extraWaitTime = TotalVisibilityTime - AnimationDuration;

        // q no sea negatvio xd
        if (extraWaitTime > 0)
        {
            yield return new WaitForSeconds(extraWaitTime);
        }

        // apago el texto cuando termine todo
        bloodEchoesText.gameObject.SetActive(false);
        animateCoroutine = null;
    }
}
