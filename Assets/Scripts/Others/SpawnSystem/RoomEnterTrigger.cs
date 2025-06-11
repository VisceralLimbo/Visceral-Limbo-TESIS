using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEnterTrigger : MonoBehaviour
{

    [SerializeField]GameObject SolidCollider;
    [SerializeField]RoomSpawnerManager roomSpawnerManager;


    void Start()
    {
        roomSpawnerManager= transform.root.GetComponentInChildren<RoomSpawnerManager>();

    }

    public void StartCombat(Collider other)
    {
        var Contex = other.GetComponentInParent<PlayerContext>();

        if (Contex.faction == FactionID.Player && roomSpawnerManager != null)
        {
            roomSpawnerManager.AssignPlayerContext(Contex);
            roomSpawnerManager.NotifyMinionDeath();

        }
    }

    private void OnTriggerExit(Collider other)
    {
        var Contex = other.GetComponentInParent<PlayerContext>();

        if (Contex.faction == FactionID.Player && roomSpawnerManager != null)
        {
                roomSpawnerManager.AssignPlayerContext(Contex);
                roomSpawnerManager.NotifyMinionDeath();
                
        }

    }
    public void SetSolidState(bool isSolid)
    {
        SolidCollider.SetActive(isSolid);
    }
}
