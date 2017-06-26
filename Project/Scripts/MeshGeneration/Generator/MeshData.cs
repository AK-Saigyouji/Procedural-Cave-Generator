/* Unity's Mesh class has two issues that motivated the creation of this class. The first is that it can only be used
 on the primary thread, like much of the Unity API. The second is that the access methods for vertices, triangles,
 uv, normals, and tangents all create copies of buffered data. Although this is actually sound design, the 
 MeshGeneration system requires the same properties (e.g. vertices) to be accessed multiple times throughout
 the system. Given the size of these arrays (potentially tens or hundreds of KB each), this is a substantial amount
 of garbage to create and may bloat the working set of the program.*/

using System;
using UnityEngine;
using MeshTangentCalculator;

namespace AKSaigyouji.MeshGeneration
{
    /// <summary>
    /// Holds core data necessary to build a mesh. Can use outside of the main thread, unlike the Mesh class in the Unity API.
    /// Note that for performance reasons accessing data does not produce copies, so be careful about altering state.
    /// </summary>
    public sealed class MeshData
    {
        public Vector3[] vertices { get; set; }
        public Vector2[] uv { get; set; }
        public int[] triangles { get; set; }

        /// <summary>
        /// Convert MeshData into Mesh. Must have assigned vertex, triangle and uv arrays. Must be called on the main thread,
        /// as it uses Unity's Mesh class.
        /// </summary>
        /// <exception cref="System.InvalidOperationException"></exception>
        public Mesh CreateMesh()
        {
            ValidateState();
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents(vertices, triangles, uv, mesh.normals);
            return mesh;
        }

        /// <summary>
        /// Create a deep copy of this MeshData.
        /// </summary>
        public MeshData DeepCopy()
        {
            var mesh = new MeshData();
            mesh.vertices = (Vector3[])vertices.Clone();
            mesh.triangles = (int[])triangles.Clone();
            mesh.uv = (Vector2[])uv.Clone();
            return mesh;
        }

        void ValidateState()
        {
            if (vertices == null)
            {
                throw new InvalidOperationException("MeshData is missing vertices!");
            }
            if (triangles == null)
            {
                throw new InvalidOperationException("MeshData is missing triangles!");
            }
            if (uv == null)
            {
                throw new InvalidOperationException("MeshData is missing texture uvs!");
            }
        }
    } 
}
