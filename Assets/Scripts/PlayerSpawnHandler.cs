using UnityEngine;

public class PlayerSpawnHandler : MonoBehaviour
{
    [SerializeField] private Player_Movement playerMovement;

    [SerializeField] private Transform tutorialSpawn;
    [SerializeField] private Transform normalSpawn;

    void Start()
    {
        DungeonGenerator generator = FindObjectOfType<DungeonGenerator>();

        if (generator != null)
        {
            generator.OnSuccessfulGeneration += HandleSpawn;
        }
        else
        {
            HandleSpawn();
        }
    }

    void HandleSpawn()
    {
        if (!TutorialManager.HasSeenTutorial())
        {
            // primera vez al tuto
            playerMovement.SetCharacterPosition(tutorialSpawn.position);
            BloodEchoesManager.AddBloodEchoes(50);
        }
        else
        {
            // si ya jugo al normal
            playerMovement.SetCharacterPosition(normalSpawn.position);
        }
    }
}
