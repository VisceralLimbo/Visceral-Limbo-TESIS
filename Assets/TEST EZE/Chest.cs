using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class Chest : MonoBehaviour, IRaycastInteractable
{
    [Header("cofre config")]
    public int cost = 50;
    public LootChest lootChest;
    private bool isOpened = false;

    [Header("animator")]
    [SerializeField] private Animator chestAnimator;

    [Header("Feedback de UI")]
    // para no tener el quilmobo de arratras las cosas matcheo con el nombre exacto y fue
    private const string COMBAT_UI_CANVAS_NAME = "CombatUI";
    private const string CHEST_TEXT_NAME = "ChestText";

    // lo mismo de arriba pero ahora para las imagenes
    private const string BG_CAN_AFFORD_NAME = "ChestTextBG_CanAfford";
    private const string BG_CANT_AFFORD_NAME = "ChestTextBG_CantAfford"; 

    private TextMeshProUGUI interactText; //el texto
    private GameObject interactTextObject; // donde esta

    private Image bgCanAfford; // podes
    private Image bgCantAfford; // no podes

    [Header("Material visual feedback")]
    [SerializeField] private List<Renderer> visualObjects = new List<Renderer>();
    [SerializeField] private List<ParticleSystem> particleEffects = new List<ParticleSystem>();
    [SerializeField] private List<Light> chestLights = new List<Light>();
    [SerializeField] private ParticleSystem openChestParticle;



    private void Start()
    {
        // busco canvas
        GameObject combatUICanvas = GameObject.Find(COMBAT_UI_CANVAS_NAME);
        // busco el texto dentro del canvas
        Transform textTransform = combatUICanvas.transform.Find(CHEST_TEXT_NAME);

        if (textTransform != null)
        {
            interactTextObject = textTransform.gameObject;
            // saco el componente del texto
            interactText = interactTextObject.GetComponent<TextMeshProUGUI>();
            // lo apago al inciar
            interactTextObject.SetActive(false);
        }

        // busco y apago los fondos
        // primero el verde dsp el rojo
        Transform bgCanTransform = combatUICanvas.transform.Find(BG_CAN_AFFORD_NAME);
        if (bgCanTransform != null)
        {
            bgCanAfford = bgCanTransform.GetComponent<Image>();
            bgCanAfford.gameObject.SetActive(false);
        }

        Transform bgCantTransform = combatUICanvas.transform.Find(BG_CANT_AFFORD_NAME);
        if (bgCantTransform != null)
        {
            bgCantAfford = bgCantTransform.GetComponent<Image>();
            bgCantAfford.gameObject.SetActive(false);
        }
    }

    public void TryOpen()
    {
        if (isOpened) return;

        // chequeo si se puede comprar
        if (BloodEchoesManager.CanPurchase(cost))
        {
            isOpened = true;
            OpenChest();
        }

        // oculto dsp de intentar interactuar
        if (interactTextObject != null)
        {
            interactTextObject.SetActive(false);
        }
    }

    private void OpenChest()
    {
        isOpened = true;

        if (chestAnimator != null)
            chestAnimator.SetBool("isOpen", true);

        DisableVisuals();

       

        StartCoroutine(SpawnLootDelayed(0.5f)); 
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
            PlayOpenParticle();
        }
    }

    public void OnInteract(RayCastWrapper Detector)
    {
        TryOpen();
    }

    public void OnRayCastEnter(RayCastWrapper Detector = null)
    {
        if (isOpened || interactTextObject == null || interactText == null) return;
        UpdateInteractText(true);
    }

    public void OnRayCastStay(RayCastWrapper Detector = null)
    {
    }

    public void OnRayCastExit(RayCastWrapper Detector = null)
    {
        if (interactTextObject != null)
        {
            interactTextObject.SetActive(false);
        }

        if (bgCanAfford != null)
        {
            bgCanAfford.gameObject.SetActive(false);
        }

        if (bgCantAfford != null)
        {
            bgCantAfford.gameObject.SetActive(false);
        }
    }

    private void UpdateInteractText(bool activate)
    {
        // controlo el msg q aparece, si tenes "toca g" si no tenes "no tenes"

        if (isOpened || interactTextObject == null || interactText == null) return;

        if (activate)
        {
            // chequea y no consume
            bool canAfford = BloodEchoesManager.CanAfford(cost);

            if (canAfford)
            {
                interactText.text = "PRESIONA G PARA ABRIR";
                // activo podes y apago no podes
                if (bgCanAfford != null)
                {
                    bgCanAfford.gameObject.SetActive(true);
                }

                if (bgCantAfford != null)
                {
                    bgCantAfford.gameObject.SetActive(false);
                }
            }
            else
            {
                interactText.text = $"NO TENES LOS BLOOD ECHOES SUFICIENTES ({cost})";
                // biceversa
                if (bgCanAfford != null)
                {
                    bgCanAfford.gameObject.SetActive(false);
                }

                if (bgCantAfford != null)
                {
                    bgCantAfford.gameObject.SetActive(true);
                }
            }

            interactTextObject.SetActive(true);
        }
        else
        {
            interactTextObject.SetActive(false);
            // y apago cuando se apaga el texto
            if (bgCanAfford != null)
            {
                bgCanAfford.gameObject.SetActive(false);
            }

            if (bgCantAfford != null)
            {
                bgCantAfford.gameObject.SetActive(false);
            }
        }
    }

    private void DisableVisuals()
    {
        // Apaga renderers
        foreach (Renderer rend in visualObjects)
        {
            if (rend != null)
                rend.gameObject.SetActive(false);
        }

        // Apaga partículas
        foreach (ParticleSystem ps in particleEffects)
        {
            if (ps != null)
                ps.gameObject.SetActive(false);
        }

        // Apaga luces
        foreach (Light l in chestLights)
        {
            if (l != null)
                l.enabled = false;
        }
    }

    private void PlayOpenParticle()
    {
        if (openChestParticle == null) return;

        openChestParticle.gameObject.SetActive(true);
        openChestParticle.Clear(true);
        openChestParticle.Play();
    }
}
    
