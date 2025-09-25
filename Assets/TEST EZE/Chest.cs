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
            //instancio el item que toque
            if (loot.prefab != null)
            {
                // hice q los items spawneen un toque adelante para que no pase eso de que no se pueden agarrar, quiza era mejor agranadar simplemente el collider pero bue 
                Vector3 spawnOffset = transform.up * 0.5f + transform.forward * 1f;
                Instantiate(loot.prefab, transform.position + spawnOffset, Quaternion.identity);
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
