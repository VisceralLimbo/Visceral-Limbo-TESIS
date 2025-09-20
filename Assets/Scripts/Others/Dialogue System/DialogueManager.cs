using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

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

    [Header("UI Elements to Hide During Dialogue")]
    public List<GameObject> UIElementsToHide;

    [Header("Settings")]
    [SerializeField] private float _TextSpeed = 0.05f;
    [SerializeField] private int _CurrentNodeIndex = 0;
    [SerializeField] private bool _IsTyping = false;

    [SerializeField] private List<GameObject> CurrentOptionsList = new List<GameObject>();

    private Coroutine TypingCoroutine;
    private Coroutine AutoAdvanceTextCoroutine;

    public DialogueData CurrentDialogue;

    private bool _dialogueActive = false;
    private DialogueNode currentNodeData;

    public Action OnDialogueStart, OnDialogueEnd;

    [Header("External Effects")]
    [SerializeField] private BloodEffectController bloodController;
    [SerializeField] private DialogueShaderController vignetteController;

    public void Awake()
    {
        if (instance == null && instance != this) instance = this;
    }

    private void Update()
    {
        if (!_dialogueActive) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_IsTyping)
            {
                if (TypingCoroutine != null)
                {
                    StopCoroutine(TypingCoroutine);
                    TypingCoroutine = null;
                }

                DialogueText.text = currentNodeData.TextData;
                _IsTyping = false;

                if (currentNodeData.Options != null && currentNodeData.Options.Count > 0)
                {
                    ShowOptions(currentNodeData.Options);
                }
                else if (currentNodeData.AutoAdvance)
                {
                    AutoAdvanceTextCoroutine = StartCoroutine(AutoAdvanceDialogue(currentNodeData.TimeToAdvance));
                }
            }
            else if (!OptionsPanel.activeSelf)
            {
                _CurrentNodeIndex = currentNodeData.NextDialogeOption;
                NextNode();
            }
        }
    }

    public void StartDialogue(DialogueData DialogeDT)
    {
        _dialogueActive = true;
        CurrentDialogue = DialogeDT;
        _CurrentNodeIndex = 0;
        DialoguePanel.SetActive(true);

        foreach (var uiElement in UIElementsToHide)
            uiElement.SetActive(false);

        LockPlayerMovement();

        // activar efecto sangre
        if (bloodController != null) bloodController.PlayEffect();

        // activar vigneta
        if (vignetteController != null) vignetteController.PlayEffect();

        NextNode();
        OnDialogueStart?.Invoke();
    }

    private void NextNode()
    {
        if (CurrentDialogue == null)
        {
            Debug.LogError("<Color=blue> Visceral Error: Dialoge not found, are you sure you passed a dialoge?</Color>");
        }
        if (_CurrentNodeIndex < 0 || _CurrentNodeIndex >= CurrentDialogue.DialogueNodes.Count)
        {
            EndDialogue();
            return;
        }

        if (TypingCoroutine != null)
        {
            StopCoroutine(TypingCoroutine);
            TypingCoroutine = null;
        }
        if (AutoAdvanceTextCoroutine != null)
        {
            StopCoroutine(AutoAdvanceTextCoroutine);
            AutoAdvanceTextCoroutine = null;
        }

        ClearOptions();
        TypingCoroutine = StartCoroutine(TypeText(CurrentDialogue.DialogueNodes[_CurrentNodeIndex]));
    }

    private IEnumerator TypeText(DialogueNode NodeDT)
    {
        _IsTyping = true;
        DialogueText.text = "";
        currentNodeData = NodeDT;

        foreach (char Letter in NodeDT.TextData.ToCharArray())
        {
            DialogueText.text += Letter;
            if (Input.GetKeyDown(KeyCode.E))
            {
                DialogueText.text = NodeDT.TextData;
                break;
            }
            yield return new WaitForSeconds(_TextSpeed);
        }

        _IsTyping = false;
        TypingCoroutine = null;

        if (NodeDT.Options != null && NodeDT.Options.Count > 0)
        {
            ShowOptions(NodeDT.Options);
        }
        else if (NodeDT.AutoAdvance)
        {
            AutoAdvanceTextCoroutine = StartCoroutine(AutoAdvanceDialogue(NodeDT.TimeToAdvance));
        }
    }

    private void ShowOptions(List<DialogeOption> options)
    {
        OptionsPanel.SetActive(true);

        var validoptions = options.Where(o => !string.IsNullOrEmpty(o.TextReply)).ToList();

        foreach (var Option in validoptions)
        {
            GameObject ButtonOBJ = Instantiate(OptionButtonPrefab, OptionsPanel.transform);
            TMP_Text ButtonText = ButtonOBJ.GetComponentInChildren<TMP_Text>();
            ButtonText.text = Option.TextReply;

            Button Button = ButtonOBJ.GetComponent<Button>();
            int nextOption = Option.nextDialogueID;
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
        foreach (var Button in CurrentOptionsList)
        {
            Destroy(Button);
        }
        CurrentOptionsList.Clear();
    }

    private IEnumerator AutoAdvanceDialogue(float Duration)
    {
        yield return new WaitForSeconds(Duration);
        if (_CurrentNodeIndex >= CurrentDialogue.DialogueNodes.Count)
        {
            _CurrentNodeIndex = 0;
            EndDialogue();
            yield break;
        }
        else
        {
            if (_CurrentNodeIndex >= 0)
            {
                _CurrentNodeIndex = CurrentDialogue.DialogueNodes[_CurrentNodeIndex].NextDialogeOption;
                NextNode();
            }
        }
    }

    private void EndDialogue()
    {
        _dialogueActive = false;

        DialogueText.text = "";
        DialoguePanel.SetActive(false);
        OptionsPanel.SetActive(false);

        foreach (var uiElement in UIElementsToHide)
            uiElement.SetActive(true);

        UnlockPlayerMovement();

        // desactivar efecto sangre
        if (bloodController != null) bloodController.StopEffect();

        // desactivar vigneta
        if (vignetteController != null) vignetteController.StopEffect();

        Debug.Log("FinishDialogue");
        OnDialogueEnd?.Invoke();
    }

    private void LockPlayerMovement()
    {
        Player_Movement player = FindObjectOfType<Player_Movement>();
        if (player != null) player.IsMovementBlocked = true;
    }

    private void UnlockPlayerMovement()
    {
        Player_Movement player = FindObjectOfType<Player_Movement>();
        if (player != null) player.IsMovementBlocked = false;
    }
}




//codigo hecho por patricio malvasio
// manager de dialogo
//
// TO DO: 
// pedir aiuda para solucionar el problema con los botones :(




