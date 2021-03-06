﻿using System;
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
        [Inject]
        private static IExecutionQueue executionQueue;

        private static List<DirectObservation> observations = new List<DirectObservation>();
        private GameObject UI;
        private static float playerPositionUpdateTime = 300f;
        public static float TrackPositionUpdateTime = 300f;

        public static Action<Vector3> OnPlayerPosition;
        public static Action OnAlarmStopped;
        public static Action OnAlarmStarted;
        public static Vector3 PlayerPostion => new Vector3(playerShip.SandboxEntity.Position.x, playerShip.SandboxEntity.Position.y, 0);
        public static Vector2 PlayerPostion2D => playerShip.SandboxEntity.Position;
        public static float CrewAccuracy => playerShip.CrewAccuracy;
        public static DirectObservation[] Observations => observations.ToArray();
        public static string BoatName => playerShip.Name;
        public static bool Alarmed { get; private set; }

        public static Action<DirectObservation> OnObservationChanged;
        public static Action<DirectObservation> OnObservationAdded;

        public static Entity MostDistanceEntity;

        private void onAlarmStarted()
        {
            Alarmed = true;
            if (OnAlarmStarted != null)
            {
                OnAlarmStarted();
            }
        }

        private void onAlarmStoped()
        {
            Alarmed = false;
            if (OnAlarmStopped != null)
            {
                OnAlarmStopped();
            }
            observations.Clear();
        }

        private void onObservationChanded(DirectObservation observation)
        {
            determineMostDistance(observation.Entity);
            Debug.Log("Schussskizze - Observation Changed");
            if (OnObservationChanged != null)
            {
                OnObservationChanged(observation);
            }
        }

        private void determineMostDistance(Entity entity)
        {
            if (MostDistanceEntity == null || entity.Equals(MostDistanceEntity))
            {
                MostDistanceEntity = entity;
                return;
            }
            var distance = (entity.SandboxEntity.Position - playerShip.SandboxEntity.Position).magnitude;
            var last_distance = (entity.SandboxEntity.Position - MostDistanceEntity.SandboxEntity.Position).magnitude;
            if (distance > last_distance)
            {
                MostDistanceEntity = entity;
            }
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
            var cat = observation.Entity.Category;

            // ignore aircraft for now
            if (cat == "Aircraft" || cat == "Carrier-Borne Aircraft") return;

            if (observation.Entity.Country.GetRelationWith(playerShip.Country) == Country.Relation.Enemy)
            {
                observation.Changed += onObservationChanded;
                observations.Add(observation);
                determineMostDistance(observation.Entity);
                if (OnObservationAdded != null)
                {
                    OnObservationAdded(observation);
                }
            }
        }

        private void onObservationRemoved(Observator observer, DirectObservation observation)
        {
            //Debug.Log("Schussskizze - Observation removed! Name: " + observation.Entity.Name + " (total: " + playerShip.GetObservationsDirect().Count + ")");
            if (observations.Contains(observation))
            {
                //Debug.Log("Observation in list removed!");
                //Do nothing
            }
        }

        private void wireEvents()
        {
            playerShip.AlarmStarted += onAlarmStarted;
            playerShip.AlarmStopped += onAlarmStoped;
            playerShip.ObservationAdded += onObservationAdded;
            playerShip.ObservationAdded += onObservationRemoved;
        }

        private void unwireAllEvents()
        {
            playerShip.AlarmStarted -= onAlarmStarted;
            playerShip.AlarmStopped -= onAlarmStoped;
            playerShip.ObservationAdded -= onObservationAdded;
            playerShip.ObservationAdded -= onObservationRemoved;
            SceneEventsListener.OnSceneAwake -= onSceneAwake;
        }

        public void OnLoaded()
        {
            Debug.Log("Schussskizze mod is active!");

            var harmony = HarmonyInstance.Create("com.dws.uboat");
            harmony.PatchAll();

            SceneEventsListener.OnSceneAwake += onSceneAwake;
        }

        private static float UpdatePlayerPosition()
        {
            if (OnPlayerPosition != null)
            {
                OnPlayerPosition(new Vector3(playerShip.SandboxEntity.Position.x, playerShip.SandboxEntity.Position.y, 0));
            }
            return playerPositionUpdateTime;
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
                    wireEvents();
                    executionQueue.AddTimedUpdateListener(UpdatePlayerPosition, playerPositionUpdateTime);
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
