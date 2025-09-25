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

    // ref del recttransform del slot por el q paso para saber donde pongo la descripcion
    private RectTransform slotRectTransform;

    private void Awake()
    {
        // seteo inventoymnager
        inventoryManager = FindObjectOfType<InventoryManager>();
        // rect transform del slot
        slotRectTransform = GetComponent<RectTransform>();
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

        //para hacer q el cuadro aparezca abajo a la dercha del slot
        // agarro el recttransofmr
        RectTransform descriptionRect = descriptionPanel.GetComponent<RectTransform>();

        // ubicio abajo a la derecha del slot
        float xOffset = slotRectTransform.rect.width / 4f; //fue prueba y error pero se divide para que se ubique bien si no aparece mal posicionado
        float yOffset = -slotRectTransform.rect.height / 7f; //fue prueba y error pero se divide para que se ubique bien si no aparece mal posicionado
        Vector3 offset = new Vector3(xOffset, yOffset, 0);

        // sumo la pos del slot mas el offset de arriba
        descriptionRect.position = slotRectTransform.position + offset;

        // activo cuando estoy encima
        descriptionPanel.SetActive(true);
    }

    // saco el mouse del item y desactivo
    public void OnPointerExit(PointerEventData eventData)
    {
        // panel off
        descriptionPanel.SetActive(false);
    }
}