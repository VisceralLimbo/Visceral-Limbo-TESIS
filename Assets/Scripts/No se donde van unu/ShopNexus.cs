using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopNexus : MonoBehaviour
{
    [Header("Item")]
    public ItemDefinitionSO ItemToSell;
    public int Cost;

    [Header("UI")]
    public TMP_Text StatusText;
    public Button BuyButton;

    [SerializeField] SoundData _ClickSound;

    private void Start()
    {
        BuyButton.onClick.AddListener(OnBuyClicked);
        UpdateUI();
    }

    private void OnDestroy()
    {
        if (BuyButton != null)
        {
            BuyButton.onClick.RemoveListener(OnBuyClicked);
        }
    }

    private void OnEnable()
    {
        BloodEchoesManager.OnBloodEchoesChanged += OnCurrencyChanged;
    }

    private void OnDisable()
    {
        BloodEchoesManager.OnBloodEchoesChanged -= OnCurrencyChanged;
    }

    private void OnCurrencyChanged(float newTotal, float changeAmount)
    {
        UpdateUI();
    }

    public void OnBuyClicked()
    {
        if (ItemToSell == null)
        {
            Debug.LogError("ShopNexus: no hay ningún ItemDefinitionSO asignado");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("ShopNexus: no hay gamemanager");
            return;
        }

        // chequeo si puedo pagar
        if (!BloodEchoesManager.CanPurchase(Cost))
        {
            Debug.Log("No tenes Blood Echoes suficientes");
            UpdateUI();
            return;
        }

        // guardo el item para cargarlo cuando empieza proc gen
        GameManager.Instance.AddItemToSavedInventory(ItemToSell);

        SoundManager.Instance.CreateSound()
            .WithSoundData(_ClickSound)
            .WithPosition(transform.position)
            .play();

        UpdateUI();

        Debug.Log($"Compraste {ItemToSell.ItemName} por {Cost} Blood Echoes.");
    }

    public void UpdateUI()
    {
        if (ItemToSell == null || StatusText == null || BuyButton == null)
            return;

        float currentCurrency = BloodEchoesManager.BloodEchoes;

        StatusText.text =
            $"Costo: {Cost} Blood Echoes. Tenés: {currentCurrency:F0}";

        BuyButton.interactable = currentCurrency >= Cost;
    }
}
