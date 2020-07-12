using System.Collections.Generic;
using UBOAT.Mods.Schussskizze;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIElementDragger : EventTrigger
{
    private GraphicRaycaster canvasRaycaster;
    private bool dragging;

    public void Update()
    {
        if (SketchTools.ToolIsActive)
        {
            dragging = false;
        }

        if (dragging)
        {
            this.GetComponent<RectTransform>().position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        dragging = true;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        dragging = false;
    }
}