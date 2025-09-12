using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    private bool isPaused = false;
    [SerializeField] private Player_Base playerBase; // ref al player base para traer el _Player_InputActions (no lo tienen en la jerarquia, lo llaman en playerbase entonces tuve que hacer esto xd)

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame && !isPaused)
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        //  desactivo los inputs cuando esta en pausa el juego
        if (playerBase != null)
        {
            playerBase._Player_InputActions.Gameplay.Disable();
        }
    }


    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // habilito los inputs cuando vuelvo
        if (playerBase != null)
        {
            playerBase._Player_InputActions.Gameplay.Enable();
        }

        ResetUIState();
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

    //ataque cuando cierro menu de pausa (corregir) CORREGIDO
    //si habilitaba y desactivaba los inputs (por el nuevo sistema) como que se guardaba un ataque y se hacia solo al resumir aunque no toques click
}
