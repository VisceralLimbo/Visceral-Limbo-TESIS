using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTriggerZone : MonoBehaviour
{
    [Header("Dialogue to Trigger")]
    public DialogueData dialogueToTrigger;

    [Header("Optional Settings")]
    public bool destroyAfterTriggered = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player")) 
        {
            if (DialogueManager.instance != null && dialogueToTrigger != null)
            {
                DialogueManager.instance.StartDialogue(dialogueToTrigger);
                hasTriggered = true;

                if (destroyAfterTriggered)
                {
                    Destroy(gameObject); 
                }
            }
        }
    }
}
