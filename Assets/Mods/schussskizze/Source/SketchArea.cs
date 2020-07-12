using DWS.Common.Resources;
using System;
using System.Collections.Generic;
using UBOAT.Game.Core.Serialization;
using UBOAT.Game.Core.Time;
using UBOAT.Game.Scene.Entities;
using UBOAT.ModAPI.Core.InjectionFramework;
using UnityEngine;
using UnityEngine.UI;

namespace UBOAT.Mods.Schussskizze
{
    public class Track
    {
        public Vector3 LastKnowPosition;
        public DirectObservation Observation;
        public long LastObservationTime;
        public Vector3 EstimatedPostion;
        public float Error = float.NaN;
    }

    [NonSerializedInGameState]
    public class SketchArea : MonoBehaviour
    {
        [Inject]
        private static GameTime gameTime;
        [Inject]
        private static ResourceManager resourceManager;
        private Texture2D texture;
        private Matrix4x4 matrix;
        private Vector3 last_position = Vector3.zero;
        private float line_width = 10;
        private float scale = 60;
        private Dictionary<Entity, Track> tracks = new Dictionary<Entity, Track>();
        private Vector3 texture_offset;
        private Dictionary<string, Texture2D> splats = new Dictionary<string, Texture2D>();
        private Vector2 viewportSize = new Vector2(1040, 585);
        private Vector2 textureSize = new Vector2(1920, 1080);
        private float TextureToViewPortScale;
        private float entityToSandboxEntityScale = 1f/1000f;

        public static Action<Vector2, string> AddSplatAtUIPos;
        public static Action<Vector2, Vector2> DrawLineWithUICoords;

        public void Start()
        {
            Debug.Log("Player has started a sketch.");
            
            InitTexture();

            TextureToViewPortScale = viewportSize.x / texture.width;

            texture_offset = new Vector3(texture.width / 2, texture.height / 2, 0);
            var most_distant_offset = (Schussskizze.PlayerPostion2D - Schussskizze.MostDistanceEntity.SandboxEntity.Position);
            var offset = new Vector3(
                most_distant_offset.x, 
                most_distant_offset.y, 
                0);

            var mySprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            GetComponent<Image>().sprite = mySprite;

            matrix.SetTRS((7f * offset.normalized - Schussskizze.PlayerPostion) * scale, Quaternion.identity, Vector3.one);

            last_position = matrix * Schussskizze.PlayerPostion;

            Schussskizze.OnPlayerPosition += onPlayerPositionUpdate;
            Schussskizze.OnObservationChanged += onObservationChanged;
            Schussskizze.OnObservationAdded += onObservatioAdded;

            loadSplatsAt(last_position, "PlayerStartPoint");

            foreach (DirectObservation o in Schussskizze.Observations)
            {
                DrawTrack(o);
            }

            AddSplatAtUIPos += addSplatAtUIPos;
            DrawLineWithUICoords += drawLineAtUICoords;
        }

        public void OnDestroy()
        {
            Schussskizze.OnPlayerPosition -= onPlayerPositionUpdate;
            Schussskizze.OnObservationChanged -= onObservationChanged;
            Schussskizze.OnObservationAdded -= onObservatioAdded;
            foreach (DirectObservation o in Schussskizze.Observations)
            {
                o.EstimationChanged -= drawNewCustomObservation;
            }
            AddSplatAtUIPos -= addSplatAtUIPos;
        }

        void drawLineAtUICoords(Vector2 v1, Vector2 v2)
        {
            var v1_px = v1 * (1f/TextureToViewPortScale) + new Vector2(texture_offset.x, texture_offset.y);
            var v2_px = v2 * (1f/TextureToViewPortScale) + new Vector2(texture_offset.x, texture_offset.y);
            Debug.Log("PLot Line: " + v1_px + " " + v2_px);
            plotLineWidth((int)v1_px.x, (int)v1_px.y, (int)v2_px.x, (int)v2_px.y, 1f);
            texture.Apply();
        }

        void addSplatAtUIPos(Vector2 postion, string splat)
        {
            var splatObject = resourceManager.InstantiatePrefab("UI/" + splat);
            splatObject.transform.SetParent(this.transform, true);
            splatObject.transform.SetAsLastSibling();
            splatObject.transform.localPosition = new Vector3(postion.x, postion.y, 0);
        }

        void loadSplatsAt(Vector3 position, string splat)
        {
            var point = matrix.MultiplyPoint3x4(scale * position) * TextureToViewPortScale;
            Debug.Log("Draw mark at: " + position);
            Debug.Log("Draw mark at UI point: " + point);
            var start_point = resourceManager.InstantiatePrefab("UI/" + splat);
            start_point.transform.SetParent(this.transform, true);
            start_point.transform.SetAsLastSibling();
            start_point.transform.localPosition = matrix.MultiplyPoint3x4(scale * position) * TextureToViewPortScale;
        }

        void InitTexture()
        {
            texture = new Texture2D(1920, 1080);
            texture.Fill(new Color(0f, 0f, 0f, 0f));
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply();
        }

        void SplatTexture(int x, int y, Texture2D splat)
        {
            texture.SetPixels(x, y, splat.width, splat.height, splat.GetPixels());
            createSprite();
        }

