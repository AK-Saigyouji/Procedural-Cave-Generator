using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace AKSaigyouji.Modules.Outlines
{
    public struct Edge
    {
        public Vector3 EndPoint { get { return new Vector3(end.x, 0f, end.y); } }
        public Vector3 StartPoint { get { return new Vector3(start.x, 0f, start.y); } }
        public Vector3 MidPoint { get { return (EndPoint + StartPoint) / 2; } }
        public Vector3 Direction { get { return (EndPoint - StartPoint).normalized; } }
        public float Length { get { return (end - start).magnitude; } }

        readonly Vector2 start;
        readonly Vector2 end;

        public Edge(Vector3 start, Vector3 end)
        {
            // we omit the y-values to reduce the struct size to 16 bytes, since the y-value is always 0.
            this.start = new Vector2(start.x, start.z);
            this.end = new Vector2(end.x, end.z);
        }
    } 
}