using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRayCast : RayCastWrapper
{
    //HACK! Reemplazar por el PlayerInputs
    [SerializeField] KeyCode InteractKey= KeyCode.None;

    public bool IsLookingAtInteractible;

    public void Update()
    {
        PerformRayCast();

        //estoy viendo un raycastDetectable
        if(CurrentDetected != null)
        {
            CurrentDetected.OnRayCastStay(this);
            IsLookingAtInteractible = false;
        }

        if(CurrentDetected != null && CurrentDetected is IRaycastInteractable interactable &&
            Input.GetKeyDown(InteractKey))
        {
            interactable.OnInteract(this);
            IsLookingAtInteractible = true;
        }

    }
}
