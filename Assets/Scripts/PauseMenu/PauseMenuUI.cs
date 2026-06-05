using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pauseMenuPanel;   // menu princi
    public GameObject opcionesPanel;    // menu opciones 
    public GameObject abandonPanel;     // menu abandonar
    public GameObject itemsPanel;       // menu items
    [SerializeField] private GameObject optionsFirstButton;
    [SerializeField] private GameObject abandonFirstButton;
    [SerializeField] private GameObject itemsFirstButton;
    [SerializeField] private GameObject pauseFirstButton;

    [Header("Refes")]
    public PauseMenu ref_PauseMenu;
    [SerializeField] private ItemsPanelBehaviour itemsUI;

    [SerializeField] SoundData _ClickSound;

    //no hace falta explicar
    public void ShowPauseMenu()
    {
        pauseMenuPanel.SetActive(true);
        opcionesPanel.SetActive(false);
        abandonPanel.SetActive(false);
        itemsPanel.SetActive(false);
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(this.transform.position).play();
        //reseteo la ui por bug en flechitas xd
        ref_PauseMenu.ResetUIState();

        EventSystem.current.SetSelectedGameObject(pauseFirstButton);
    }

    public void ShowOpciones()
    {
        pauseMenuPanel.SetActive(false);
        opcionesPanel.SetActive(true);
        abandonPanel.SetActive(false);
        itemsPanel.SetActive(false);
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(this.transform.position).play();
        ref_PauseMenu.ResetUIState();

        EventSystem.current.SetSelectedGameObject(optionsFirstButton);
    }

    public void ShowAbandon()
    {
        pauseMenuPanel.SetActive(false);
        opcionesPanel.SetActive(false);
        abandonPanel.SetActive(true);
        itemsPanel.SetActive(false);
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(this.transform.position).play();
        ref_PauseMenu.ResetUIState();

        EventSystem.current.SetSelectedGameObject(abandonFirstButton);
    }

    public void ShowItems()
    {
        pauseMenuPanel.SetActive(false);
        opcionesPanel.SetActive(false);
        abandonPanel.SetActive(false);
        itemsPanel.SetActive(true);
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(this.transform.position).play();
        ref_PauseMenu.ResetUIState();
        // redibujo ui
        itemsUI.RefreshInventoryUI();
        EventSystem.current.SetSelectedGameObject(itemsFirstButton);
    }

    public void BackToPauseMenu()
    {
        ShowPauseMenu();
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(this.transform.position).play();
    }

    public void Abandonn()
    {
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(this.transform.position).play();
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartProcGen()
    {
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(this.transform.position).play();
        SceneManager.LoadScene("Proc Gen Scene");
    }

    public void OnClickItemsTab()
    {
        TutorialController.Instance?.OnItemsTabOpened();
    }
}
