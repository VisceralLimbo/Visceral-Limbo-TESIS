using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("ui refes")]
    [SerializeField] private Slider smoothHealthSlider;
    [SerializeField] private Image currentHealthFillImage;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private TextMeshProUGUI healthValueText;
    

    [Header("smooth health")]
    // velocidad a la q quieren q baje el daño amarillo de la barra de vida del boss
    [SerializeField] private float lerpSpeed = 1f;

    // refes
    private Health_Component bossHealthComponent;
    private float maxHealth;
    private float targetHealthValue;

    void Update()
    {
        // el lerp solo esta activo si el amarillo esta por encima del rojo
        if (smoothHealthSlider.value > targetHealthValue)
        {
            // lerp
            float lerpedValue = Mathf.Lerp(smoothHealthSlider.value, targetHealthValue, Time.deltaTime * lerpSpeed);
            smoothHealthSlider.value = lerpedValue;
        }
    }


    // spawnea el boss para inicializar la barra de vida
    public void InitializeBossBar(Health_Component healthComp, string name)
    {
        // guardo refes y valores
        bossHealthComponent = healthComp;
        maxHealth = bossHealthComponent.MaxHealth;

        // ui
        bossNameText.text = name;
        smoothHealthSlider.maxValue = maxHealth;

        // todo inicia al mango
        smoothHealthSlider.value = maxHealth;
        currentHealthFillImage.fillAmount = 1f;
        targetHealthValue = maxHealth;

        this.gameObject.SetActive(true);
        UpdateHealthUI();
    }

    // se llama cada q cambia la vida
    public void UpdateHealthUI()
    {
        if (bossHealthComponent == null) return;

        float currentHealth = bossHealthComponent.CurrentHealth;

        currentHealthFillImage.fillAmount = currentHealth / maxHealth;

        // objetivo para el lerp
        targetHealthValue = currentHealth;

        // se actualiza el texto
        healthValueText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
    }

    // se llama cuando muewre el boss
    public void HideBossBar()
    {
        // apago ui
        this.gameObject.SetActive(false);
    }

    public void OnDestroy()
    {
        if (bossHealthComponent != null)
        {
            bossHealthComponent.OnDamaged -= UpdateHealthUI;
            bossHealthComponent.OnHealed -= UpdateHealthUI;
            bossHealthComponent.OnDeath -= HideBossBar;
        }
    }

}
