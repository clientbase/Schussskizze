using System;
using UBOAT.Game.Core.Serialization;
using UBOAT.Game.Scene.Entities;
using UBOAT.ModAPI.Core.InjectionFramework;
using UnityEngine;
using UnityEngine.UI;

namespace UBOAT.Mods.Schussskizze
{
    [NonSerializedInGameState]
    public class SketchArea : MonoBehaviour
    {
        private Texture2D texture;
        private Matrix4x4 matrix;
        private Vector3 last_position = Vector3.zero;
        private float line_width = 10;
        private float scale = 100;

        public void Start()
        {
            Debug.Log("Player has started a sketch.");
            texture = new Texture2D(1920, 1080);
            plotLineWidth(0, 0, 1000, 1000, 10);
            texture.Apply();
            var mySprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            GetComponent<Image>().sprite = mySprite;

            matrix.SetTRS(-Schussskizze.PlayerPostion * scale + new Vector3(texture.width/2, texture.height/2, 0), Quaternion.identity, Vector3.one);
            last_position = Schussskizze.PlayerPostion;

            last_position = matrix * Schussskizze.PlayerPostion;
            Schussskizze.OnPlayerPosition += onPlayerPositionUpdate;
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
            texture.SetPixel(x0, y2, new Color(v, v, v, 1));
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
            Debug.Log("Sketch Area - Player Position: " + position);
            var formatted_last_position = matrix.MultiplyPoint3x4(scale * last_position);
            var formatted_position = matrix.MultiplyPoint3x4(scale * position);
            Debug.Log("Sketch Area - Formated Player Position: " + formatted_position);
            Debug.Log("Sketch Area - Formatted Last Position: " + formatted_last_position);
            plotLineWidth(
                (int)formatted_last_position.x,
                (int)formatted_last_position.y,
                (int)formatted_position.x,
                (int)formatted_position.y,
                line_width
                );
            texture.Apply();
            var mySprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            GetComponent<Image>().sprite = mySprite;
            last_position = position;
        }
    }
}