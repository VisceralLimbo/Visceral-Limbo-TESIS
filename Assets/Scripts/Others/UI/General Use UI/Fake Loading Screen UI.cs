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

    [SerializeField] private float LastValue;

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
            Dungeon.OnUnSuccessfulGeneration += ResetProgressBar;
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
        // como queda feo que la barra vuelva para atras.
        // decidi poner un  cacheo / chequeo de cual fue el ultimo valor de generacion
        // si el valor entrante es menor que el del cache. entonces no actualizamos nada
        if(LastValue > AddProgress)
        {
            return;
        }
        LastValue = AddProgress;

        progressBar.value = AddProgress;

        int data = (int)(AddProgress * 100);

        progressText.text = data.ToString() + " %";

        print("loading value: " + AddProgress + " Total " + data);
    }

    public void ResetProgressBar()
    {
        LastValue = progressBar.value;

    }

    public void HideLoadingScreen()
    {
        loadingPanel.SetActive(false); 
    }
}
