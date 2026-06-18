using System.Collections;
using UnityEngine;

public class RoomEffectMessageManager : MonoBehaviour
{
    public static RoomEffectMessageManager Instance;

    [SerializeField] private GameObject messageObject;
    [SerializeField] private Animator animator;

    [SerializeField] private float hideDelay = 3f;

    private bool shownFrozen;
    private bool shownLowVisibility;

    private readonly int FrozenHash = Animator.StringToHash("Frozen");
    private readonly int LowVisibilityHash = Animator.StringToHash("LowVisibility");

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void TryShowMessage(RoomSpawnerManager.EnvironmentalEffect effect)
    {
        switch (effect)
        {
            case RoomSpawnerManager.EnvironmentalEffect.Frozen:

                if (shownFrozen) return;

                shownFrozen = true;

                ShowMessage(FrozenHash);

                break;

            case RoomSpawnerManager.EnvironmentalEffect.LowVisibility:

                if (shownLowVisibility) return;

                shownLowVisibility = true;

                ShowMessage(LowVisibilityHash);

                break;
        }
    }

    private void ShowMessage(int triggerHash)
    {
        StopAllCoroutines();

        messageObject.SetActive(true);

        animator.ResetTrigger("Frozen");
        animator.ResetTrigger("LowVisibility");

        animator.SetTrigger(triggerHash);

        StartCoroutine(HideRoutine());
    }

    private IEnumerator HideRoutine()
    {
        yield return new WaitForSeconds(hideDelay);

        messageObject.SetActive(false);
    }
}
