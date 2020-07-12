using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SketchTools : MonoBehaviour
{
    private static bool toolIsActive = false;

    public enum ToolType
    {
        Ruler,
        Protractor,
        Compass
    }

    public static ToolType CurrentTool;
    public static bool ToolIsActive => toolIsActive;

    private void Start()
    {
        var rulerButtonEvent = new Button.ButtonClickedEvent();
        rulerButtonEvent.AddListener(() => useTool(ToolType.Ruler));
        transform.Find("RulerButton").GetComponent<Button>().onClick = rulerButtonEvent;
        UIElementEvents.OnPointerClickInSketch += onClick;
    }

    private void onClick(Vector2 position)
    {
        if (!toolIsActive) return;

        switch (CurrentTool)
        {
            case ToolType.Ruler:
                rulerClick(position);
                break;
            case ToolType.Protractor:
                break;
            case ToolType.Compass:
                break;
            default:
                break;
        }
    }

    private void useTool(ToolType tool)
    {
        CurrentTool = tool;
        toolIsActive = true;
    }

    private void stopTool()
    {
        toolIsActive = false;
    }

    void rulerClick(Vector2 position)
    {
        Debug.Log("Handle Ruler Click at: " + position);
        stopTool();
    }

}
