using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryUIBehaviour : MonoBehaviour
{
    public static InventoryUIBehaviour Instance;
    [SerializeField] private InventoryManager PlayerInventory; 


    private Dictionary<ItemDefinitionSO, GameObject> InventoryDic=new Dictionary<ItemDefinitionSO, GameObject>();

    [SerializeField] private GameObject ItemPrefab;
    [SerializeField] private Transform _Layout;

    [SerializeField] private GameObject[] PLAYEROBJ;
    private void Awake()
    {
        if(Instance == null && Instance != this)
        {
            Instance = this;
            PLAYEROBJ = GameObject.FindGameObjectsWithTag("Player");

            foreach(GameObject obj in PLAYEROBJ)
            {
                if(obj.TryGetComponent(out InventoryManager manager))
                {
                    PlayerInventory = manager;
                    PlayerInventory.ItemPickUp += UpdateUIInventory;
                    
                }
            }

            /*PlayerInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<InventoryManager>();
            PlayerInventory.ItemPickUp += UpdateUIInventory;
            */
        }
        else
        {
            Destroy(this);
        }
    }

    public void UpdateUIInventory(ItemDefinitionSO Definition)
    {
        if (InventoryDic.ContainsKey(Definition))
        {
            var GMID = InventoryDic[Definition];

            var textGMID = GMID.GetComponentInChildren<TextMeshProUGUI>();
            textGMID.text = PlayerInventory.GetItemStack(Definition).ToString();

        }
        else
        {
            var GMIDPrefab = Instantiate(ItemPrefab,_Layout);
            var textGMID = GMIDPrefab.GetComponentInChildren<TextMeshProUGUI>();
            textGMID.text = "1";

            if(Definition.ItemSprite != null)
            {
                GMIDPrefab.GetComponent<Image>().sprite = Definition.ItemSprite;
            }

            GMIDPrefab.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);

            
            InventoryDic.Add(Definition, GMIDPrefab);
        }



    }



}
