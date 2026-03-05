using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthVisualEffects : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health_Component healthComponent;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private RectTransform barTransform;

    [Header("Shader Property")]
    [SerializeField] private string colorProperty = "_HealthColor1";

    [Header("Frame")]
    [SerializeField] private RectTransform frameTransform;
    [SerializeField] private RectTransform frameBackgroundTransform;

    private Vector3 frameOriginalScale;
    private Vector3 frameBackgroundOriginalScale;

    [Header("Colors")]
    [SerializeField] private Color fullColor = Color.green;
    [SerializeField] private Color midColor = Color.yellow;
    [SerializeField] private Color lowColor = Color.red;

    [Header("Damage Punch")]
    [SerializeField] private float punchScale = 1.15f;
    [SerializeField] private float punchDuration = 0.15f;

    [Header("Critical Pulse")]
    [SerializeField] private float criticalThreshold = 0.2f;
    [SerializeField] private float pulseSpeed = 6f;
    [SerializeField] private float pulseAmount = 1.1f;

    private Material runtimeMaterial;
    private Vector3 originalScale;
    private bool isCritical;
    private float maxHealth;

    [SerializeField] private float yellowThreshold;
    [SerializeField] private float redThreshold;

    private void Awake()
    {
        runtimeMaterial = Instantiate(healthFillImage.material);
        healthFillImage.material = runtimeMaterial;

        originalScale = barTransform.localScale;

        frameOriginalScale = frameTransform.localScale;

        frameBackgroundOriginalScale = frameBackgroundTransform.localScale;
    }

    private void Start()
    {
        if (healthComponent == null)
            healthComponent = FindObjectOfType<Health_Component>();

        maxHealth = healthComponent.MaxHealth;

        healthComponent.OnDamaged += HandleHealthChanged;
        healthComponent.OnHealed += HandleHealthChanged;
        healthComponent.OnDamaged += PlayDamagePunch;

        UpdateVisuals();
    }

    private void Update()
    {
        if (isCritical)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * (pulseAmount - 1f);

            barTransform.localScale = originalScale * pulse;
            frameTransform.localScale = frameOriginalScale * pulse;
            frameBackgroundOriginalScale = frameBackgroundOriginalScale * pulse;
        }
    }

    private void HandleHealthChanged()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        float normalized = healthComponent.CurrentHealth / maxHealth;

        UpdateColor(normalized);

        isCritical = normalized <= criticalThreshold;

        if (!isCritical)
        {
            barTransform.localScale = originalScale;
            frameTransform.localScale = frameOriginalScale;
            frameBackgroundTransform.localScale = frameBackgroundOriginalScale;
        }
    }

    private void UpdateColor(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);

        Color target;

        if (normalized >= yellowThreshold)
        {
            target = fullColor;
        }
        else if (normalized > redThreshold)
        {
            float t = Mathf.InverseLerp(yellowThreshold, redThreshold, normalized);
            target = Color.Lerp(midColor, fullColor, t);
        }
        else
        {
            
            target = lowColor;
        }

        runtimeMaterial.SetColor(colorProperty, target);
    }

    private void PlayDamagePunch()
    {
        StopAllCoroutines();
        StartCoroutine(PunchCoroutine());
    }

    private System.Collections.IEnumerator PunchCoroutine()
    {
        barTransform.localScale = originalScale * punchScale;
        frameTransform.localScale = frameOriginalScale * punchScale;
        frameBackgroundTransform.localScale = frameBackgroundOriginalScale * punchScale;
        yield return new WaitForSeconds(punchDuration);

        if (!isCritical)
        {
            barTransform.localScale = originalScale;
            frameTransform.localScale = frameOriginalScale;
            frameBackgroundTransform.localScale = frameBackgroundOriginalScale;
        }
    }

    private void OnDestroy()
    {
        if (healthComponent != null)
        {
            healthComponent.OnDamaged -= HandleHealthChanged;
            healthComponent.OnHealed -= HandleHealthChanged;
            healthComponent.OnDamaged -= PlayDamagePunch;
        }
    }
}