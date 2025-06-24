using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ui_Interactive : MonoBehaviour
{
    [SerializeField] GameObject pressEText;

    private DoorAnimation currentDoor;

    private void OnEnable()
    {
        DoorAnimation.OnAnyPlayerEnter += HandlePlayerEnterDoor;
        DoorAnimation.OnAnyPlayerExit += HandlePlayerExitDoor;
    }

    private void OnDisable()
    {
        DoorAnimation.OnAnyPlayerEnter -= HandlePlayerEnterDoor;
        DoorAnimation.OnAnyPlayerExit -= HandlePlayerExitDoor;
    }

    private void Update()
    {
        if (currentDoor != null && Input.GetKeyDown(KeyCode.G))
        {
            currentDoor.TryOpenDoor();
            pressEText.SetActive(false);
            currentDoor = null;
        }
    }

    private void HandlePlayerEnterDoor(DoorAnimation door)
    {
        currentDoor = door;
        pressEText.SetActive(true);
    }

    private void HandlePlayerExitDoor(DoorAnimation door)
    {
        if (currentDoor == door)
        {
            currentDoor = null;
            pressEText.SetActive(false);
        }
    }
}
