using System.Collections;
using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Holds core data necessary to build a mesh. Creating MeshData and 
    /// assigning data is threadsafe, unlike the Mesh class in the Unity API. Note that for performance reasons
    /// accessing data does not produce copies, so be careful about altering state.
    /// </summary>
    public class MeshData
    {
        public Vector3[] vertices { get; set; }
        public Vector2[] uv { get; set; }
        public int[] triangles { get; set; }
        public Vector3[] normals { get; set; }
        public Vector4[] tangents { get; set; }

        /// <summary>
        /// Convert MeshData into Mesh. Not threadsafe.
        /// </summary>
        public Mesh CreateMesh()
        {
            ValidateState();
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.uv = uv;
            mesh.tangents = TangentSolver.DetermineTangents(this, mesh.normals);
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
