using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIElementEvents : EventTrigger
{
    private static bool isPointerInSketch = false;

    public static Action<Vector2> OnPointerDownInSketch;
    public static Action<Vector2> OnPointerClickInSketch;
    public static bool IsPointerInSketch => isPointerInSketch;

    public override void OnPointerDown(PointerEventData eventData)
    {
        var viewPortPosition = this.GetComponent<RectTransform>().InverseTransformPoint(eventData.position);
        Debug.Log("Pointer Down Viewport Position: " + viewPortPosition);
        if (OnPointerDownInSketch != null)
        {
            OnPointerDownInSketch(viewPortPosition);
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        var viewPortPosition = this.GetComponent<RectTransform>().InverseTransformPoint(eventData.position);
        Debug.Log("Pointer Click Viewport Position: " + viewPortPosition);
        if (OnPointerClickInSketch != null)
        {
            OnPointerClickInSketch(viewPortPosition);
        }
    }

    public override void OnPointerEnter(PointerEventData data)
    {
        Debug.Log("Pointer Enter");
        isPointerInSketch = true;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Pointer Exit");
        isPointerInSketch = false;
    }
}
