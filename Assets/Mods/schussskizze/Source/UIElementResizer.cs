﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIElementResizer : EventTrigger {

    private bool resizing;
    private RectTransform uiElement;
    private Vector2 lastMousePosition;
    private float currentHeight = 800;

	// Use this for initialization
	void Start () {
        uiElement = transform.parent.GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update () {
        if (resizing)
        {
            var mouseDelta = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - lastMousePosition;
            uiElement.sizeDelta = new Vector2(uiElement.sizeDelta.x + mouseDelta.x, uiElement.sizeDelta.y + mouseDelta.y);
            lastMousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }
	}

    public override void OnPointerDown(PointerEventData eventData)
    {
        resizing = true;
        lastMousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        resizing = false;
    }
}
