using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveUI : MonoBehaviour
{
    [SerializeField] GameObject[] _UI_Elements;

    private void Start()
    {
        PlayerEvents.OnInteractSeeing += ActivateInteractUI;
        PlayerEvents.OnInteractStopSeeing += DeactivateInteractUI;


    }


    private void OnDestroy()
    {
        PlayerEvents.OnInteractSeeing -= ActivateInteractUI;
        PlayerEvents.OnInteractStopSeeing -= DeactivateInteractUI;
    }

    private void ActivateInteractUI()
    {
        foreach(var element in _UI_Elements)
        {
            element.gameObject.SetActive(true);
        }
    }

    private void DeactivateInteractUI()
    {
        foreach (var element in _UI_Elements)
        {
            element.gameObject.SetActive(false);
        }
    }
}
