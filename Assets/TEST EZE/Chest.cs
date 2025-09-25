using System.Collections;
using UnityEngine;

public class Chest : MonoBehaviour, IRaycastInteractable
{
    [Header("cofre config")]
    public int cost = 150;
    public LootChest lootChest;
    private bool isOpened = false;

    [Header("animator")]
    [SerializeField] private Animator chestAnimator;

    public void TryOpen()
    {
        if (isOpened) return;

        // chequeo si se puede comprar
        if (BloodEchoesManager.CanPurchase(cost))
        {
            isOpened = true;
            BloodEchoesManager.PurchaseBloodEchoes(cost); // descuenta y llamo el evento
            OpenChest();
        }
        else
        {
            Debug.Log("No tenés suficientes Ecos de Sangre");
        }
    }

    private void OpenChest()
    {
        isOpened = true;

        if (chestAnimator != null)
            chestAnimator.SetBool("isOpen", true);

        StartCoroutine(SpawnLootDelayed(0.4f)); 
    }

    private IEnumerator SpawnLootDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        LootItem loot = lootChest.GetRandomLoot();
        if (loot != null && loot.prefab != null)
        {
            Vector3 spawnOffset = transform.up * 0.5f;
            GameObject spawned = Instantiate(loot.prefab, transform.position + spawnOffset, Quaternion.identity);
            spawned.transform.forward = transform.forward;
            spawned.AddComponent<LootFlyOut>();
        }
    }

    public void OnInteract()
    {
        TryOpen();
    }

    public void OnRayCastEnter(RayCastWrapper Detector = null)
    {

    }

    public void OnRayCastStay(RayCastWrapper Detector = null)
    {
        
    }

    public void OnRayCastExit(RayCastWrapper Detector = null)
    {

    }
}
