using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pauseMenuPanel;   // menu princi
    public GameObject opcionesPanel;    // menu opciones 
    public GameObject abandonPanel;     // menu abandonar
    public GameObject itemsPanel;       // menu items

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
    }

    public void ShowOpciones()
    {
        pauseMenuPanel.SetActive(false);
        opcionesPanel.SetActive(true);
        abandonPanel.SetActive(false);
        itemsPanel.SetActive(false);
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(this.transform.position).play();
        ref_PauseMenu.ResetUIState();
    }

    public void ShowAbandon()
    {
        pauseMenuPanel.SetActive(false);
        opcionesPanel.SetActive(false);
        abandonPanel.SetActive(true);
        itemsPanel.SetActive(false);
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(this.transform.position).play();
        ref_PauseMenu.ResetUIState();
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
}
