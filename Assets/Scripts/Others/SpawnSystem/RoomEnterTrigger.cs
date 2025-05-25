using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEnterTrigger : MonoBehaviour
{

    [SerializeField]GameObject SolidCollider;
    [SerializeField]RoomSpawnerManager roomSpawnerManager;
    [SerializeField] bool IsSolid;


    void Start()
    {
        roomSpawnerManager= transform.root.GetComponentInChildren<RoomSpawnerManager>();
        SolidCollider.SetActive(false);

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
