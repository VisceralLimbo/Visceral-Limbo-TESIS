using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopNexus : MonoBehaviour
{
    public string UpgradeKey; // escribir bien la key ej "Upgrade_TestItemLogic"
    public int Cost;          // el valor
    public TMP_Text StatusText;
    public Button BuyButton;

    void Start()
    {
        BuyButton.onClick.AddListener(OnBuyClicked);
        UpdateUI();
    }

    void OnEnable()
    {
        // suscribo el boton al evento de currencychange
        MetaProgressionManager.Instance.OnCurrencyChanged += OnGlobalCurrencyChanged;
    }

    void OnDisable()
    {
        // desuscribo para no cagarla si el objeto se destruye o se desactiva
        if (MetaProgressionManager.Instance != null)
        {
            MetaProgressionManager.Instance.OnCurrencyChanged -= OnGlobalCurrencyChanged;
        }
    }

    // se llama cada que la currency cambia
    private void OnGlobalCurrencyChanged(float newTotal, float changeAmount)
    {
        // actualizo ui (texto)
        UpdateUI();
    }

    public void OnBuyClicked()
    {
        if (MetaProgressionManager.Instance.TryBuyUpgrade(UpgradeKey, Cost))
        {
            // compre bien
            UpdateUI();
        }
        else
        {
            // no tengo plata
            Debug.Log("No tenes suficientes ecos cristalizados.");
        }
    }

    public void UpdateUI()
    {
        float currentCurrency = MetaProgressionManager.Instance.GetCurrency();

        if (MetaProgressionManager.Instance.IsUpgradeBought(UpgradeKey))
        {
            // q diga comprado cuando lo compre
            StatusText.text = "¡COMPRADO!";
            BuyButton.interactable = false;
        }
        else
        {
            // q diga el costo y las monedas que tengo
            StatusText.text = $"Costo: {Cost:F0} Monedas. Tienes: {currentCurrency}";

            // chequeo si puedo pagar para deshabilitar si no
            BuyButton.interactable = currentCurrency >= Cost;
        }
    }
}
