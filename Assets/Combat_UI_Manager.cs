using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Combat_UI_Manager : MonoBehaviour
{
    private void Start()
    {
        if(_Instance == null && _Instance != this)
        {
            _Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static Combat_UI_Manager _Instance;
    [SerializeField] Slider _PlayerHealthSlider;
    [SerializeField] GameObject _WinWindow;
    [SerializeField] GameObject _LoseWindow;
    [Space]

    [Header("ScoreSystem")]
    [SerializeField] VerticalLayoutGroup _layoutGroup;
    [SerializeField] GameObject _ScorePrefab;
    [SerializeField] Transform _LayoutGroupPosition;
    Queue<GameObject> _ScoreQueue = new Queue<GameObject>();
    [SerializeField] float _maximunEntries, _timeToClearEntry;


    public void UpdatePlayerHealthBar(float CurrentHP,float MaxHP)
    {
        if(_PlayerHealthSlider != null)
        {
            _PlayerHealthSlider.value = CurrentHP / MaxHP;
        }
    }

    public void DisplayWin(bool visible)
    {
        _WinWindow.SetActive(visible);
    }

    public void DisplayLose(bool visible)
    {
        _LoseWindow.SetActive(visible);
    }

    public void AddNewScoreEntry(DamageScore DMScore)
    {
        var ScoredText = KillFeedManager.ComposeKillFeedText(DMScore);
        var ListOFScores = ScoredText.Split("_",System.StringSplitOptions.RemoveEmptyEntries);

        foreach(var IndividualScore in ListOFScores)
        {
            if(_ScoreQueue.Count < _maximunEntries)
            {
                var _Prefab = Instantiate(_ScorePrefab, _LayoutGroupPosition);

                var PrefabText = _Prefab.GetComponentInChildren<TextMeshProUGUI>();

                PrefabText.text = IndividualScore;
                if (!_ScoreQueue.Contains(_Prefab)) _ScoreQueue.Enqueue(_Prefab);
            }
        }

        StartCoroutine(DeleteEntry());
    }


    IEnumerator DeleteEntry()
    {
        while(_ScoreQueue.Count > 0)
        {
            yield return new WaitForSeconds(_timeToClearEntry);
            _ScoreQueue.TryDequeue(out var score);

            if (score != null)
            {
                Destroy(score);
            }

        }
    }

}
