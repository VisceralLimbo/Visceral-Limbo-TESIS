using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class DoorAnimation : MonoBehaviour
{
    [SerializeField] Animator _anim;

    public static event Action<DoorAnimation> OnAnyPlayerEnter;
    public static event Action<DoorAnimation> OnAnyPlayerExit;
    [SerializeField] RoomEnterTrigger roomEnterTrigger;

    private bool doorOpen = false;

    private void OpenDoor()
    {
        _anim.SetBool("Abrir", true);
        doorOpen = true;
        if (roomEnterTrigger != null)
            roomEnterTrigger.SetSolidState(true);
    }

    private void CloseDoor(Collider other)
    {
        _anim.SetBool("Abrir", false);
        doorOpen = false;
        if (roomEnterTrigger != null)
        {
            StartCoroutine(DelayCollider());
        }
    }
    private IEnumerator DelayCollider()
    {
        yield return new WaitForSeconds(1f);
        roomEnterTrigger.SetSolidState(false);
    }
    public void TryOpenDoor()
    {
        if (!doorOpen && !roomEnterTrigger.IsInCombat)
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
            CloseDoor(other);
            
        }
    }
}
