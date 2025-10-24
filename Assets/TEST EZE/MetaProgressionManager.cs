using System;
using UnityEngine;

public class MetaProgressionManager : MonoBehaviour
{
    public static MetaProgressionManager Instance;

    // las keys de cada playerpref q vamos a querer
    public const string KEY_CURRENCY = "MetaCurrency"; // la currency 
    public const string KEY_HEALTH_UP_BOUGHT = "Upgrade_TestItemLogic";     // vida
    public const string KEY_DAMAGE_UP_BOUGHT = "Upgrade_Damage";            // damage

    // hack con 9 para darnos ecos cristalizados xd
    private const float HACK_AMOUNT = 1000f; // la cantidad q nos da
    private const KeyCode HACK_KEY = KeyCode.Alpha9; // el 9 para q nos de :v

    // mejora de hp basada en el testitemlogic asi q daria 25 de vida
    public const float PERM_MAX_HEALTH_BOOST = 25f;
    // mejora del dmg 
    public const float PERM_BASE_DAMAGE_BOOST = 2f;

    // evento para q la ui sepa cuando actualiza la currency
    public event Action<float, float> OnCurrencyChanged; // total, cambio

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        OnCurrencyChanged?.Invoke(GetCurrency(), 0f);
    }

    private void Update()
    {
        //hack 
        if (Input.GetKeyDown(HACK_KEY))
        {
            AddCurrency(HACK_AMOUNT);
            Debug.Log($"hack {HACK_AMOUNT} ecos cristalizados añadidos. total: {GetCurrency()}");
        }
    }

    // ecos cristalizados
    public float GetCurrency()
    {
        // devuelve el float de la currency
        return PlayerPrefs.GetFloat(KEY_CURRENCY, 0f);
    }

    /// <summary>
    /// Añade ecos cristalizados y guarda el progreso.
    /// </summary>
    public void AddCurrency(float amount)
    {
        float current = GetCurrency();

        PlayerPrefs.SetFloat(KEY_CURRENCY, current + amount);
        PlayerPrefs.Save();

        // evento para la ui (por ahora los botones)
        OnCurrencyChanged?.Invoke(GetCurrency(), amount);
    }

    /// <summary>
    /// Intenta comprar una mejora. Verifica costo, resta los ecos cristalizados, guarda la mejora y los ecos cristalizados.
    /// </summary>
    public bool TryBuyUpgrade(string upgradeKey, float cost)
    {
        // si ya este comprado
        if (IsUpgradeBought(upgradeKey)) return true;

        float currentCurrency = GetCurrency();

        // si tengo los ecos
        if (currentCurrency >= cost)
        {
            // hago la compra y guardo la mejora y los ecos q me queden
            PlayerPrefs.SetInt(upgradeKey, 1);
            PlayerPrefs.SetFloat(KEY_CURRENCY, currentCurrency - cost);
            PlayerPrefs.Save();

            // aviso a la ui
            OnCurrencyChanged?.Invoke(GetCurrency(), -cost);

            Debug.Log($"Mejora {upgradeKey} comprada. ecos cristalizados restantes: {GetCurrency()}");
            return true;
        }

        // no tengo plata :v dsp seria como en el cofre diciendo q no tenes
        Debug.Log($"no me alcanza para {upgradeKey}. necesito {cost}.");
        return false;
    }

    /// <summary>
    /// Chequea si una mejora ya fue comprada.
    /// </summary>
    public bool IsUpgradeBought(string upgradeKey)
    {
        return PlayerPrefs.GetInt(upgradeKey, 0) == 1;
    }
}