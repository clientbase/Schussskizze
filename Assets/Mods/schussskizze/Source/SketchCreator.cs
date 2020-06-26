using System.Collections.Generic;
using DWS.Common.Resources;
using UBOAT.Game.Core.Serialization;
using UBOAT.ModAPI.Core.InjectionFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UBOAT.Mods.Schussskizze
{
    [NonSerializedInGameState]
    public class SketchCreator : MonoBehaviour
    {
        [Inject]
        private static ResourceManager resourceManager;

        private List<GameObject> sketches = new List<GameObject>();

        public void Start()
        {
            Debug.Log("Sketch creator started.");
            var buttonObject = transform.Find("SketchButton");
            var onClick = new Button.ButtonClickedEvent();
            onClick.AddListener(CreateNewSketch);
            buttonObject.GetComponent<Button>().onClick = onClick;
        }

        public void CreateNewSketch()
        {
            Debug.Log("Creating new sketch.");
            var new_sketch = resourceManager.InstantiatePrefab("UI/SketchAreaPanel");
            new_sketch.transform.SetParent(this.transform, false);
            sketches.Add(new_sketch);
        }
    }
}
