﻿using System;
using System.Collections.Generic;
using DWS.Common.Resources;
using UBOAT.Game.Core.Serialization;
using UBOAT.Game.Core.Time;
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
        [Inject]
        private static GameTime gameTime;
        private List<GameObject> sketches = new List<GameObject>();
        private SketchArea activeSketch;
        private GameObject ShowSketchButtonObject;

        public void Start()
        {
            Debug.Log("Sketch creator started.");
            var buttonObject = transform.Find("NewSketchButton");
            var onClick = new Button.ButtonClickedEvent();
            onClick.AddListener(CreateNewSketch);
            buttonObject.GetComponent<Button>().onClick = onClick;

            //ShowSketchButtonObject = transform.Find("ShowSketchButton").gameObject;
            //var onClickShow = new Button.ButtonClickedEvent();
            //onClickShow.AddListener(showActive);
            //ShowSketchButtonObject.GetComponent<Button>().onClick = onClickShow;
        }

        public void CreateNewSketch()
        {
            Debug.Log("Creating new sketch.");
            var new_sketch = resourceManager.InstantiatePrefab("UI/SketchAreaPanel");
            new_sketch.transform.SetParent(this.transform, false);
            sketches.Add(new_sketch);
            activeSketch = new_sketch.GetComponentInChildren<SketchArea>();

            var dateTimeObject = new_sketch.transform.Find("SketchHeader/DateTimeText");
            dateTimeObject.GetComponent<Text>().text = new DateTime(gameTime.StoryTicks).ToString(gameTime.LongDateTimeFormat);

            //var hideButtonOject = new_sketch.transform.Find("HideButton");
            //var onClickHide = new Button.ButtonClickedEvent();
            //onClickHide.AddListener(hideActive);
            //hideButtonOject.GetComponent<Button>().onClick = onClickHide;
        }

        public void hideActive()
        {
            activeSketch.transform.parent.gameObject.SetActive(false);
            ShowSketchButtonObject.SetActive(true);
        }

        public void showActive()
        {
            activeSketch.transform.parent.gameObject.SetActive(true);
            ShowSketchButtonObject.SetActive(false);
        }
    }
}
