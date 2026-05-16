using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ArrowsUI : MonoBehaviour
{
    public RectTransform leftArrow;
    public RectTransform rightArrow;
    public float smoothSpeed = 15f;

    //target actual de las flechitas
    private RectTransform currentTarget;

    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            RectTransform target = EventSystem.current.currentSelectedGameObject.GetComponent<RectTransform>();

            if (target != null)
            {
                currentTarget = target;
                UpdateArrowsSmooth();
            }
        }
        else
        {
            HideArrows();
        }
    }

    void UpdateArrowsSmooth()
    {
        // prendo flechitas
        leftArrow.gameObject.SetActive(true);
        rightArrow.gameObject.SetActive(true);

        // agarro la altura del boton en el que este
        float buttonHeight = currentTarget.rect.height;

        // y las distancia (el espacio q se deja para el boton)
        float offsetX = currentTarget.rect.width / 2f + 20f;

        // la pos
        Vector3 leftTargetPos = new Vector3(currentTarget.position.x - offsetX, currentTarget.position.y, currentTarget.position.z);
        Vector3 rightTargetPos = new Vector3(currentTarget.position.x + offsetX, currentTarget.position.y, currentTarget.position.z);

        // "animacion" ponele es para que se mueva mas bonito en el menu 
        leftArrow.position = Vector3.Lerp(leftArrow.position, leftTargetPos, Time.unscaledDeltaTime * smoothSpeed);
        rightArrow.position = Vector3.Lerp(rightArrow.position, rightTargetPos, Time.unscaledDeltaTime * smoothSpeed);

        // y la altura tmb 
        Vector2 newSize = new Vector2(leftArrow.sizeDelta.x, buttonHeight);
        leftArrow.sizeDelta = Vector2.Lerp(leftArrow.sizeDelta, newSize, Time.unscaledDeltaTime * smoothSpeed);
        rightArrow.sizeDelta = Vector2.Lerp(rightArrow.sizeDelta, newSize, Time.unscaledDeltaTime * smoothSpeed);
    }

    public void HideArrows()
    {
        //apago xd
        leftArrow.gameObject.SetActive(false);
        rightArrow.gameObject.SetActive(false);

        // reset de la referencia
        currentTarget = null;
    }
}
