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
        // carga asincronica
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);

        // me quedo en la pantalla hasta q llegue al 100
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            // muetsro el progreso en la barra
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            // actualizo barra
            if (progressBar != null)
                progressBar.value = progress;

            if (progressText != null)
                progressText.text = Mathf.Round(progress * 100f) + "%";

            yield return null;

            // si esta completa activo escena
            if (operation.progress >= 0.9f)
            {
                // fuerzo q este un toque en la de carga porque el proc gen carga de una y ni ideacomo conectar las cosas para que no se inicie hasta este hecha la generacion xd
                yield return new WaitForSeconds(0.5f);
                operation.allowSceneActivation = true;
            }
        }
    }
}