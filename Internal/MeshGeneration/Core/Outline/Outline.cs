/* Outlines are generated from an existing collection of vertices, namely the vertex array in a mesh. We could
 store the vertices themselves, but this would cost 12 bytes per vertex (Vector3). Instead, an Outline stores
 an index into the original collection, and a reference to the collection. Given the 65k limit on vertices in 
 a mesh, this allows us to use 2 bytes per vertex instead of 12.
 
  The class is designed to hide these implementation details, and can be consumed like an array.*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Outline represents an ordered collection of points corresponding to points that separate a region of walls 
    /// from a region of space along the y-axis. 
    /// </summary>
    sealed class Outline
    {
        public int NumVertices { get { return indices.Length; } }
        public int NumEdges { get { return indices.Length - 1; } }
        public float PerimeterLength { get { return length; } }

        VertexIndex[] indices;
        Vector3[] vertices;
        float length;
        
        internal Outline(List<VertexIndex> indexList, Vector3[] vertices)
        {
            Assert.IsNotNull(indexList);
            Assert.IsNotNull(vertices);
            this.vertices = vertices;
            indices = new VertexIndex[indexList.Count];
            for (int i = 0; i < indexList.Count; i++)
            {
                indices[i] = indexList[i];
            }
            length = ComputeLength();
        }
        
        public void Reverse()
        {
            Array.Reverse(indices);
        }

        public Vector3 this[int i]
        {
            get
            {
                Vector3 vertex = vertices[indices[i]];
                vertex.y = 0;
                return vertex;
            }
        }

        float ComputeLength()
        {
            float length = 0;
            for (int i = 1; i < indices.Length; i++)
            {
                length += Vector3.Distance(this[i], this[i - 1]);
            }
            return length;
        }
    }
}
