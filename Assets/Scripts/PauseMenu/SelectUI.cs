using UnityEngine;
using UnityEngine.EventSystems;

public class SelectUI : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        // fuerzo a q por donde paso el mouse lo tome como seleccion, no me dejaba ver las flechas si no 
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
