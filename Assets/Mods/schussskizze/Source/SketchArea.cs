using UBOAT.Game.Core.Serialization;
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