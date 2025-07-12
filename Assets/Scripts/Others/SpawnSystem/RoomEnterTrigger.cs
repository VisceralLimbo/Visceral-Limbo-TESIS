using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEnterTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Collider SolidCollider;
    [SerializeField] Collider DoorTrigger;
    [SerializeField] RoomSpawnerManager roomSpawnerManager;

    public bool IsInCombat;

    void Start()
    {
        roomSpawnerManager= transform.root.GetComponentInChildren<RoomSpawnerManager>();

    }

    public void StartCombat(Collider other)
    {
        var Contex = other.GetComponentInParent<PlayerContext>();

        if (Contex.faction == FactionID.Player && roomSpawnerManager != null)
        {
            PlayerEnteredRoom();
            IsInCombat = true;
            
        }
    }
    

    PlayerContext Pcontext;
    private void OnTriggerEnter(Collider other)
    {
        var Contex = other.GetComponentInParent<PlayerContext>();
        if(Contex == null)
        {
            return;
        }

        if (Contex.faction == FactionID.Player && roomSpawnerManager != null)
        {
            Pcontext = Contex;     
        }

    }
    public void SetSolidState(bool isSolid)
    {
        SolidCollider.isTrigger= isSolid;

    }

    public void PlayerEnteredRoom()
    {
        roomSpawnerManager.AssignPlayerContext(Pcontext);
        roomSpawnerManager.StartRoomCombat();
        roomSpawnerManager.NotifyMinionDeath();
        SetSolidState(false);
    }

    public void SetCombatState(bool state)
    {
        IsInCombat= state;
    }
}
