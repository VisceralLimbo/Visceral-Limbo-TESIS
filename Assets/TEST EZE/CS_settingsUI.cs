using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio; // <-- NUEVO: Para el Audio Mixer
using TMPro;

public class CS_settingsUI : MonoBehaviour
{
    public static CS_settingsUI Instance; // st

    // uso de playerprefs para guardar valores del slider y pasarlos en la escena ingame
    public const string SENSITIVITY_KEY = "GameSensitivity";
    public const string SHAKE_KEY = "CameraShakeIntensity";
    public const string HITSTOP_KEY = "HitStopDuration";
    public const string VOLUME_KEY = "Volume";
    // ----------------------------
    private const float DEFAULT_SENS = 5f;
    private const float DEFAULT_SHAKE = 0.5f;
    private const float DEFAULT_HITSTOP = 0.1f;
    private const float DEFAULT_VOLUME = 1.0f;

    [Header("Sliders")]
    public Slider sensSlider;
    public Slider shakeSlider;
    public Slider hitStopSlider; 
    public Slider volumeSlider;

    [Header("Valores")]
    public TMP_Text sensitivityText;
    public TMP_Text shakeText;
    public TMP_Text hitStopText;
    public TMP_Text volumeText;

    [Header("Referencias")]
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


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadSettings();

        // agrego listeners
        sensSlider.onValueChanged.AddListener(OnSensitivityChanged);
        shakeSlider.onValueChanged.AddListener(OnShakeChanged);
        hitStopSlider.onValueChanged.AddListener(OnHitStopChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        // aplico si estoy en la escena de juego (lo q guarde en el menu)
        ApplySettingsToGame();
        UpdateTexts();
    }

    private void LoadSettings()
    {
        // cargo lo guardado o uso default
        sensSlider.value = PlayerPrefs.GetFloat(SENSITIVITY_KEY, DEFAULT_SENS);
        shakeSlider.value = PlayerPrefs.GetFloat(SHAKE_KEY, DEFAULT_SHAKE);
        hitStopSlider.value = PlayerPrefs.GetFloat(HITSTOP_KEY, DEFAULT_HITSTOP);
        volumeSlider.value = PlayerPrefs.GetFloat(VOLUME_KEY, DEFAULT_VOLUME);
    }

    private void ApplySettingsToGame()
    {
        //  solo aplico si existe refe
        if (cameraController != null)
        {
            cameraController.sensitivity = sensSlider.value;
        }
        CameraShakeIntensity.currentIntensity = shakeSlider.value;
        OnVolumeChanged(volumeSlider.value);
    }

    private void OnSensitivityChanged(float value)
    {
        if (cameraController != null)
        {
            cameraController.sensitivity = value;
        }
        PlayerPrefs.SetFloat(SENSITIVITY_KEY, value);
        PlayerPrefs.Save();

        UpdateTexts();
    }

    private void OnShakeChanged(float value)
    {
        CameraShakeIntensity.currentIntensity = value;
        PlayerPrefs.SetFloat(SHAKE_KEY, value);
        PlayerPrefs.Save();
        UpdateTexts();
    }

    private void OnHitStopChanged(float value)
    {
        // normalize porque a veces estaba 10 anios en pausa
        SlowMotion.GlobalMultiplier = Mathf.Clamp(value, 0.1f, 2f);
        PlayerPrefs.SetFloat(HITSTOP_KEY, value);
        PlayerPrefs.Save();

        UpdateTexts();
    }

    private void OnVolumeChanged(float value)
    {
        // aplico al audio mixer el volumen
        float volume;
        if (value <= 0.0001f)
        {
            volume = -80f; //full muteee
        }
        else
        {
            // q sea decibelios
            volume = Mathf.Log10(value) * 20;
        }

        masterMixer.SetFloat(MASTER_VOLUME_PARAM, volume);
        PlayerPrefs.SetFloat(VOLUME_KEY, value);
        PlayerPrefs.Save();
        UpdateTexts();
    }

    private void UpdateTexts()
    {
        if (sensitivityText != null)
            sensitivityText.text = "Sensibilidad: " + sensSlider.value.ToString("F1");

        if (shakeText != null)
            shakeText.text = "Camera Shake: " + shakeSlider.value.ToString("F1");

        if (hitStopText != null)
            hitStopText.text = "HitStop: " + hitStopSlider.value.ToString("F1");

        if (volumeText != null)
            volumeText.text = "Volume: " + volumeSlider.value.ToString("F1");
    }
}
