using System;
using System.Collections.Generic;
using System.Reflection;

using DWS.Common.InjectionFramework;
using DWS.Common.Resources;

using Harmony;

using UBOAT.Game.Core.Serialization;
using UBOAT.Game.Sandbox;
using UBOAT.Game.Scene.Entities;
using UBOAT.Game.UI;
using UBOAT.ModAPI;
using UBOAT.ModAPI.Core.InjectionFramework;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace UBOAT.Mods.Schussskizze
{
    [NonSerializedInGameState]
    public class Schussskizze : IUserMod
    {
        [Inject]
        private static ResourceManager resourceManager;
        [Inject]
        private static PlayerShip playerShip;
        private List<DirectObservation> observations = new List<DirectObservation>();
        private GameObject UI;

        private void showNewSketchButton(bool show)
        {
            UI.transform.Find("NewSketchButton").gameObject.SetActive(show);
        }

        private void onAlarmStarted()
        {
            Debug.Log("Schussskizze - Alarm!");
            showNewSketchButton(true);
        }

        private void onAlarmStoped()
        {
            Debug.Log("Schussskizze - Alarm stopped!");
            showNewSketchButton(false);
        }

        private void onObservationChanded(DirectObservation observation)
        {
            Debug.Log("Schussskizze - Observation Changed");
            logObservation(observation);
        }

        private void logObservation(DirectObservation observation)
        {
            Debug.Log("Schussskizze - Observation: Name: " + observation.Entity.Name + ", Catagory: " + observation.Entity.Category + "\n" +
                    "Estimated Speed: " + observation.EstimatedVelocity + "\n" +
                    "Estimated Course: " + observation.EstimatedCourse + "\n" +
                    "Estimated Distance: " + observation.EstimatedDistance + "\n" +
                    "Entity Position: " + observation.Entity.transform.position.ToString() + "\n" +
                    "Entity Distance: " + (observation.Entity.transform.position - playerShip.transform.position).magnitude + " meters" + "\n" +
                    "Crew Accuracy: " + playerShip.CrewAccuracy.Value
                    );
        }

        private void onObservationAdded(Observator observer, DirectObservation observation)
        {
            Debug.Log("Schussskizze - Observation added! Name:" + observation.Entity.Name + " (total: " + playerShip.GetObservationsDirect().Count + ")");
            if (observation.Entity.Country.GetRelationWith(playerShip.Country) == Country.Relation.Enemy)
            {
                logObservation(observation);
                observation.Changed += onObservationChanded;
                observations.Add(observation);
            }
        }

        private void onObservationRemoved(Observator observer, DirectObservation observation)
        {
            Debug.Log("Schussskizze - Observation removed! Name: " + observation.Entity.Name + " (total: " + playerShip.GetObservationsDirect().Count + ")");
            if (observations.Contains(observation))
            {
                Debug.Log("Observation in list removed!");
            }
        }

        public void wireEvents()
        {
            playerShip.AlarmStarted += onAlarmStarted;
            playerShip.AlarmStopped += onAlarmStoped;
            playerShip.ObservationAdded += onObservationAdded;
            playerShip.ObservationAdded += onObservationRemoved;
        }

        public void OnLoaded()
        {
            Debug.Log("Schussskizze mod is active!");

            var harmony = HarmonyInstance.Create("com.dws.uboat");
            harmony.PatchAll();

            SceneEventsListener.OnSceneAwake += onSceneAwake;
        }

        private void onSceneAwake(Scene scene)
        {
            try
            {
                InjectionFramework.Instance.InjectIntoAssembly(Assembly.GetExecutingAssembly());

                if (scene.name == "Main Scene")
                {
                    Debug.Log("Main Scene has loaded.");
                    var schussskizze = resourceManager.InstantiatePrefab("UI/SchussskizzeCanvas");
                    var viewport = GameObject.Find("/GUI/Viewport").transform;
                    schussskizze.transform.SetParent(viewport, false);
                    UI = schussskizze;
                    showNewSketchButton(false);
                    wireEvents();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Exeption caught: ");
                Debug.LogException(e);
            }
        }
    }
}
