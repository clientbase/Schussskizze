using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UBOAT.Mods.Schussskizze
{
    public class ShotSketchData
    {
        public string DateTimeString;
        public List<PositionTime> PlayerMovements;
    }

    public class PositionTime
    {
        public Vector3 Position;
        public long AtStoryTick; 
    }
}

