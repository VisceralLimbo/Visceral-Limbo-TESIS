using UnityEngine;

public class RoomDiscovery : MonoBehaviour
{
    [Header("FOW")]
    [SerializeField] private GameObject[] fogObjects;

    private bool isDiscovered = false;

    private void OnTriggerEnter(Collider other)
    {
        // si ya se descubrio o no es el player vuelvo
        if (isDiscovered || !other.CompareTag("Player")) return;

        if (fogObjects != null && fogObjects.Length > 0)
        {
            // explorado
            isDiscovered = true;


            foreach (GameObject fog in fogObjects)
            {
                if (fog != null)
                {
                    fog.SetActive(false);
                }
            }
            Destroy(this);
        }
    }
}
