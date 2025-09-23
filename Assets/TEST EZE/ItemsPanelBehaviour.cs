using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemsPanelBehaviour : MonoBehaviour
{
    public static ItemsPanelBehaviour Instance;
    [SerializeField] private InventoryManager PlayerInventory;

    private Dictionary<ItemDefinitionSO, GameObject> InventoryDic = new Dictionary<ItemDefinitionSO, GameObject>();

    [SerializeField] private GameObject ItemPrefab;
    [SerializeField] private Transform _Layout;
    // refes para la descripcion
    [Header("item description ui")]
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private void Awake()
    {
        if (Instance == null && Instance != this)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // se activa con el itempickup
    // se maneja creacion y actualizacion del inventario
    public void UpdateUIInventory(ItemDefinitionSO definition)
    {
        if (InventoryDic.ContainsKey(definition))
        {
            // si existe solo actualizo la cantidad
            var itemSlot = InventoryDic[definition];
            var textComponent = itemSlot.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                // saco la cantidad del inventorymanagr
                textComponent.text = PlayerInventory.GetItemStack(definition).ToString();
            }
        }
        else
        {
            // si es nuevo creo el slot y la cantidad actual (1 xq es nuevo xd)
            var stackCount = PlayerInventory.GetItemStack(definition);
            DrawSingleItem(definition, stackCount);
        }
    }

    // se actualiza la ui al abrir el inventario
    public void RefreshInventoryUI()
    {
        // limpio los slots 
        foreach (var slot in InventoryDic.Values)
        {
            Destroy(slot);
        }
        InventoryDic.Clear();

        // itereo en los items q tenga el palyer
        if (PlayerInventory != null)
        {
            foreach (var itemEntry in PlayerInventory.GetInventoryItems())
            {
                // llamo a la funcion para hacer el slot
                DrawSingleItem(itemEntry.Key, PlayerInventory.GetItemStack(itemEntry.Key));
            }
        }
    }

    // dibuja/crea el slot
    public void DrawSingleItem(ItemDefinitionSO definition, int stackCount)
    {
        var itemSlot = Instantiate(ItemPrefab, _Layout);

        // llamo al script para la descripcion
        var descriptionHandler = itemSlot.GetComponent<DescriptionSlotItem>();
        if (descriptionHandler != null)
        {
            //info del item y las refes del description
            descriptionHandler.itemDefinition = definition;
            descriptionHandler.descriptionPanel = descriptionPanel;
            descriptionHandler.descriptionText = descriptionText;
        }

        // pongo el sprite
        var imageComponent = itemSlot.GetComponent<Image>();
        if (imageComponent != null)
        {
            imageComponent.sprite = definition.ItemSprite;
        }

        // texto de la cantidad
        var textComponent = itemSlot.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = stackCount.ToString();
        }

        itemSlot.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);

        InventoryDic.Add(definition, itemSlot);
    }


}
