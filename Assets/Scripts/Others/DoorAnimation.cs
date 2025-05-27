using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class DoorAnimation : MonoBehaviour
{
    [SerializeField] Animator _anim;

    public static event Action<DoorAnimation> OnAnyPlayerEnter;
    public static event Action<DoorAnimation> OnAnyPlayerExit;

    private bool doorOpen = false;

    private void OpenDoor()
    {
        _anim.SetBool("Abrir", true);
        doorOpen = true;
    }

    private void CloseDoor()
    {
        _anim.SetBool("Abrir", false);
        doorOpen = false;
    }

    public void TryOpenDoor()
    {
        if (!doorOpen)
        {
            OpenDoor();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnAnyPlayerEnter?.Invoke(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnAnyPlayerExit?.Invoke(this);
            CloseDoor(); // opcional
        }
    }
}
