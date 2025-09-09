using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastTest : MonoBehaviour, IRaycastInteractable
{
    public void OnInteract()
    {
        Debug.Log("ME HAN INTERACTUADO!");
    }

    public void OnRayCastEnter(RayCastWrapper Detector = null)
    {
        Debug.Log("me han visto");
    }

    public void OnRayCastExit(RayCastWrapper Detector = null)
    {
        Debug.Log("me han dejado de  ver");
    }

    public void OnRayCastStay(RayCastWrapper Detector = null)
    {
        Debug.Log("me estan viendo,baka");
    }
}
