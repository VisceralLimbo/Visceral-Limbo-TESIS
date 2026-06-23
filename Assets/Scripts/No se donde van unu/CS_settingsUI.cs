using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class CS_settingsUI : MonoBehaviour
{
    public static CS_settingsUI Instance; // st

    // playerprefs para guardar valores del slider y pasarlos en la escena ingame
    public const string SENSITIVITY_KEY = "GameSensitivity";
    public const string SHAKE_KEY = "CameraShakeIntensity";
    public const string HITSTOP_KEY = "HitStopDuration";
    public const string VOLUME_KEY = "Volume";
    public const string MUSIC_VOLUME_KEY = "MusicVolume";
    public const string SFX_VOLUME_KEY = "SFXVolume";
    public const string UI_VOLUME_KEY = "UIVolume";

    public const float DEFAULT_SENS = 5f;
    public const float DEFAULT_SHAKE = 1.4f;
    public const float DEFAULT_HITSTOP = 1.6f;
    public const float DEFAULT_VOLUME = 0.5f;
    public const float DEFAULT_MUSIC_VOLUME = 0.5f;
    public const float DEFAULT_SFX_VOLUME = 0.5f;
    public const float DEFAULT_UI_VOLUME = 0.5f;

    [Header("refes")]
    public Player_CameraController cameraController;    
    public CameraShake cameraShake;

    [Header("HitStop Opciones")]
    // las opciones q tienen en el Stop del slowmotion
    public float slowdownFactor = 0.2f;
    public bool affectAudio = true;

    // refe del mixer
    [Header("audio Mixer")]
    public AudioMixer masterMixer;
    private const string MASTER_VOLUME_PARAM = "VolumeMaster"; // nombre q expuse en el mixer
    private const string MUSIC_VOLUME_PARAM = "VolumeMusic";
    private const string SFX_VOLUME_PARAM = "VolumeSFX";
    private const string UI_VOLUME_PARAM = "VolumeUI";


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // lo mantengo
            LoadSettings(); // cargo valaores
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // sollo aplico
        ApplySettingsToGame();
    }

    public void LoadSettings()
    {
        //cargo las default para la primera vez

        if (!PlayerPrefs.HasKey(SENSITIVITY_KEY))
            PlayerPrefs.SetFloat(SENSITIVITY_KEY, DEFAULT_SENS);

        if (!PlayerPrefs.HasKey(SHAKE_KEY))
            PlayerPrefs.SetFloat(SHAKE_KEY, DEFAULT_SHAKE);

        if (!PlayerPrefs.HasKey(HITSTOP_KEY))
            PlayerPrefs.SetFloat(HITSTOP_KEY, DEFAULT_HITSTOP);

        if (!PlayerPrefs.HasKey(VOLUME_KEY))
            PlayerPrefs.SetFloat(VOLUME_KEY, DEFAULT_VOLUME);

        PlayerPrefs.Save();
    }

    // inicio y conecto sliders y textos

    public void InitializeUI(Slider sens, Slider shake, Slider hitStop, Slider vol, TMP_Text sensT, TMP_Text shakeT, TMP_Text hitStopT, TMP_Text volT)
    {
        // aplico vlaores a sliders
        sens.value = PlayerPrefs.GetFloat(SENSITIVITY_KEY, DEFAULT_SENS);
        shake.value = PlayerPrefs.GetFloat(SHAKE_KEY, DEFAULT_SHAKE);
        hitStop.value = PlayerPrefs.GetFloat(HITSTOP_KEY, DEFAULT_HITSTOP);
        vol.value = PlayerPrefs.GetFloat(VOLUME_KEY, DEFAULT_VOLUME);

        // agrego listerns a la ui, primero remuevo para no tener duplicados

        sens.onValueChanged.RemoveListener(OnSensitivityChanged);
        shake.onValueChanged.RemoveListener(OnShakeChanged);
        hitStop.onValueChanged.RemoveListener(OnHitStopChanged);
        vol.onValueChanged.RemoveListener(OnVolumeChanged);

        sens.onValueChanged.AddListener(OnSensitivityChanged);
        shake.onValueChanged.AddListener(OnShakeChanged);
        hitStop.onValueChanged.AddListener(OnHitStopChanged);
        vol.onValueChanged.AddListener(OnVolumeChanged);

        // acutalizo textos
        UpdateTexts(sens.value, vol.value, shake.value, hitStop.value, sensT, shakeT, hitStopT, volT);
    }

    public void ApplySettingsToGame()
    {
        // agarro valores desde los playerprefs
        float sensValue = PlayerPrefs.GetFloat(SENSITIVITY_KEY, DEFAULT_SENS);
        float shakeValue = PlayerPrefs.GetFloat(SHAKE_KEY, DEFAULT_SHAKE);
        float volumeValue = PlayerPrefs.GetFloat(VOLUME_KEY, DEFAULT_VOLUME);

        if (cameraController != null)
        {
            cameraController.sensitivity = sensValue;
        }
        CameraShakeIntensity.currentIntensity = shakeValue;

        // aplico volumen
        ApplyVolumeToMixer(MASTER_VOLUME_PARAM, PlayerPrefs.GetFloat(VOLUME_KEY, DEFAULT_VOLUME));
        ApplyVolumeToMixer(MUSIC_VOLUME_PARAM, PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, DEFAULT_MUSIC_VOLUME));
        ApplyVolumeToMixer(SFX_VOLUME_PARAM, PlayerPrefs.GetFloat(SFX_VOLUME_KEY, DEFAULT_SFX_VOLUME));
        ApplyVolumeToMixer(UI_VOLUME_PARAM, PlayerPrefs.GetFloat(UI_VOLUME_KEY, DEFAULT_UI_VOLUME));
    }

    // ahora son publicas y hacen lo de antes, cambio y guardado
    public void OnSensitivityChanged(float value)
    {
        if (cameraController != null)
        {
            cameraController.sensitivity = value;
        }
        PlayerPrefs.SetFloat(SENSITIVITY_KEY, value);
        PlayerPrefs.Save();
    }

    public void OnShakeChanged(float value)
    {
        CameraShakeIntensity.currentIntensity = value;
        PlayerPrefs.SetFloat(SHAKE_KEY, value);
        PlayerPrefs.Save();
    }

    public void OnHitStopChanged(float value)
    {
        SlowMotion.GlobalMultiplier = Mathf.Clamp(value, 0.1f, 2f);
        PlayerPrefs.SetFloat(HITSTOP_KEY, value);
        PlayerPrefs.Save();
    }

    public void OnVolumeChanged(float value)
    {
        ApplyVolumeToMixer(MASTER_VOLUME_PARAM, value);
        PlayerPrefs.SetFloat(VOLUME_KEY, value);
        PlayerPrefs.Save();
    }

    public void OnMusicVolumeChanged(float value)
    {
        ApplyVolumeToMixer(MUSIC_VOLUME_PARAM, value);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
        PlayerPrefs.Save();
    }

    public void OnSFXVolumeChanged(float value)
    {
        ApplyVolumeToMixer(SFX_VOLUME_PARAM, value);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, value);
        PlayerPrefs.Save();
    }

    public void OnUIVolumeChanged(float value)
    {
        ApplyVolumeToMixer(UI_VOLUME_PARAM, value);
        PlayerPrefs.SetFloat(UI_VOLUME_KEY, value);
        PlayerPrefs.Save();
    }

    private void ApplyVolumeToMixer(string mixerParam, float value)
    {
        float volume;
        if (value <= 0.0001f)
        {
            volume = -80f; // full mute
        }
        else
        {
            volume = Mathf.Log10(value) * 20; // q sea decibelios
        }
        masterMixer.SetFloat(mixerParam, volume);
    }

    // textos
    public void UpdateTexts(float sensVal, float volVal, float shakeVal, float hitStopVal, TMP_Text sensT, TMP_Text shakeT, TMP_Text hitStopT, TMP_Text volT)
    {
        if (sensT != null) sensT.text = "SENSIBILIDAD: " + sensVal.ToString("F1");
        if (shakeT != null) shakeT.text = "MOVIMIENTO DE LA CAMARA: " + shakeVal.ToString("F1");
        if (hitStopT != null) hitStopT.text = "PAUSA DE IMPACTO: " + hitStopVal.ToString("F1");
        if (volT != null) volT.text = "VOLUMEN: " + volVal.ToString("F1");
    }
}
