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

    [SerializeField] float AirKillExtra;
    [SerializeField] float FriendlyFireExtra;
    [SerializeField] float PlayerScore;
    [SerializeField] float Skill1ScoreBonus;
    [SerializeField] float Skill2ScoreBonus;
    [SerializeField] float ParryBonus;
    [SerializeField] TextMeshProUGUI ScoreText;


    private void Awake()
    {
        if (Instance == null && Instance != this) Instance = this;
        else Destroy(this.gameObject);
    }


    public void ProcessKill(DamageScore DamageData)
    {
        if(DamageData.Victim == null|| DamageData.Attacker == null) return;

        if (DamageData.Victim == null || DamageData.Attacker == null) return;

        float FinalScore = DamageData.EnemyScoreBase;

        if (DamageData.IsTagged(ScoreFlags.Airkill))
        {
            FinalScore += AirKillExtra;
        }

        if (DamageData.IsTagged(ScoreFlags.Overkill))
        {
            FinalScore += DamageData.Overkill;
        }

        if (DamageData.IsTagged(ScoreFlags.TrapKill))
        {
            FinalScore += FriendlyFireExtra * 1.25f;
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


        PlayerScore += FinalScore;
        ScoreText.text = PlayerScore.ToString();

        Combat_UI_Manager._Instance.AddNewScoreEntry(DamageData);

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
