using UnityEngine;
using MeshTangentExtension;

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
            mesh.UploadMeshData(true);
            return mesh;
        }

        void ValidateState()
        {
            if (vertices == null)
            {
                throw new System.InvalidOperationException("MeshData is missing vertices!");
            }
            if (triangles == null)
            {
                throw new System.InvalidOperationException("MeshData is missing triangles!");
            }
            if (uv == null)
            {
                throw new System.InvalidOperationException("MeshData is missing texture uvs!");
            }
        }
    } 
}
