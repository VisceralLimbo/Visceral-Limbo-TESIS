using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Combat_UI_Manager : MonoBehaviour
{
    private void Start()
    {
        if (_Instance == null && _Instance != this)
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
    [SerializeField] FloatingText _FloatingText;
    Queue<GameObject> _ScoreQueue = new Queue<GameObject>();
    [SerializeField] float _maximunEntries, _timeToClearEntry;


    [Space]
    [Header("SkillUIElements")]
    [SerializeField] Slider[] _CooldownSliders;

    [Header("Shader")]
    [SerializeField] private Material shaderMaterial;
    [SerializeField] private string damageFlashProperty = "_VignetteIntensity";


    public void UpdateCooldownImages(float CooldownValue,float MaxCooldownValue, string SkillID)
    {
        float currentCooldown = CooldownValue / MaxCooldownValue;

        switch(SkillID)
        {
            case "Skill1":
                _CooldownSliders[0].value = currentCooldown;
                break;
            case "Skill2":
                _CooldownSliders[1].value = currentCooldown;
                break;
            case "Ultimate":
                _CooldownSliders[2].value = currentCooldown;
                break;
            case "Support":
                _CooldownSliders[3].value = currentCooldown;
                break;
            default: break;

        }

    }



    public void UpdatePlayerHealthBar(float CurrentHP,float MaxHP)
    {
        if(_PlayerHealthSlider != null)
        {
            _PlayerHealthSlider.value = CurrentHP / MaxHP;
        }

        if(shaderMaterial != null)
        {
            StopCoroutine(nameof(FlashDamageEffect));
            StartCoroutine(FlashDamageEffect());
        }
    }

    public void UpdatePlayerHealthBar(float CurrentHP, float MaxHP,bool TookDamage)
    {
        if (_PlayerHealthSlider != null)
        {
            _PlayerHealthSlider.value = CurrentHP / MaxHP;
        }

        if (shaderMaterial != null && TookDamage)
        {
            StopCoroutine(nameof(FlashDamageEffect));
            StartCoroutine(FlashDamageEffect());
        }
    }

    IEnumerator FlashDamageEffect()
    {


        shaderMaterial.SetFloat(damageFlashProperty, 1f);


        yield return new WaitForSeconds(0.5f);


        float fadeDuration = 0.5f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float newValue = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            shaderMaterial.SetFloat(damageFlashProperty, newValue);
            yield return null;
        }
        shaderMaterial.SetFloat(damageFlashProperty, 0f);


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


                // HAAACKKK!! 
                // EL UI NO TENDRIA QUE SER QUIEN HAGA ESTO!!!
                // DIOS QUE HORRIBLE VIOLACION DE TODO LO QUE ME ENSEÑARON EN LA FACU
                // QUE CHATGPT SE AMPARE DE MIIIIIIIIIIIIIII  - pato
                var _FloatingTextPrefav = Instantiate(_FloatingText._Prefab,
                    DMScore.Victim.transform.position,
                    DMScore.Victim.transform.rotation);

                   _FloatingText.SetText(IndividualScore, Color.yellow);
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
