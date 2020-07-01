using UBOAT.Game.Core.Serialization;
using UBOAT.Game.Scene.Entities;
using UBOAT.ModAPI.Core.InjectionFramework;
using UnityEngine;

namespace UBOAT.Mods.Schussskizze
{
    [NonSerializedInGameState]
    public class SketchArea : MonoBehaviour
    {
        public void Start()
        {
            Debug.Log("Player has started a sketch.");
        }
    }
}