using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class DescriptionSlotItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // refe al so se asigna cuando se instancia
    public ItemDefinitionSO itemDefinition;

    // refe del panel y texto para la descripcion se asignada desde el script itempanelbehaviour
    [SerializeField] public GameObject descriptionPanel;
    [SerializeField] public TextMeshProUGUI descriptionText;

    // refe para los stacks
    private InventoryManager inventoryManager;

    private void Awake()
    {
        // seteo inventoymnager
        inventoryManager = FindObjectOfType<InventoryManager>();
    }

    // me paro sobre el item y activo
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemDefinition != null && descriptionText != null && inventoryManager != null)
        {
            // calculo el buffo total dependiendo los stacks q tenga
            int stackCount = inventoryManager.GetItemStack(itemDefinition);
            float totalBenefit = itemDefinition.buffvalueperstack * stackCount;

            // todo el choclo de texto se arma aca
            string descriptionContent = $"";
            descriptionContent += $"{itemDefinition.ItemDescription}\n\n";
            descriptionContent += $"+{itemDefinition.buffvalueperstack} de {itemDefinition.buff} por stack";
            descriptionContent += $" ({totalBenefit} en total)";

            descriptionText.text = descriptionContent;
            descriptionPanel.SetActive(true);
        }
    }

    // saco el mouse del item y desactivo
    public void OnPointerExit(PointerEventData eventData)
    {
        // panel off
        descriptionPanel.SetActive(false);
    }
}