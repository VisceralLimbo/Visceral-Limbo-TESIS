using UnityEngine;
using UnityEngine.UI;

public class SliderAnimTrigger : MonoBehaviour
{
    public Slider slider;
    public Animator animator;

    private float lastValue;
    private bool initialized = false;
    private bool isOnCooldown = false;

    public Image cooldownImage1;
    public Image cooldownImage2;
    public Image cooldownImage3;
    public Image fireImage;


    void Start()
    {
        lastValue = slider.value;

        if (cooldownImage1 != null) cooldownImage1.gameObject.SetActive(false);
        if (cooldownImage2 != null) cooldownImage2.gameObject.SetActive(false);
        if (cooldownImage3 != null) cooldownImage3.gameObject.SetActive(false);
        if (fireImage != null) fireImage.gameObject.SetActive(true);
    }

    void Update()
    {
        float value = slider.value;

        if (!initialized)
        {
            if (value != lastValue)
            {
                initialized = true;
            }
            lastValue = value;
            return;
        }

        if (value == 0f && lastValue > 0f)
        {
            animator.SetTrigger("Mostrar");
        }


        if (value > 0f)
        {
            if (cooldownImage1 != null) cooldownImage1.gameObject.SetActive(true);
            if (cooldownImage2 != null) cooldownImage2.gameObject.SetActive(true);
            if (cooldownImage3 != null) cooldownImage3.gameObject.SetActive(true);
            if (fireImage != null) fireImage.gameObject.SetActive(false);
        }
        else 
        {
            if (cooldownImage1 != null) cooldownImage1.gameObject.SetActive(false);
            if (cooldownImage2 != null) cooldownImage2.gameObject.SetActive(false);
            if (cooldownImage3 != null) cooldownImage3.gameObject.SetActive(false);
            if (fireImage != null) fireImage.gameObject.SetActive(true);
        }

        lastValue = value;
    }
}


