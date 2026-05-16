using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public TutorialID tutorialID;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        TutorialController.Instance.TriggerTutorial(tutorialID);
        gameObject.SetActive(false);
    }
}
