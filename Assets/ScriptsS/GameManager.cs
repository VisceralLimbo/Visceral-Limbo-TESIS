using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
    public void PlayGame()
    {
        Debug.Log("empezar juego");
        SceneManager.LoadScene(1);
    }
    public void Options(GameObject option)
    {
        option.SetActive(true);
    }
    public void BackMainMenu(GameObject option)
    {
        option.SetActive(false);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
