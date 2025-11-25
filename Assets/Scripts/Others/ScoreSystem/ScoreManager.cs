using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{

    //to do: reemplazar por un GlobalBusCommunicator => un script que comunique todos los eventos, similar a un event manager
    public static ScoreManager Instance;

    [Header("Variables")]
    [SerializeField] float AirKillExtra;
    [SerializeField] float FriendlyFireExtra;
    [SerializeField] float PlayerScore;
    [SerializeField] float Skill1ScoreBonus;
    [SerializeField] float Skill2ScoreBonus;
    [SerializeField] float ParryBonus;

    [Header("References")]
    [SerializeField] TextMeshProUGUI ScoreText;

    [Header("Desired points")]
    private List<float> milestoneThresholds = new List<float>();
    //[SerializeField] AudioClip milestoneClip;
    private AudioSource audioSource;
    private bool milestoneReached = false;
    [SerializeField] private AudioSource milestoneClip;

    public float GetPlayerScore { get { return PlayerScore; } }

    private float CurrentPlayerScore;

    public float GetCurrentPlayerScore { get { return CurrentPlayerScore; } }


    public Action<float> OnPlayerScoreChanged;

    private void Awake()
    {
        if (Instance == null && Instance != this) Instance = this;
        else Destroy(this.gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    /// <summary>
    /// Registro de milestones 
    /// </summary>
    /// <param name="threshold"></param>
    public void RegisterMilestone(float threshold)
    {
        if (!milestoneThresholds.Contains(threshold))
        {
            milestoneThresholds.Add(threshold);
        }
    }

    public void ResetScoreText()
    {
        CurrentPlayerScore = 0;
        ScoreText.text = CurrentPlayerScore.ToString();
    }


    /// <summary>
    /// Funcion de proceso de la muerte del NPC
    /// </summary>
    /// <param name="DamageData"></param>
    public void ProcessKill(DamageScore DamageData)
    {
        if(DamageData.Victim == null|| DamageData.Attacker == null) return; // no hay atacante o victima


        float FinalScore = DamageData.EnemyScoreBase; // creamos el valor base

        if (DamageData.IsTagged(ScoreFlags.Airkill)) // chequeamos si tenemos flag de airkill
        {
            FinalScore += AirKillExtra; //sumamos extra
        }

        if (DamageData.IsTagged(ScoreFlags.Overkill)) // chequeamos si tenemos flag de overkill
        {
            FinalScore += DamageData.Overkill; // sumamos overkill
        }

        //chequeamos si tiene tag de muerte de trampa Y no tiene tag de daño explosivo
        // en pocas palabras, los barriles estan excentos
        if (DamageData.IsTagged(ScoreFlags.TrapKill) && !DamageData.IsTagged(ScoreFlags.Explosion))
        {
            FinalScore += FriendlyFireExtra * 0.8f; // hack: usamos el friendlyfire menos un valor
        }

        // hack!: tecnicamente este flag puede ser levantado por cualquier source de daño explosivo
        // pero solo el barril tiene la habilidad de levantar ese flag... yem
        if (DamageData.IsTagged(ScoreFlags.Explosion))
        {
            FinalScore += FriendlyFireExtra * 1.2f;
        }

        if (DamageData.IsTagged(ScoreFlags.Skill1Kill))
        {
            FinalScore += Skill1ScoreBonus;

        }

        if(DamageData.IsTagged(ScoreFlags.Skill2Kill))
        {
            FinalScore += Skill2ScoreBonus;
        }

        if (DamageData.IsTagged(ScoreFlags.Parried))
        {
            FinalScore += ParryBonus;
        }

        if (VerifyFriendlyFire(DamageData))
        {
            FinalScore += FriendlyFireExtra;
        }

        CurrentPlayerScore += FinalScore;
        PlayerScore += FinalScore;
        //ScoreText.text = CurrentPlayerScore.ToString();

        for (int i = milestoneThresholds.Count - 1; i >= 0; i--)
        {
            if (PlayerScore >= milestoneThresholds[i])
            {
                if (milestoneClip != null)
                    milestoneClip.Play();

                milestoneThresholds.RemoveAt(i); 
            }
        }

        Combat_UI_Manager._Instance.AddNewScoreEntry(DamageData);
        BloodEchoesManager.AddBloodEchoes(FinalScore);
    }


    private bool VerifyFriendlyFire(DamageScore DamageData) 
    {
        if (DamageData.Attacker == null) Debug.LogError("attacker data is null");
        if (DamageData.Victim == null) Debug.LogError("Victim Data is null");

        if (DamageData.Attacker.faction == DamageData.Victim.faction)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}

///
/// Script creado por Patricio Malvasio 2/5/2025
///
/// este script sirve como manager del sistema de puntuacion.
///
