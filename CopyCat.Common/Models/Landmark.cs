using System;
using System.Collections.Generic;
using System.Text;

namespace CopyCat.Common.Models
{
    [Serializable]
    public abstract class Landmark
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        public float Visibility { get; }
        public float Presence { get; }

        public Landmark(
            float x,
            float y,
            float z,
            float visibility,
            float presence)
        {
            X = x;
            Y = y;
            Z = z;
            Visibility = visibility;
            Presence = presence;
        }
    }
}
