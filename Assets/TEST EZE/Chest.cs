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

        // activo anim
        if (chestAnimator != null)
        {
            chestAnimator.SetBool("isOpen", true);
        }

        //hago el random 
        LootItem loot = lootChest.GetRandomLoot();
        if (loot != null)
        {
            //para debugeo que me muestre el nombre y rareza (se puede borrar)
            Debug.Log($"Obtuviste: {loot.itemName} ({loot.rarity})");

            //instancio el item que toque
            if (loot.prefab != null)
            {
                Instantiate(loot.prefab, transform.position + Vector3.up, Quaternion.identity);
            }
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