        void plotLineWidth(int x0, int y0, int x1, int y1, float wd)
        {
            int dx = abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx - dy, e2, x2, y2;                          /* error value e_xy */
            float ed = dx + dy == 0 ? 1 : sqrt((float)dx * dx + (float)dy * dy);

            for (wd = (wd + 1) / 2; ;)
            {                                   /* pixel loop */
                setPixelColor(x0, y0, max(0, 255 * (abs(err - dx + dy) / ed - wd + 1)));
                e2 = err; x2 = x0;
                if (2 * e2 >= -dx)
                {                                           /* x step */
                    for (e2 += dy, y2 = y0; e2 < ed * wd && (y1 != y2 || dx > dy); e2 += dx)
                    {
                        y2 += sy;
                        setPixelColor(x0, y2, max(0, 255 * (abs(e2) / ed - wd + 1)));
                    }
                    if (x0 == x1) break;
                    e2 = err; err -= dy; x0 += sx;
                }
                if (2 * e2 <= dy)
                {                                            /* y step */
                    for (e2 = dx - e2; e2 < ed * wd && (x1 != x2 || dx < dy); e2 += dy)
                    {
                        x2 += sx;
                        setPixelColor(x2, y0, max(0, 255 * (abs(e2) / ed - wd + 1)));
                    }
                    if (y0 == y1) break;
                    err += dx; y0 += sy;
                }
            }
        }

        private void setPixelColor(int x0, int y2, int v)
        {
            texture.SetPixel(x0, y2, new Color(v/255f, v/255f, v/255f, 1f));
        }

        private int max(int v1, float v2)
        {
            return (int)Mathf.Max(v1, v2);
        }

        private int sqrt(float v)
        {
            return (int)Mathf.Sqrt(v);
        }

        private int abs(int v)
        {
            return Mathf.Abs(v);
        }

        private void onPlayerPositionUpdate(Vector3 position)
        {
            DrawTrackLine(last_position, position, 10f);
            last_position = position;
        }

        void createSprite()
        {
            texture.Apply();
            var mySprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            GetComponent<Image>().sprite = mySprite;
        }

        private void DrawTrackLine(Vector3 v1, Vector3 v2, float width)
        {
            var formatted_last_position = matrix.MultiplyPoint3x4(scale * v1) + texture_offset;
            var formatted_position = matrix.MultiplyPoint3x4(scale * v2) + texture_offset;
            plotLineWidth(
                (int)formatted_last_position.x,
                (int)formatted_last_position.y,
                (int)formatted_position.x,
                (int)formatted_position.y,
                width
            );
            createSprite();
        }

        private void drawNewCustomObservation(DirectObservation observation)
        {
            DrawTrack(observation);
            var track = tracks[observation.Entity];
            loadSplatsAt(track.EstimatedPostion, "XMarker_Icon");
            DrawTrackLine(track.EstimatedPostion, Schussskizze.PlayerPostion, 0.5f);
        }

        private void onObservatioAdded(DirectObservation observation)
        {
            DrawTrack(observation);
        }

        private void onObservationChanged(DirectObservation observation)
        {
            DrawTrack(observation);
        }

        void DrawTrack(DirectObservation observation)
        {
            if (tracks.ContainsKey(observation.Entity))
            {
                var track = tracks[observation.Entity];
                var current_position = new Vector3(
                        observation.Entity.SandboxEntity.Position.x,
                        observation.Entity.SandboxEntity.Position.y,
                        0
                        );
                var last_estimate = track.EstimatedPostion;
                estimatePostion(ref track);
                DrawTrackLine(last_estimate, track.EstimatedPostion, 10f);
                track.LastKnowPosition = current_position;
                track.LastObservationTime = gameTime.StoryTicks;
                track.Observation = observation;
            }
            else
            {
                var track = new Track();
                track.LastObservationTime = gameTime.StoryTicks;
                track.Observation = observation;
                track.LastKnowPosition = new Vector3(
                    observation.Entity.SandboxEntity.Position.x,
                    observation.Entity.SandboxEntity.Position.y,
                    0
                );
                estimatePostion(ref track);
                Debug.Log("Track Error: " + track.Error);
                Debug.Log("Track Estimated Position: " + track.EstimatedPostion);
                Debug.Log("Track LastKnowPositon: " + track.LastKnowPosition);
                tracks.Add(observation.Entity, track);
                loadSplatsAt(track.EstimatedPostion, "EnemyContactPoint");
                observation.EstimationChanged += drawNewCustomObservation;
            }
        }

        void estimatePostion(ref Track track)
        {
            var target_position = new Vector3(track.Observation.Entity.SandboxEntity.Position.x, track.Observation.Entity.SandboxEntity.Position.y, 0);
            var offset = (target_position - Schussskizze.PlayerPostion);
            if (!float.IsNaN(track.Observation.EstimatedDistance))
            {
                Debug.Log("Distance Estimate from Observation: " + track.Observation.EstimatedDistance);
                var observed_offset = offset.normalized * track.Observation.EstimatedDistance * entityToSandboxEntityScale;
                track.EstimatedPostion = Schussskizze.PlayerPostion + observed_offset;
                track.Error = offset.magnitude - track.Observation.EstimatedDistance * entityToSandboxEntityScale;
                return;
            }
            else if (!float.IsNaN(track.Error))
            {
                track.EstimatedPostion = (offset.magnitude + track.Error) * offset.normalized + Schussskizze.PlayerPostion;
                return;
            }
            var distance = offset.magnitude;
            var error = Schussskizze.CrewAccuracy * distance;
            error = UnityEngine.Random.Range(-error, error);
            var estimate = distance + error;
            var estimated_offset = (estimate / distance) * offset;
            track.EstimatedPostion = estimated_offset + Schussskizze.PlayerPostion;
            track.Error = error;
        }
    }
}