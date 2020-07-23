using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UBOAT.Mods.Schussskizze { 
    public class SketchTools : MonoBehaviour
    {
        private static bool toolIsActive = false;
        private List<Vector2> points;
        private Transform protractor;
        private float rotationSpeed
        {
            get
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    return 300f;
                }
                else
                {
                    return 50f;
                }
            }
        }
        private float translationSpeed
        {
            get
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    return 300f;
                }
                else
                {
                    return 50f;
                }
            }
        }

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

            protractor = transform.Find("CourseTriangle");
            var protractorButtonEvent = new Button.ButtonClickedEvent();
            protractorButtonEvent.AddListener(() => protractor.gameObject.SetActive(!protractor.gameObject.activeInHierarchy));
            transform.Find("CourseButton").GetComponent<Button>().onClick = protractorButtonEvent;
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.Q))
            {
                protractor.Rotate(new Vector3(0, 0, 1f), rotationSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.E))
            {
                protractor.Rotate(new Vector3(0, 0, -1f), rotationSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.W))
            {
                protractor.localPosition += new Vector3(0, translationSpeed * Time.deltaTime, 0);
            }
            if (Input.GetKey(KeyCode.A))
            {
                protractor.localPosition += new Vector3(translationSpeed * Time.deltaTime, 0, 0);
            }
            if (Input.GetKey(KeyCode.S))
            {
                protractor.localPosition += new Vector3(0, -translationSpeed * Time.deltaTime, 0);
            }
            if (Input.GetKey(KeyCode.D))
            {
                protractor.localPosition += new Vector3(-translationSpeed * Time.deltaTime, 0, 0);
            }
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
                Vector2 offset = GetTextOffset(points[0],points[1]);
                var angle = GetTextAngle(offset);
                var halfway = (points[0] + points[1]) / 2f;
                var distance = offset.magnitude * SketchArea.UItoGameScale;
                var true_angle = GetTrueAngle(points[0], points[1]);
                SketchArea.AddTextAtUIPos(halfway, (int)distance + "m " + (int)true_angle + "°", angle);
                stopTool();
            }
            else
            {
                // add first point and continue
                points.Add(position);
                SketchArea.AddSplatAtUIPos(position, "XMarker_Icon");              
            }       
        }

        public static Vector2 GetTextOffset(Vector2 v1, Vector2 v2)
        {
            if (v1.x < v2.x)
            {
                return v1 - v2;
            }
            else
            {
                return v2 - v1;
            }
        }

        public static float GetTextAngle(Vector2 offset)
        {
            return Vector2.Angle(offset, new Vector2(0, 1)) - 90;
        }

        public static float GetTrueAngle(Vector2 v1, Vector2 v2)
        {
            var true_angle = Vector2.Angle(v2 - v1, new Vector2(0, 1));
            if (v1.x > v2.x)
            {
                true_angle = 360 - true_angle;
            }
            return true_angle;
        }

        private void OnDestroy()
        {
            UIElementEvents.OnPointerClickInSketch -= onClick;
            transform.Find("RulerButton").GetComponent<Button>().onClick.RemoveAllListeners();
            SketchTools.toolIsActive = false;
        }
    }
}
