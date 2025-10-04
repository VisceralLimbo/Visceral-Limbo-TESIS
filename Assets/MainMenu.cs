using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject buttonsHide;

    private void Awake()
    {
        optionsPanel.SetActive(false);
        buttonsHide.SetActive(true);
        ResetUIState();
    }

    public void MainMenuPlayGame()
    {
        LoadTargetScene("GameScene");
        ResetUIState();
    }

    public void MainMenuOptions()
    {
        optionsPanel.SetActive(true);
        buttonsHide.SetActive(false);
        ResetUIState();
    }

    public void MainMenuBack()
    {
        optionsPanel.SetActive(false);
        buttonsHide.SetActive(true);
        ResetUIState();
    }

    public void MainMenuGenProc()
    {
        LoadTargetScene("Proc Gen Scene");
        ResetUIState();
    }

    public void MainMenuCloseGame()
    {
        Application.Quit();
    }

    // limpio seleccion y flechitas
    public void ResetUIState()
    {
        ArrowsUI arrowSelector = FindObjectOfType<ArrowsUI>();
        if (arrowSelector != null)
            arrowSelector.HideArrows();

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    // parte del loadscene
    private void LoadTargetScene(string targetSceneName)
    {
        // target
        LoadingScreenManager.sceneToLoad = targetSceneName;

        // cargo escena de carga
        SceneManager.LoadScene("LoadingScene");
        ResetUIState();
    }

}
