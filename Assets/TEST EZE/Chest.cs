using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("cofre config")]
    public int cost = 50;
    public LootChest lootChest;
    private bool isOpened = false;
    private bool playerInRange = false;

    [Header("animator")]
    [SerializeField] private Animator chestAnimator;

    private void Update()
    {
        if (playerInRange && !isOpened && Input.GetKeyDown(KeyCode.G))
        {
            TryOpen();
        }
    }

    public void TryOpen()
    {
        if (isOpened) return;

        // chequeo si se puede comprar
        if (BloodEchoesManager.BloodEchoes >= cost)
        {
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

        // le apago el collider despues de usarlo porque si spameaba podia instanciar dos
        GetComponent<Collider>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
