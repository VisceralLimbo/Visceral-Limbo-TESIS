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
        LoadGameScene("GameScene");
        ResetUIState();
    }

    public void MainMenuShop()
    {
        LoadGameScene("Nexus");
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
        //hay q cambiarle el nobmre a esto me comi los espacios y las mayusxd
        LoadGameScene("Proc Gen Scene");
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

    public void ClearPlayerRef()
    {
        // borro todo lo guardado
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save(); // guardo lo borrado

        // CS_settingsUI crea los valores default de nuevo

        if (CS_settingsUI.Instance != null)
        {
            CS_settingsUI.Instance.LoadSettings();
        }

        // fuerzo q las opciones se recargen
        SettingsUIPanelManager uiManager = FindObjectOfType<SettingsUIPanelManager>();

        if (uiManager != null)
        {
            // llamo al refresh
            uiManager.RefreshUIFromPlayerPrefs();
        }
    }

    public void LoadGameScene(string sceneName)
    {
        // chequeo q este guardado el nobmre de la escena
        LoadingScreenManager.sceneToLoad = sceneName;

        // cargo la loadscreen
        SceneManager.LoadScene("LoadingScene");
    }

}
