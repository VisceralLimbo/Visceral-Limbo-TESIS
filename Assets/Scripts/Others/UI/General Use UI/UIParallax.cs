using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIParallax : MonoBehaviour
{
    [System.Serializable]
    public class UIItem
    {
        public RectTransform rect;
        public float intensity = 1f;

        [HideInInspector] public Vector2 initialPos;
        [HideInInspector] public Vector2 currentOffset;
    }

    [Header("UI Elements")]
    [SerializeField] private List<UIItem> uiItems = new List<UIItem>();

    [Header("Parallax Settings")]
    [SerializeField] private float movementAmount = 2f;
    [SerializeField] private float smoothSpeed = 10f;

    [Header("Attack Shake Settings")]
    [SerializeField] private float attackShakeDuration = 0.12f;
    [SerializeField] private float attackShakeStrength = 8f;

    [Header("References")]
    [SerializeField] private Player_MeleeAttack meleeAttack;

    private float shakeTimer;
    private Vector2 shakeOffset;

    [Header("Attack Recoil Movement")]
    [SerializeField] private float attackMoveDistance = 20f;
    [SerializeField] private float attackReturnSpeed = 8f;

    private Vector2 recoilOffset;

    void Start()
    {
        foreach (var item in uiItems)
        {
            if (item.rect != null)
                item.initialPos = item.rect.anchoredPosition;
        }
    }

    void Update()
    {
        

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Vector2 mouseDelta = new Vector2(mouseX, mouseY);

        

        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

           
            float fade = shakeTimer / attackShakeDuration; //un pequeño shake

            shakeOffset = Random.insideUnitCircle * attackShakeStrength * fade;
        }
        else
        {
            shakeOffset = Vector2.zero;
        }

        recoilOffset = Vector2.Lerp(
          recoilOffset,
          Vector2.zero,
          Time.deltaTime * attackReturnSpeed
);

        foreach (var item in uiItems)
        {
            if (item.rect == null) continue;

            Vector2 targetOffset = new Vector2(
                -mouseDelta.x,
                 mouseDelta.y
            ) * movementAmount * 50f * item.intensity;

            item.currentOffset = Vector2.Lerp(
                item.currentOffset,
                targetOffset,
                Time.deltaTime * smoothSpeed
            );

            item.rect.anchoredPosition =
            item.initialPos +
            item.currentOffset +
            recoilOffset +
            shakeOffset;
        }
    }


    private void OnEnable()
    {
        if (meleeAttack != null)
            meleeAttack.OnMeleeAttackCompleted += PlayAttackShake;
    }

    private void OnDisable()
    {
        if (meleeAttack != null)
            meleeAttack.OnMeleeAttackCompleted -= PlayAttackShake;
    }

    private void PlayAttackShake()
    {
        shakeTimer = attackShakeDuration;

        
        Vector2 mouseDir = new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y") //se usa el movimiento con direccion del mouse
        );

        if (mouseDir.sqrMagnitude < 0.01f)
        {
            
            mouseDir = Random.insideUnitCircle;
        }

        mouseDir.Normalize();

        recoilOffset = mouseDir * attackMoveDistance;
    }
}
