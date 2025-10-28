using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingScreenManager : MonoBehaviour
{
    // estatica para q mainmenu diga q escena cargar
    public static string sceneToLoad;


    [Header("UI References")]
    public GameObject loadingPanel;
    public Slider progressBar;
    public TMP_Text progressText;

    private void Start()
    {
        // panel true cuando inicio escena (para q se vea la carga, texto, etc)
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            StartCoroutine(LoadSceneAsync());
        }
    }

    IEnumerator LoadSceneAsync()
    {
        yield return null;

        string targetScene = sceneToLoad; // la escena a cargar 
        sceneToLoad = null;              // reinicio la estatica

        if (string.IsNullOrEmpty(targetScene))
        {
            // debug por qsoy bobo xd
            Debug.LogError("ESCRIBISTE MAL EL NOMBRE BOLUDAZO");
            yield break;
        }

        // asincronicaaa jaja odio

        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            // barrita de carga
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);
            int progressPercentage = Mathf.RoundToInt(progressValue * 100f);

            // actualizo
            if (progressBar != null)
            {
                progressBar.value = progressValue;
            }

            // actualizo el texto
            if (progressText != null)
            {
                progressText.text = progressPercentage.ToString() + "%";
            }

            if (operation.progress >= 0.9f)
            {
                //  cuando llega al 90 muestra 100 y que este llena (nunca veia el 100% tonces lo fuerzo, igual la carga termina al 90 basciamente xd)
                if (progressText != null)
                {
                    progressText.text = "100%";
                }
                if (progressBar != null)
                {
                    progressBar.value = 1f;
                }
                operation.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}