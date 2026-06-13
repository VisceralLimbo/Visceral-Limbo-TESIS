using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUIPanelManager : MonoBehaviour
{
    [Header("refes para la escenad de main menu")]
    public Slider sensSlider;
    public Slider shakeSlider;
    public Slider hitStopSlider;
    public Slider volumeSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider uiSlider;

    public TMP_Text sensitivityText;
    public TMP_Text shakeText;
    public TMP_Text hitStopText;
    public TMP_Text volumeText;
    public TMP_Text musicText;
    public TMP_Text sfxText;
    public TMP_Text uiText;

    //tuve q separar logica del cs settings aca porque se rompia la carga
    void OnEnable()
    {
        if (CS_settingsUI.Instance != null)
        {
            // cargo los defaults para los sliders
            sensSlider.value = PlayerPrefs.GetFloat(CS_settingsUI.SENSITIVITY_KEY, CS_settingsUI.DEFAULT_SENS);
            shakeSlider.value = PlayerPrefs.GetFloat(CS_settingsUI.SHAKE_KEY, CS_settingsUI.DEFAULT_SHAKE);
            hitStopSlider.value = PlayerPrefs.GetFloat(CS_settingsUI.HITSTOP_KEY, CS_settingsUI.DEFAULT_HITSTOP);
            volumeSlider.value = PlayerPrefs.GetFloat(CS_settingsUI.VOLUME_KEY, CS_settingsUI.DEFAULT_VOLUME);
            musicSlider.value = PlayerPrefs.GetFloat(CS_settingsUI.MUSIC_VOLUME_KEY, CS_settingsUI.DEFAULT_MUSIC_VOLUME);
            sfxSlider.value = PlayerPrefs.GetFloat(CS_settingsUI.SFX_VOLUME_KEY, CS_settingsUI.DEFAULT_SFX_VOLUME);
            uiSlider.value = PlayerPrefs.GetFloat(CS_settingsUI.UI_VOLUME_KEY, CS_settingsUI.DEFAULT_UI_VOLUME);

            // actualizo el texto incial para q no me aparezca el "a" que tenia jeje
            CS_settingsUI.Instance.UpdateTexts(
    sensSlider.value,
    volumeSlider.value,
    shakeSlider.value,
    hitStopSlider.value,
    sensitivityText,
    shakeText,
    hitStopText,
    volumeText
);
            musicText.text = "MÚSICA: " + musicSlider.value.ToString("F1");
            sfxText.text = "EFECTOS: " + sfxSlider.value.ToString("F1");
            uiText.text = "INTERFAZ: " + uiSlider.value.ToString("F1");

            // listeners
            SetupLocalListeners();
        }
    }

    void OnDisable()
    {
        // limpio los listeners en singleton en cada escena basicamente
        if (CS_settingsUI.Instance != null)
        {
            sensSlider.onValueChanged.RemoveListener(CS_settingsUI.Instance.OnSensitivityChanged);
            shakeSlider.onValueChanged.RemoveListener(CS_settingsUI.Instance.OnShakeChanged);
            hitStopSlider.onValueChanged.RemoveListener(CS_settingsUI.Instance.OnHitStopChanged);
            volumeSlider.onValueChanged.RemoveListener(CS_settingsUI.Instance.OnVolumeChanged);
            musicSlider.onValueChanged.RemoveListener(CS_settingsUI.Instance.OnMusicVolumeChanged);
            sfxSlider.onValueChanged.RemoveListener(CS_settingsUI.Instance.OnSFXVolumeChanged);
            uiSlider.onValueChanged.RemoveListener(CS_settingsUI.Instance.OnUIVolumeChanged);
        }
    }

    private void SetupLocalListeners()
    {
        // limpio listeners anteriorers y agrego nuevos
        sensSlider.onValueChanged.RemoveAllListeners();
        shakeSlider.onValueChanged.RemoveAllListeners();
        hitStopSlider.onValueChanged.RemoveAllListeners();
        volumeSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();
        uiSlider.onValueChanged.RemoveAllListeners();

        // sens
        sensSlider.onValueChanged.AddListener(value => {
            CS_settingsUI.Instance.OnSensitivityChanged(value);
            CS_settingsUI.Instance.UpdateTexts(
                value,
                volumeSlider.value,
                shakeSlider.value,
                hitStopSlider.value,
                sensitivityText,
                shakeText,
                hitStopText,
                volumeText
            );
        });
        // shake 
        shakeSlider.onValueChanged.AddListener(value => {
            CS_settingsUI.Instance.OnShakeChanged(value);
            CS_settingsUI.Instance.UpdateTexts(
                sensSlider.value,
                volumeSlider.value,
                value,
                hitStopSlider.value,
                sensitivityText,
                shakeText,
                hitStopText,
                volumeText
            );
        });
        //hitstop
        hitStopSlider.onValueChanged.AddListener(value => {
            CS_settingsUI.Instance.OnHitStopChanged(value);
            CS_settingsUI.Instance.UpdateTexts(
                sensSlider.value,
                volumeSlider.value,
                shakeSlider.value,
                value,
                sensitivityText,
                shakeText,
                hitStopText,
                volumeText
            );
        });
        // volumen
        volumeSlider.onValueChanged.AddListener(value => {
            CS_settingsUI.Instance.OnVolumeChanged(value);
            CS_settingsUI.Instance.UpdateTexts(
                sensSlider.value,
                value,
                shakeSlider.value,
                hitStopSlider.value,
                sensitivityText,
                shakeText,
                hitStopText,
                volumeText
            );
        });

        // volumen music
        musicSlider.onValueChanged.AddListener(value => {
            CS_settingsUI.Instance.OnMusicVolumeChanged(value);
            musicText.text = "MÚSICA: " + value.ToString("F1");
        });

        // volumen sfx
        sfxSlider.onValueChanged.AddListener(value => {
            CS_settingsUI.Instance.OnSFXVolumeChanged(value);
            sfxText.text = "EFECTOS: " + value.ToString("F1");
        });

        // volumen ui
        uiSlider.onValueChanged.AddListener(value => {
            CS_settingsUI.Instance.OnUIVolumeChanged(value);
            uiText.text = "INTERFAZ: " + value.ToString("F1");
        });
    }

    // si limpio los playerprefs no se limpiaban las opciones asi q hice un reset basicamente de eso tmb para q asigne los default 
    public void RefreshUIFromPlayerPrefs()
    {
        if (CS_settingsUI.Instance != null)
        {
            sensSlider.value = PlayerPrefs.GetFloat(CS_settingsUI.SENSITIVITY_KEY, CS_settingsUI.DEFAULT_SENS);
            shakeSlider.value = PlayerPrefs.GetFloat(CS_settingsUI.SHAKE_KEY, CS_settingsUI.DEFAULT_SHAKE);
            hitStopSlider.value = PlayerPrefs.GetFloat(CS_settingsUI.HITSTOP_KEY, CS_settingsUI.DEFAULT_HITSTOP);
            volumeSlider.value = PlayerPrefs.GetFloat(CS_settingsUI.VOLUME_KEY, CS_settingsUI.DEFAULT_VOLUME);

            // actualizo textos
            CS_settingsUI.Instance.UpdateTexts(
                sensSlider.value,
                volumeSlider.value,
                shakeSlider.value,
                hitStopSlider.value,
                sensitivityText,
                shakeText,
                hitStopText,
                volumeText
            );

            // me aseguro de q aplique los cambios
            CS_settingsUI.Instance.ApplySettingsToGame();
        }
    }
}
