using UnityEngine;

public class TutorialEndTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        TutorialController.Instance.FinishTutorial();

        gameObject.SetActive(false);
    }
}