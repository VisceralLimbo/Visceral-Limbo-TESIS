using TMPro;
using UnityEngine;
using System.Collections;

public class TutorialUI : MonoBehaviour
{
    public TextMeshProUGUI text;
    public TutorialTaskManager manager;

    [SerializeField] private RectTransform rect;
    [SerializeField] private GameObject tutorialCanvas;

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.4f;

    private bool isHiding = false;

    void Start()
    {
        tutorialCanvas.SetActive(false);
        if (DungeonGenerator.Instance != null)
        {
            DungeonGenerator.Instance.OnSuccessfulGeneration += ShowTutorial;
        }
        else
        {
            Debug.LogError("DungeonGenerator sigue siendo NULL en Start");
        }
    }

    void Update()
    {
        if (!tutorialCanvas.activeSelf) return;
        
        if (!isHiding)
        text.text =
            $"{Format(manager.moveDone, "Moverse (WASD)")}\n" +
            $"{Format(manager.jumpDone, "Saltar (Espacio)")}\n" +
            $"{Format(manager.crouchDone, "Agacharse (C)")} \n" +
            $"{Format(manager.attackDone, "Atacar (M1)")}";


        if (!isHiding && manager.AllCompleted)
        {
            isHiding = true;
            StartCoroutine(HideRoutine());
        }
    }

    string Format(bool done, string task)
    {
        // gris si no verde si esta hecho je
        if (done)
            return $"<color=green>[<color=white>X</color>] {task}</color>";
        else
            return $"<color=#888888>[ ] {task}</color>";
    }

    void ShowTutorial()
    {
        if (TutorialManager.HasSeenTutorial())
            return;

        tutorialCanvas.SetActive(true);
        manager.EnableTutorial();

        // arranca fuera de pantalla
        rect.anchoredPosition = new Vector2(-300, rect.anchoredPosition.y);

        StartCoroutine(AnimateIn());
    }

    IEnumerator HideRoutine()
    {
        yield return new WaitForSeconds(0.8f);

        yield return AnimateOut();

        TutorialManager.MarkTutorialAsSeen();
        tutorialCanvas.SetActive(false);
    }

    IEnumerator AnimateIn()
    {
        Vector2 start = new Vector2(-300, rect.anchoredPosition.y);
        Vector2 end = new Vector2(20, rect.anchoredPosition.y);

        float t = 0;

        while (t < animationDuration)
        {
            t += Time.deltaTime;
            float n = t / animationDuration;

            rect.anchoredPosition = Vector2.Lerp(start, end, n);
            yield return null;
        }

        rect.anchoredPosition = end;
    }

    IEnumerator AnimateOut()
    {
        Vector2 start = rect.anchoredPosition;
        Vector2 end = new Vector2(-300, rect.anchoredPosition.y);

        float t = 0;

        while (t < animationDuration)
        {
            t += Time.deltaTime;
            float n = t / animationDuration;

            rect.anchoredPosition = Vector2.Lerp(start, end, n);
            yield return null;
        }

        rect.anchoredPosition = end;
    }

    void OnEnable()
    {
        {
            if (DungeonGenerator.Instance != null)
                DungeonGenerator.Instance.OnSuccessfulGeneration += ShowTutorial;
        }
    }
    void OnDestroy()
    {
        if (DungeonGenerator.Instance != null)
        {
            DungeonGenerator.Instance.OnSuccessfulGeneration -= ShowTutorial;
        }
    }

}
