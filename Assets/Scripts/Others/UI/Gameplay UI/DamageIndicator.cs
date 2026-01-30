using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DamageIndicator : MonoBehaviour
{
    [SerializeField] private Vector3 damageLocation;

    [SerializeField] private Transform playerObject;

    [SerializeField] private Transform damageImagePivot;

    [SerializeField] private CanvasGroup damageImageCanvasGroup;

    [SerializeField] private float fadeStartTime, fadeTime;

    private float maxFadeTime;

    private void Start()
    {
        maxFadeTime = fadeTime;
    }

    private void Update()
    {
        if (fadeStartTime > 0)
        {
            fadeStartTime -= Time.deltaTime;
        }
        else
        {
            fadeTime -= Time.deltaTime;
            damageImageCanvasGroup.alpha = fadeTime / maxFadeTime;

            if (fadeTime <= 0)
            {
                Destroy(gameObject);
            }
        }

            damageLocation.y = playerObject.position.y;
        Vector3 Direction = (damageLocation - playerObject.position).normalized;
        float angle = (Vector3.SignedAngle(Direction, playerObject.forward, Vector3.up));
        damageImagePivot.transform.localEulerAngles = new Vector3(0, 0, angle);
    }

    public void SetDamageLocation(Vector3 location, Transform player)
    {
        damageLocation = location;
        playerObject = player;
    }

}
