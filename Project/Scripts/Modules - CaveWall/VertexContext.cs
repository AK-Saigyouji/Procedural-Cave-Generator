using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using AKSaigyouji.HeightMaps;

namespace AKSaigyouji.Modules.CaveWalls
{
    /// <summary>
    /// Represents the context for a vertex in a cavewall.
    /// </summary>
    public sealed class VertexContext
    {
        public Vector3 Vertex { get { return vertex; } }
        public Vector3 Normal { get { return normal; } }
        public bool IsFloor { get { return interpolationIndex == numVertices - 1; } }
        public bool IsCeiling { get { return interpolationIndex == 0; } }
        public int InterpolationIndex { get { return interpolationIndex; } }

        readonly IHeightMap floorHeightMap;
        readonly IHeightMap ceilingHeightMap;
        readonly int numVertices;

        int interpolationIndex;
        Vector3 vertex;
        Vector3 normal;

        public VertexContext(IHeightMap floor, IHeightMap ceiling, int numVerticesPerCorner)
        {
            if (floor == null)
                throw new ArgumentNullException("floor");

            if (ceiling == null)
                throw new ArgumentNullException("ceiling");

            if (numVertices < 0)
                throw new ArgumentOutOfRangeException("numVerticesPerCorner");

            floorHeightMap = floor;
            ceilingHeightMap = ceiling;
            numVertices = numVerticesPerCorner;
        }

        public void Update(Vector3 vertex, Vector3 normal, int interpolationIndex)
        {
            this.vertex = vertex;
            this.normal = normal;
            this.interpolationIndex = interpolationIndex;
        }

        public float GetFloorHeightAt(float x, float z)
        {
            return floorHeightMap.GetHeight(x, z);
        }

        public float GetCeilingHeightAt(float x, float z)
        {
            return ceilingHeightMap.GetHeight(x, z);
        }
    } 
}