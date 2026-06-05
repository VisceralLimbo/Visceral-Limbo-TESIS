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
    private GameObject lastSelectedObject;

    void Update()
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        // si hay seleccion actual
        if (selected != null)
        {
            RectTransform target = selected.GetComponent<RectTransform>();

            if (target != null)
            {
                currentTarget = target;
                lastSelectedObject = selected;

                UpdateArrowsSmooth();
            }
        }
        else
        {
            // si habia uno guardado y esta activo
            if (lastSelectedObject != null &&
                lastSelectedObject.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(lastSelectedObject);
            }
            else
            {
                // limpio todo si el objeto ya no existe o esta oculto
                currentTarget = null;
                lastSelectedObject = null;

                HideArrows();
            }
        }
    }

    void UpdateArrowsSmooth()
    {
        leftArrow.gameObject.SetActive(true);
        rightArrow.gameObject.SetActive(true);

        Vector3[] corners = new Vector3[4];
        currentTarget.GetWorldCorners(corners);

        // corners:
        // 0 = abajo izquierda
        // 1 = arriba izquierda
        // 2 = arriba derecha
        // 3 = abajo derecha

        Vector3 leftCenter = (corners[0] + corners[1]) / 2f;
        Vector3 rightCenter = (corners[2] + corners[3]) / 2f;

        float padding = 20f;

        Vector3 leftTargetPos = leftCenter + Vector3.left * padding;
        Vector3 rightTargetPos = rightCenter + Vector3.right * padding;

        leftArrow.position = Vector3.Lerp(
            leftArrow.position,
            leftTargetPos,
            Time.unscaledDeltaTime * smoothSpeed
        );

        rightArrow.position = Vector3.Lerp(
            rightArrow.position,
            rightTargetPos,
            Time.unscaledDeltaTime * smoothSpeed
        );

        // altura
        float buttonHeight = Vector3.Distance(corners[0], corners[1]);

        Vector2 newSize = new Vector2(leftArrow.sizeDelta.x, buttonHeight);

        leftArrow.sizeDelta = Vector2.Lerp(
            leftArrow.sizeDelta,
            newSize,
            Time.unscaledDeltaTime * smoothSpeed
        );

        rightArrow.sizeDelta = Vector2.Lerp(
            rightArrow.sizeDelta,
            newSize,
            Time.unscaledDeltaTime * smoothSpeed
        );
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
