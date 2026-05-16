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

    [Header("Gulag Local System")]
    public Vector3 posAntesDeMorir;
    public bool AlreadyUseGulag = false; // chequeo si ya uso la chance

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

    // cuando muere llamo a esto para guardar la pos
    public void DeathPosition(Vector3 posicion)
    {
        posAntesDeMorir = posicion;
        AlreadyUseGulag = true; // se uso el gulag
    }

    public void AdvanceLevel()
    {
        currentLevel++;
        AlreadyUseGulag = false; // para q no tenga otra chance de gulag al avanzar de nivel
    }

    public void GoToNexus() => StartCoroutine(WaitAndLoad("Nexus"));

    private IEnumerator WaitAndLoad(string sceneName)
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(sceneName);
    }

    public void SavePlayerInventory(Dictionary<ItemDefinitionSO, ItemLogic> currentInventory)
    {
        savedInventory.Clear();
        foreach (var pair in currentInventory)
        {
            if (pair.Key != null && pair.Value != null)
            {
                savedInventory.Add(new SavedItem { definition = pair.Key, stacks = pair.Value.ItemStacks });
            }
        }
    }
}