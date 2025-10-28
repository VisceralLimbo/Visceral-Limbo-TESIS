using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FakeLoadingScreenUI : MonoBehaviour
{


    [Header("UI References")]
    public GameObject loadingPanel;
    public Slider progressBar;
    public TMP_Text progressText;

    private void Start()
    {
        // panel true cuando inicio escena (para q se vea la carga, texto, etc)
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }
        DungeonGenerator Dungeon = FindObjectOfType<DungeonGenerator>();
        if(Dungeon != null)
        {
            Dungeon.GenerationValue += AddProgressBar;
            Dungeon.OnSuccessfulGeneration += HideLoadingScreen;
        }

    }


    public void SetProgressBar(float NewProgress)
    {

        float progressValue = Mathf.Clamp01(NewProgress);
        float ProgressInt = progressValue * 100;

        progressText.text = ProgressInt.ToString() + " %";
        progressBar.value = progressValue;
    }

    public void AddProgressBar(float AddProgress)
    {
        var Current = progressBar.value;

        var Extra = Mathf.Clamp01(AddProgress);

        progressBar.value = Current + Extra;

        var data = progressBar.value * 100;

        progressText.text = data.ToString() + " %";
    }

    public void ResetProgressBar()
    {
        progressBar.value = 0;
        progressText.text+= "0 %";
    }

    public void HideLoadingScreen()
    {
        loadingPanel.SetActive(false); 
    }
}
