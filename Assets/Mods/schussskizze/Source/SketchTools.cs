using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UBOAT.Mods.Schussskizze { 
    public class SketchTools : MonoBehaviour
    {
        private static bool toolIsActive = false;
        private List<Vector2> points;

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
            points = new List<Vector2>();
        }

        private void stopTool()
        {
            toolIsActive = false;
        }

        void rulerClick(Vector2 position)
        {
            Debug.Log("Handle Ruler Click at: " + position);
            if (points.Count == 1)
            {
                // seccond point and complete
                points.Add(position);
                SketchArea.AddSplatAtUIPos(position, "XMarker_Icon");
                SketchArea.DrawLineWithUICoords(points[0], points[1]);
                var halfway = (points[0] + points[1]) / 2f;
                SketchArea.AddTextAtUIPos(halfway, "420m");
                stopTool();
            }
            else
            {
                // add first point and continue
                points.Add(position);
                SketchArea.AddSplatAtUIPos(position, "XMarker_Icon");              
            }       
        }

    }
}
