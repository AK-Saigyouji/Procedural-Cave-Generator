using System;
using UnityEngine;
using MeshTangentCalculator;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Holds core data necessary to build a mesh. Can use outside of the main thread, unlike the Mesh class in the Unity API.
    /// Note that for performance reasons accessing data does not produce copies, so be careful about altering state.
    /// </summary>
    sealed class MeshData
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
        public MeshData Clone()
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
