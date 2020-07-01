using System;
using System.Collections.Generic;
using System.Reflection;

using DWS.Common.InjectionFramework;
using DWS.Common.Resources;

using Harmony;

using UBOAT.Game.Core.Serialization;
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

        private void onAlarmStarted()
        {
            Debug.Log("Schussskizze - Alarm!");
        }

        private void onAlarmStoped()
        {
            Debug.Log("Schussskizze - Alarm stopped!");
        }

        private void onObservationAdded(Observator observer, DirectObservation observation)
        {
            Debug.Log("Schussskizze - Observation added! Name:" + observation.Entity.Name + " (total: " + playerShip.GetObservationsDirect().Count + ")");
        }

        private void onObservationRemoved(Observator observer, DirectObservation observation)
        {
            Debug.Log("Schussskizze - Observation removed! Name:" + observation.Entity.Name + " (total: " + playerShip.GetObservationsDirect().Count + ")");
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
