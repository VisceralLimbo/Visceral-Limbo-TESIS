using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pauseMenuPanel;   // menu princi
    public GameObject opcionesPanel;    // menu opciones 
    public GameObject abandonPanel;     // menu abandonar

    [Header("Refes")]
    public PauseMenu ref_PauseMenu;

    //no hace falta explicar
    public void ShowPauseMenu()
    {
        pauseMenuPanel.SetActive(true);
        opcionesPanel.SetActive(false);
        abandonPanel.SetActive(false);

        //reseteo la ui por bug en flechitas xd
        ref_PauseMenu.ResetUIState();
    }

    public void ShowOpciones()
    {
        pauseMenuPanel.SetActive(false);
        opcionesPanel.SetActive(true);
        abandonPanel.SetActive(false);

        ref_PauseMenu.ResetUIState();
    }

    public void ShowAbandon()
    {
        pauseMenuPanel.SetActive(false);
        opcionesPanel.SetActive(false);
        abandonPanel.SetActive(true);

        ref_PauseMenu.ResetUIState();
    }

    public void BackToPauseMenu()
    {
        ShowPauseMenu();
    }

    public void OnQuit()
    {
        Application.Quit();
    }
}
