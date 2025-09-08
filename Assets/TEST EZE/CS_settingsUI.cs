using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CS_settingsUI : MonoBehaviour
{
    public static CS_settingsUI Instance; // st

    [Header("Sliders")]
    public Slider sensSlider;
    public Slider shakeSlider;
    public Slider hitStopSlider; 

    [Header("Valores")]
    public TMP_Text sensitivityText;
    public TMP_Text shakeText;
    public TMP_Text hitStopText;

    [Header("Referencias")]
    public Player_CameraController cameraController;
    public CameraShake cameraShake;

    [Header("HitStop Opciones")]
    // las opciones q tienen en el Stop del slowmotion
    public float slowdownFactor = 0.2f;
    public bool affectAudio = true;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // sens
        if (cameraController != null)
        {
            sensSlider.value = cameraController.sensitivity;
            sensSlider.onValueChanged.AddListener(OnSensitivityChanged);
        }

        // camera shake
        if (cameraShake != null)
        {
            shakeSlider.value = 0.5f;
            shakeSlider.onValueChanged.AddListener(OnShakeChanged);
        }

        // hitstop
        if (hitStopSlider != null)
        {
            hitStopSlider.value = 0.1f; // default
            hitStopSlider.onValueChanged.AddListener(OnHitStopChanged);
        }

        UpdateTexts();
    }

    private void OnSensitivityChanged(float value)
    {
        if (cameraController != null)
        {
            cameraController.sensitivity = value;
        }
        UpdateTexts();
    }

    private void OnShakeChanged(float value)
    {
        CameraShakeIntensity.currentIntensity = value;
        UpdateTexts();
    }

    private void OnHitStopChanged(float value)
    {
        UpdateTexts();

        // normalize porque a veces estaba 10 anios en pausa
        SlowMotion.GlobalMultiplier = Mathf.Clamp(value, 0.1f, 2f);
    }

    private void UpdateTexts()
    {
        if (sensitivityText != null)
            sensitivityText.text = "Sensibilidad: " + sensSlider.value.ToString("F1");

        if (shakeText != null)
            shakeText.text = "Camera Shake: " + shakeSlider.value.ToString("F1");

        if (hitStopText != null)
            hitStopText.text = "HitStop: " + hitStopSlider.value.ToString("F1");
    }
}
