using UnityEngine;
using UnityEngine.UI;

public class SliderAnimTrigger : MonoBehaviour
{
    public Slider slider;
    public Animator animator;

    private float lastValue;
    private bool initialized = false;

    void Start()
    {
        lastValue = slider.value;
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

        lastValue = value;
    }
}


