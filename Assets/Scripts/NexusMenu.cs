using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NexusMenu : MonoBehaviour
{
    [SerializeField] SoundData _ClickSound;
    public void ShopContinue()
    {
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(this.transform.position).play();
        LoadGameScene("Proc Gen Scene");
    }

    public void ShopToMenu()
    {
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(this.transform.position).play();
        LoadGameScene("MainMenu");
    }
    
    public void LoadGameScene(string sceneName)
    {
        // chequeo q este guardado el nobmre de la escena
        LoadingScreenManager.sceneToLoad = sceneName;
        SoundManager.Instance.CreateSound().WithSoundData(_ClickSound).WithPosition(this.transform.position).play();
        // cargo la loadscreen
        SceneManager.LoadScene("LoadingScene");
    }

}
