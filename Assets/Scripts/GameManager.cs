using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Progression")]
    public int currentLevel = 1;

    [Header("Inventory Persistence")]
    public List<SavedItem> savedInventory = new List<SavedItem>();

    [System.Serializable]
    public class SavedItem
    {
        public ItemDefinitionSO definition;
        public int stacks;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AdvanceLevel()
    {
        currentLevel++;
    }

    public void GoToNexus()
    {
        StartCoroutine(WaitAndLoad());
    }

    private IEnumerator WaitAndLoad()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("Nexus");
    }

    public void SavePlayerInventory(Dictionary<ItemDefinitionSO, ItemLogic> currentInventory)
    {
        savedInventory.Clear();
        foreach (var pair in currentInventory)
        {
            // chequeo q sea valido antes de guardar
            if (pair.Key != null && pair.Value != null)
            {
                savedInventory.Add(new SavedItem
                {
                    definition = pair.Key,
                    stacks = pair.Value.ItemStacks // uso ItemStacks que es la variable en ItemLogic
                });
            }
        }
    }
}