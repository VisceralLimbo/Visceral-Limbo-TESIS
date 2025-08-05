using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BloodScreenManager : MonoBehaviour
{
    public static BloodScreenManager Instance;

    [Header("Settings")]
    public List<Sprite> bloodSprites; 
    public GameObject bloodPrefab;    
    public Canvas canvas;             

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowRandomBloodSplash()
    {
        if (bloodSprites.Count == 0 || bloodPrefab == null || canvas == null) return;

        GameObject blood = Instantiate(bloodPrefab, canvas.transform);
        Image img = blood.GetComponent<Image>();

        
        img.sprite = bloodSprites[Random.Range(0, bloodSprites.Count)];

        
        RectTransform rect = blood.GetComponent<RectTransform>();
        float x = Random.Range(-canvas.pixelRect.width / 2f, canvas.pixelRect.width / 2f);
        float y = Random.Range(-canvas.pixelRect.height / 2f, canvas.pixelRect.height / 2f);
        rect.anchoredPosition = new Vector2(x, y);

       
        rect.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 360f));
        float scale = Random.Range(0.8f, 1.5f);
        rect.localScale = new Vector3(scale, scale, 1f);

        
        blood.AddComponent<FadeAndDestroy>();
    }
}


