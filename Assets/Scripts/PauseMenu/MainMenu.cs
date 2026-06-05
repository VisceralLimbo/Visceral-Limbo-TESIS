using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject buttonsHide;

    [SerializeField] SoundData _ClickSound;

    [SerializeField] private GameObject menuFirstButton;
    [SerializeField] private GameObject optionsFirstButton;

    private void Awake()
    {
        optionsPanel.SetActive(false);
        buttonsHide.SetActive(true);
    }

    public void Start()
    {
        EventSystem.current.SetSelectedGameObject(menuFirstButton);
    }

    public void MainMenuShop()
    {
        LoadGameScene("Nexus");
        PlayClick();
    }
    public void MainMenuOptions()
    {
        optionsPanel.SetActive(true);
        buttonsHide.SetActive(false);
        PlayClick();

        EventSystem.current.SetSelectedGameObject(optionsFirstButton);
    }

    public void MainMenuBack()
    {
        optionsPanel.SetActive(false);
        buttonsHide.SetActive(true);
        PlayClick();
        EventSystem.current.SetSelectedGameObject(menuFirstButton);
    }

    public void MainMenuGenProc()
    {
        PlayClick();
        if (!TutorialManager.HasSeenTutorial())
        {
            LoadGameScene("TutorialScene");
        }
        else
        {
            LoadGameScene("Proc Gen Scene");
        }
    }

    public void MainMenuCloseGame()
    {
        PlayClick();
        Application.Quit();
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
        PlayClick();

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

    private void PlayClick()
    {
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(transform.position).play();
    }
}
