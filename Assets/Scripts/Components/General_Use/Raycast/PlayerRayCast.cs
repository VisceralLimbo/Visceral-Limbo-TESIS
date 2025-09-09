using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRayCast : RayCastWrapper
{
    //HACK! Reemplazar por el PlayerInputs
    [SerializeField] KeyCode InteractKey= KeyCode.None;



    public void Update()
    {
        PerformRayCast();

        if(CurrentDetected != null && CurrentDetected is IRaycastInteractable interactable &&
            Input.GetKeyDown(InteractKey))
        {
            interactable.OnInteract();
        }

    }
}
