using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class DialogueManager : Visceral_Script
{
    public static DialogueManager instance;
    [Header("UI")]
    public TextMeshProUGUI DialogueText;
    public Image SpeakerIcon;
    public TextMeshProUGUI SpeakerName;
    public GameObject DialoguePanel;
    public GameObject OptionsPanel;
    public GameObject OptionButtonPrefab;
    public GameObject HandMesh;

    [Header("UI Elements to Hide During Dialogue")]
    public List<GameObject> UIElementsToHide; 
    public GameObject PlayerSword;

    [Header("Player Control")]
    public Player_Movement PlayerMovement;

    [Header("Settings")]
    [SerializeField] private float _TextSpeed;
    [SerializeField] private int _CurrentNodeIndex = 0;
    [SerializeField] private bool _IsTyping = false;

    [SerializeField]private List<GameObject> CurrentOptionsList = new List<GameObject>();


    private Coroutine TypingCoroutine;
    private Coroutine AutoAdvanceTextCoroutine;

    public DialogueData CurrentDialogue;

    [Header("Shader Effect")]
    public Material DialogueEffectMaterial;

    [SerializeField] private string damageFlashProperty = "_VignetteIntensity";

    public Material BloodEffectMaterial;

    [SerializeField] private float shaderTransitionDuration = 0.5f;

    private Coroutine dialogueShaderCoroutine;
    private Coroutine bloodShaderCoroutine;

    public void Start()
    {
        if (instance == null && instance != this) instance = this;

        if (BloodEffectMaterial != null)
            BloodEffectMaterial.SetFloat("_SetActive", 0f);
    }

    public void StartDialogue(DialogueData DialogeDT)
    {
        CurrentDialogue = DialogeDT;
        _CurrentNodeIndex = 0;
        DialoguePanel.SetActive(true);
        HandMesh.SetActive(true);

        foreach (var uiElement in UIElementsToHide)
            uiElement.SetActive(false);

        if (PlayerSword != null)
            PlayerSword.SetActive(false);

        if (PlayerMovement != null)
            PlayerMovement.IsMovementBlocked = true;

        if (DialogueEffectMaterial != null)
        {
            if (dialogueShaderCoroutine != null) StopCoroutine(dialogueShaderCoroutine);
            dialogueShaderCoroutine = StartCoroutine(SetShaderFloatOverTime(DialogueEffectMaterial, "_VignetteIntensity", 1f));
        }

        if (BloodEffectMaterial != null)
        {
            if (bloodShaderCoroutine != null) StopCoroutine(bloodShaderCoroutine);
            bloodShaderCoroutine = StartCoroutine(SetShaderFloatOverTime(BloodEffectMaterial, "_SetActive", 1f));
        }

        NextNode();
    }


    private void NextNode()
    {
        if(CurrentDialogue == null)
        {
            Debug.LogError("<Color=blue> Visceral Error: Dialoge not found, are you sure you passed a dialoge?</Color>");
        }
        if( _CurrentNodeIndex < 0 || _CurrentNodeIndex >= CurrentDialogue.DialogueNodes.Count)
        {
            EndDialogue();
            return;
        }

        StopAllCoroutines();
        ClearOptions();
        StartCoroutine(TypeText(CurrentDialogue.DialogueNodes[_CurrentNodeIndex]));
    }

    private IEnumerator TypeText(DialogueNode NodeDT)
    {
        _IsTyping = true;
        DialogueText.text = "";
        

        foreach(char Letter in NodeDT.TextData.ToCharArray())  
        {
            DialogueText.text += Letter;
            yield return new WaitForSeconds(_TextSpeed);
        }

        _IsTyping = false;

        if(NodeDT.Options != null && NodeDT.Options.Count > 0)
        {
            ShowOptions(NodeDT.Options);
        }
        else
        {
            if (NodeDT.AutoAdvance)
            {
                AutoAdvanceTextCoroutine = StartCoroutine(AutoAdvanceDialogue(NodeDT.TimeToAdvance));
            }
        }
    }

    private void ShowOptions(List<DialogeOption> options)
    {
        OptionsPanel.SetActive(true);

        var validoptions = options.Where(o => !string.IsNullOrEmpty(o.TextReply)).ToList(); //Hecho por Lucas - Where y ToList

        foreach (var Option in validoptions)
        {
            GameObject ButtonOBJ = Instantiate(OptionButtonPrefab, OptionsPanel.transform);
            TMP_Text ButtonText = ButtonOBJ.GetComponentInChildren<TMP_Text>();
            ButtonText.text = Option.TextReply;

            Button Button = ButtonOBJ.GetComponent<Button>();
            int nextOption = Option.nextDialogueID; // cache interno
            CurrentOptionsList.Add(ButtonOBJ);
            Button.onClick.AddListener(() => SelectOption(nextOption));
        }
    }

    public void SelectOption(int NextIntID)
    {
        _CurrentNodeIndex = NextIntID;
        OptionsPanel.SetActive(false);
        NextNode();
    }
    private void ClearOptions()
    {
        foreach(var Button in CurrentOptionsList)
        {
            Destroy(Button);
        }
        CurrentOptionsList.Clear();
    }

    private IEnumerator AutoAdvanceDialogue(float Duration)
    {

      
        yield return new WaitForSeconds(Duration);

        _CurrentNodeIndex = CurrentDialogue.DialogueNodes[_CurrentNodeIndex].NextDialogeOption;
        NextNode();
    }

    private void EndDialogue()
    {
        DialogueText.text = "";
        DialoguePanel.SetActive(false);
        OptionsPanel.SetActive(false);
        HandMesh.SetActive(false);

        foreach (var uiElement in UIElementsToHide)
            uiElement.SetActive(true);

        if (PlayerSword != null)
            PlayerSword.SetActive(true);

        if (PlayerMovement != null)
            PlayerMovement.IsMovementBlocked = false;

        if (DialogueEffectMaterial != null)
        {
            if (dialogueShaderCoroutine != null) StopCoroutine(dialogueShaderCoroutine);
            dialogueShaderCoroutine = StartCoroutine(SetShaderFloatOverTime(DialogueEffectMaterial, "_VignetteIntensity", 0f));
        }

        if (BloodEffectMaterial != null)
        {
            if (bloodShaderCoroutine != null) StopCoroutine(bloodShaderCoroutine);
            bloodShaderCoroutine = StartCoroutine(SetShaderFloatOverTime(BloodEffectMaterial, "_SetActive", 0f));
        }

        Debug.Log("FinishDialogue");
    }

    private IEnumerator SetShaderFloatOverTime(Material mat, string property, float targetValue)
    {
        DialogueEffectMaterial.SetFloat(damageFlashProperty, 1f);


        yield return new WaitForSeconds(0.5f);


        float fadeDuration = 0.5f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float newValue = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            DialogueEffectMaterial.SetFloat(damageFlashProperty, newValue);
            yield return null;
        }
        DialogueEffectMaterial.SetFloat(damageFlashProperty, 0f);
    }

}

//codigo hecho por patricio malvasio
// manager de dialogo
//
// TO DO: 
// pedir aiuda para solucionar el problema con los botones :(




