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
    // matcheo con el nombre exacto
    private const string COMBAT_UI_CANVAS_NAME = "CombatUI";
    private const string CHEST_TEXT_NAME = "ChestText";
    private const string BG_CAN_AFFORD_NAME = "ChestTextBG_CanAfford";
    private const string BG_CANT_AFFORD_NAME = "ChestTextBG_CantAfford";

    private TextMeshProUGUI interactText;
    private GameObject interactTextObject;
    private Image bgCanAfford;
    private Image bgCantAfford;

    [Header("Material visual feedback")]
    [SerializeField] private List<Renderer> visualObjects = new List<Renderer>();
    [SerializeField] private List<ParticleSystem> particleEffects = new List<ParticleSystem>();
    [SerializeField] private List<Light> chestLights = new List<Light>();
    [SerializeField] private ParticleSystem openChestParticle;

    [SerializeField] SoundData _SoundNearByChest;
    [SerializeField] SoundData _SoundOpenChest;
    [SerializeField] SoundData _SoundCannotOpenChest;

    [Header("Nearby Sound System")]
    [SerializeField] private float nearbyRange = 6f;
    [SerializeField] private float soundCooldown = 1.5f;

    private Transform player;
    private float soundTimer = 0f;
    private SoundEmitter nearByEmitter;

    private void Start()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;

        GameObject combatUICanvas = GameObject.Find(COMBAT_UI_CANVAS_NAME);
        // busco el texto dentro del canvas

        if (combatUICanvas == null)
        {
            Debug.LogWarning("Chest: no se encontró CombatUI.");
            return;
        }

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

    private void Update()
    {
        if (isOpened || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // si esta dentro del rango
        if (dist <= nearbyRange)
        {
            soundTimer -= Time.deltaTime;

            if (soundTimer <= 0f)
            {
                PlayNearbySound();  // reproducimos
                soundTimer = soundCooldown;
            }
        }
    }

    private void PlayNearbySound()
    {
        if (nearByEmitter != null || _SoundNearByChest == null) return;

        SoundManager.Instance.CreateSound()
            .WithSoundData(_SoundNearByChest)
            .WithRandomPitch(true)
            .WithPosition(transform.position)
            .WithSpatialBlend(1f, 1f, 50f)
            .play(out nearByEmitter);
    }

    public void TryOpen()
    {
        // Evita abrir o cobrar varias veces.
        if (isOpened) return;

        // valido el costo
        if (cost < 0)
        {
            Debug.LogError("Chest: no puede ser negativo");
            return;
        }

        // valido la loot table
        if (lootChest == null)
        {
            Debug.LogError("Chest: no hay lootchest");
            return;
        }

        // elijo ANTES de cobrar
        LootItem loot = lootChest.GetRandomLoot();

        if (loot == null || loot.prefab == null)
        {
            Debug.LogWarning("Chest: no hay item, no se cobra");
            return;
        }

        // compruebo q tengo la plata sin cobrar todaiva
        if (!BloodEchoesManager.CanAfford(cost))
        {
            if (_SoundCannotOpenChest != null)
            {
                SoundManager.Instance.CreateSound()
                    .WithSoundData(_SoundCannotOpenChest)
                    .WithRandomPitch(true)
                    .WithPosition(transform.position)
                    .play();
            }

            HideInteractUI();
            return;
        }

        // bloqueo nuevas interacciones para no spamear g y salgan varios
        isOpened = true;

        // descuento
        BloodEchoesManager.PurchaseBloodEchoes(cost);

        HideInteractUI();
        OpenChest(loot);
    }

    private void OpenChest(LootItem loot)
    {
        if (nearByEmitter != null)
        {
            nearByEmitter.Stop();
            nearByEmitter = null;
        }

        if (_SoundOpenChest != null)
        {
            SoundManager.Instance.CreateSound()
                .WithSoundData(_SoundOpenChest)
                .WithRandomPitch(true)
                .WithPosition(transform.position)
                .play();
        }

        if (chestAnimator != null)
            chestAnimator.SetBool("isOpen", true);

        DisableVisuals();

        StartCoroutine(SpawnLootDelayed(0.5f, loot));
    }

    private IEnumerator SpawnLootDelayed(float delay, LootItem loot)
    {
        yield return new WaitForSeconds(delay);

        if (loot == null || loot.prefab == null)
        {
            Debug.LogError("Chest: loot dejo de ser valido antes de aparecer");
            yield break;
        }

        Vector3 spawnOffset = transform.up * 0.5f;

        GameObject spawned = Instantiate(
            loot.prefab,
            transform.position + spawnOffset,
            Quaternion.identity
        );

        spawned.transform.forward = transform.forward;

        if (spawned.GetComponent<LootFlyOut>() == null)
            spawned.AddComponent<LootFlyOut>();

        PlayOpenParticle();
    }

    public void OnInteract(RayCastWrapper Detector)
    {
        PlayerEvents.Interact();
        TryOpen();
    }

    public void OnRayCastEnter(RayCastWrapper Detector = null)
    {
        if (isOpened || interactTextObject == null || interactText == null) return;

        UpdateInteractText(true);
        PlayerEvents.InteractSeeing();
    }

    public void OnRayCastStay(RayCastWrapper Detector = null) { }

    public void OnRayCastExit(RayCastWrapper Detector = null)
    {
        HideInteractUI();
        PlayerEvents.InteractStopSeeing();
    }

    private void UpdateInteractText(bool activate)
    {
        if (isOpened || interactTextObject == null || interactText == null) return;

        if (activate)
        {
            bool canAfford = BloodEchoesManager.CanAfford(cost);

            if (canAfford)
            {
                interactText.text = "PRESIONA G PARA ABRIR";

                if (bgCanAfford != null)
                    bgCanAfford.gameObject.SetActive(true);

                if (bgCantAfford != null)
                    bgCantAfford.gameObject.SetActive(false);
            }
            else
            {
                interactText.text = $"NO TENES LOS BLOOD ECHOES SUFICIENTES ({cost})";

                if (bgCanAfford != null)
                    bgCanAfford.gameObject.SetActive(false);

                if (bgCantAfford != null)
                    bgCantAfford.gameObject.SetActive(true);
            }

            interactTextObject.SetActive(true);
        }
        else
        {
            HideInteractUI();
        }
    }

    private void HideInteractUI()
    {
        if (interactTextObject != null)
            interactTextObject.SetActive(false);

        if (bgCanAfford != null)
            bgCanAfford.gameObject.SetActive(false);

        if (bgCantAfford != null)
            bgCantAfford.gameObject.SetActive(false);
    }

    private void DisableVisuals()
    {
        // Apaga renderers
        foreach (Renderer rend in visualObjects)
            if (rend != null)
                rend.gameObject.SetActive(false);

        // Apaga particulas
        foreach (ParticleSystem ps in particleEffects)
            if (ps != null)
                ps.gameObject.SetActive(false);

        // Apaga luces
        foreach (Light l in chestLights)
            if (l != null)
                l.enabled = false;
    }

    private void PlayOpenParticle()
    {
        if (openChestParticle == null) return;

        openChestParticle.gameObject.SetActive(true);
        openChestParticle.Clear(true);
        openChestParticle.Play();
    }
}