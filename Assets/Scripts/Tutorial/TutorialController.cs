using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour
{
    public static TutorialController Instance;

    [SerializeField] private TutorialPopupUI popupUI;

    private TutorialID currentTutorial = TutorialID.None;
    private Player_InputHandler input;

    private bool inventoryOpened;
    private bool inItemsTab;
    private bool hoveredItem;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        input = Player_InputHandler.instance;
    }

    public void TriggerTutorial(TutorialID id)
    {
        if (AlreadyDone(id)) return;

        currentTutorial = id;

        switch (id)
        {
            case TutorialID.Ability_Q:
                popupUI.Show("Toca Q para hacer un giro y repeleer enemigos");
                break;

            case TutorialID.Ability_E:
                popupUI.Show("Toca E para hacer un parry y redirigir un proyectil");
                break;

            case TutorialID.Ability_Shift:
                popupUI.Show("Toca Shift para hacer un dash");
                break;

            case TutorialID.Ability_F:
                popupUI.Show("Toca la F para patear objetos");
                break;

            case TutorialID.Inventory:
                inventoryOpened = false;
                inItemsTab = false;
                hoveredItem = false;

                popupUI.Show("Toca Escape para ir a tus items");
                break;
        }
    }

    private void Update()
    {
        if (currentTutorial == TutorialID.None) return;
        if (input == null) return; // evita crash

        var data = input.CurrentMovementInput;

        switch (currentTutorial)
        {
            case TutorialID.Ability_Q:
                if (data.Ability_1) Complete();
                break;

            case TutorialID.Ability_E:
                if (data.Ability_2) Complete();
                break;

            case TutorialID.Ability_Shift:
                if (data.Ability_Support) Complete();
                break;

            case TutorialID.Ability_F:
                if (data.Kick) Complete();
                break;

            case TutorialID.Inventory:
                if (inventoryOpened && inItemsTab && hoveredItem)
                {
                    Complete();
                }
                break;
        }
    }

    void Complete()
    {
        CompleteTutorial(currentTutorial);
        popupUI.Hide();
        currentTutorial = TutorialID.None;
    }

    bool AlreadyDone(TutorialID id)
    {
        return PlayerPrefs.GetInt(id.ToString(), 0) == 1;
    }

    public void CompleteTutorial(TutorialID id)
    {
        PlayerPrefs.SetInt(id.ToString(), 1);
        PlayerPrefs.Save();
    }

    public void OnInventoryOpened()
    {
        if (popupUI == null)
            return;

        if (currentTutorial != TutorialID.Inventory)
            return;

        if (!inventoryOpened)
        {
            inventoryOpened = true;

            popupUI.Show("Andá a la pestaña de objetos");
        }
    }

    public void OnItemsTabOpened()
    {
        if (currentTutorial != TutorialID.Inventory) return;

        if (!inItemsTab)
        {
            inItemsTab = true;

            popupUI.Show("Pasá el mouse por un objeto");
        }
    }

    public void OnItemHovered()
    {
        if (currentTutorial != TutorialID.Inventory) return;
        if (hoveredItem) return;

        hoveredItem = true;

        StartCoroutine(FinishRoutine());
    }

    // tengo q arreglar no aparece xd
    IEnumerator FinishRoutine()
    {
        popupUI.Show("Esta es la descripcion del item que conseguiste");
        yield return new WaitForSecondsRealtime(1f);

        Complete();
    }

    public void FinishTutorial()
    {
        StartCoroutine(FinishTutorialRoutine());
    }

    IEnumerator FinishTutorialRoutine()
    {
        yield return StartCoroutine(ScreenFader.Instance.FadeOut());

        TutorialManager.MarkTutorialAsSeen();

        SceneManager.LoadScene("Proc Gen Scene");
    }
}