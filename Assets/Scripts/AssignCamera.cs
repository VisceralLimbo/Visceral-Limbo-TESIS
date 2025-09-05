using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignCamera : MonoBehaviour
{
    [RequireComponent(typeof(Canvas))]
    public class AssignWorldCanvasCamera : MonoBehaviour
    {
        void Awake()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null)
            {
                canvas.worldCamera = Camera.main;
            }
        }
    }
}
