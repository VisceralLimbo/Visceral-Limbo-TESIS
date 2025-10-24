using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NexusMenu : MonoBehaviour
{
    public void ShopContinue()
    {
        LoadGameScene("Proc Gen Scene");
    }

    public void ShopToMenu()
    {
        LoadGameScene("MainMenu");
    }
    
    public void LoadGameScene(string sceneName)
    {
        // chequeo q este guardado el nobmre de la escena
        LoadingScreenManager.sceneToLoad = sceneName;

        // cargo la loadscreen
        SceneManager.LoadScene("LoadingScene");
    }

}
