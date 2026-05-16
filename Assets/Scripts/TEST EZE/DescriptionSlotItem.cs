using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DescriptionSlotItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // refe al so se asigna cuando se instancia
    public ItemDefinitionSO itemDefinition;


    [Header("refes")]
    [SerializeField] public GameObject descriptionPanel; //panel de descripcion
    [SerializeField] public TextMeshProUGUI descriptionText; //texto
    private ItemsPanelBehaviour itemsPanelBehaviour; // refe al panel de items
    private InventoryManager inventoryManager; // refe para los stacks
    private RectTransform slotRectTransform; // ref del recttransform del slot por el q paso para saber donde pongo la descripcion

    [Header("fade duracion")]
    [SerializeField] private float fadeDuration; // duracion del fade

    [Header("slot scale config")]
    [SerializeField] private float hoverScale; // lo q va a escalar cuando pase el mouse por encima
    [SerializeField] private float scaleDuration; // duracion de la anim
    private Vector3 originalScale;

    private Coroutine scaleCoroutine; // refe para corrutina de escalado


    private void Awake()
    {
        // seteo inventoymnager
        inventoryManager = FindObjectOfType<InventoryManager>();
        // rect transform del slot
        slotRectTransform = GetComponent<RectTransform>();

        // agarro refe
        itemsPanelBehaviour = FindObjectOfType<ItemsPanelBehaviour>();

        // guardo escala original del slot
        originalScale = transform.localScale;

    }

    // anim de escalado
    private IEnumerator ScaleSlot(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float time = 0;

        while (time < scaleDuration)
        {
            time += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(startScale, targetScale, time / scaleDuration); //lerp para suavizar
            yield return null;
        }

        transform.localScale = targetScale;
        scaleCoroutine = null; // limpio refe cuando termina
    }

    // me paro sobre el item y activo
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemDefinition != null && descriptionText != null && inventoryManager != null)
        {
            // limpio y animo escalado
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
                transform.localScale = originalScale;
            }
            scaleCoroutine = StartCoroutine(ScaleSlot(originalScale * hoverScale));

            //todo el choclo de texto se arma aca
            int stackCount = inventoryManager.GetItemStack(itemDefinition);
            float totalBenefit = itemDefinition.buffvalueperstack * stackCount;
            string descriptionContent = $"{itemDefinition.ItemDescription}\n\n";
            descriptionContent += $"+{itemDefinition.buffvalueperstack} DE {itemDefinition.buff} POR STACK";
            descriptionContent += $" {totalBenefit} EN TOTAL";

            //para asignar el lugar donde va a spawnear el panel 
            RectTransform slotRect = GetComponent<RectTransform>();
            float xOffset = slotRect.rect.width / 4f;
            float yOffset = -slotRect.rect.height / 7f;
            Vector3 newPosition = slotRect.position + new Vector3(xOffset, yOffset, 0);

            // fade
            if (itemsPanelBehaviour != null)
            {
                // pasa por, la descripcion y duracion de fade
                itemsPanelBehaviour.ShowDescription(newPosition, descriptionContent, fadeDuration);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // anim de escalado
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            transform.localScale = originalScale * hoverScale;
        }
        scaleCoroutine = StartCoroutine(ScaleSlot(originalScale));

        // fade out
        if (itemsPanelBehaviour != null)
        {
            itemsPanelBehaviour.HideDescription(fadeDuration);
        }
    }
}