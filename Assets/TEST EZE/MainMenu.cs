using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject buttonsHide;

    [SerializeField] SoundData _ClickSound;

    private void Awake()
    {
        optionsPanel.SetActive(false);
        buttonsHide.SetActive(true);
        ResetUIState();
    }

    public void Start()
    {
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(this.transform.position).play();
    }

    public void MainMenuPlayGame()
    {
        LoadGameScene("GameScene");
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(this.transform.position).play();
        ResetUIState();
    }

    public void MainMenuShop()
    {
        LoadGameScene("Nexus");
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(this.transform.position).play();
        ResetUIState();
    }
    public void MainMenuOptions()
    {
        optionsPanel.SetActive(true);
        buttonsHide.SetActive(false);
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(this.transform.position).play();
        ResetUIState();
    }

    public void MainMenuBack()
    {
        optionsPanel.SetActive(false);
        buttonsHide.SetActive(true);
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(this.transform.position).play();
        ResetUIState();
    }

    public void MainMenuGenProc()
    {
        //hay q cambiarle el nobmre a esto me comi los espacios y las mayusxd
        LoadGameScene("Proc Gen Scene");
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(this.transform.position).play();
        ResetUIState();
    }

    public void MainMenuCloseGame()
    {
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(this.transform.position).play();
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
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(this.transform.position).play();

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
