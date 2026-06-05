using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    private bool isPaused = false;
    [SerializeField] private Player_Base playerBase; // ref al player base para traer el _Player_InputActions (no lo tienen en la jerarquia, lo llaman en playerbase entonces tuve que hacer esto xd)
    [SerializeField] private PauseMenuUI _pauseMenuUI;
    [SerializeField] private GameObject firstSelectedButton;
    public GameObject description;
    [SerializeField] private DungeonGenerator dungeonGenerator;

    [SerializeField] SoundData _ClickSound;

    private void Awake()
    {
        // fuerzo el timescale porque se rompia al cargar escena
        Time.timeScale = 1.0f;
    }

    void Update()
    {
        //lo planteo mas como un volver para atras que cerra desde cualquier punto mas q nada para no hacer cagada mas adelante si se meten mas cosas

        // chequeo escape
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {

            // eveitar pausa si esta generando
            if (dungeonGenerator != null &&
                !dungeonGenerator.HasFinishedGeneration())
            {
                return;
            }

            TutorialController.Instance?.OnInventoryOpened();
            // si estaba pausado o nop
            if (!isPaused)
            {
                PauseGame();
            }
            else // si ya esta pausado voy a cerrar el menu
            {
                // vuelvo al menu primero y dsp resumo
                if (!_pauseMenuUI.pauseMenuPanel.activeSelf)
                {
                    //medio hardcodeado pero si el mouse quedaba encima del item y resumia con escape la descripcion se quedaba en pantalla 
                    description.SetActive(false);
                    _pauseMenuUI.BackToPauseMenu();
                }
                else
                {
                    ResumeGame();
                }
            }
        }
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(this.transform.position).play();
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        //  desactivo los inputs cuando esta en pausa el juego
        if (playerBase != null)
        {
            playerBase._Player_InputActions.Gameplay.Disable();
        }

        // pausa q inice con boton seleccionado
        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }


    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(this.transform.position).play();
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
