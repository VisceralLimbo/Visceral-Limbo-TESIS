using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pauseMenuPanel;   // menu princi
    public GameObject opcionesPanel;    // menu opciones 
    public GameObject abandonPanel;     // menu abandonar

    //no hace falta explicar
    public void ShowPauseMenu()
    {
        pauseMenuPanel.SetActive(true);
        opcionesPanel.SetActive(false);
        abandonPanel.SetActive(false);
    }

    public void ShowOpciones()
    {
        pauseMenuPanel.SetActive(false);
        opcionesPanel.SetActive(true);
        abandonPanel.SetActive(false);
    }

    public void ShowAbandon()
    {
        pauseMenuPanel.SetActive(false);
        opcionesPanel.SetActive(false);
        abandonPanel.SetActive(true);
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
