using UnityEngine;
using System.Collections;

namespace CaveGeneration.MeshGeneration
{
    class EnclosureBuilder
    {
        MeshData floorMesh;
        int wallHeight;
        public MeshData mesh { get; private set; }

        public EnclosureBuilder(MeshData floorMesh, int wallHeight)
        {
            this.floorMesh = floorMesh;
            this.wallHeight = wallHeight;
        }

        public void Build()
        {
            mesh = FloorToEnclosureMesh(wallHeight);
        }

        MeshData FloorToEnclosureMesh(int wallHeight)
        {
            MeshData enclosureMesh = new MeshData();
            enclosureMesh.vertices = FloorToCeilingVertices(floorMesh.vertices, wallHeight);
            enclosureMesh.triangles = FloorToCeilingTriangles(floorMesh.triangles);
            enclosureMesh.uv = floorMesh.uv;
            return enclosureMesh;
        }

        Vector3[] FloorToCeilingVertices(Vector3[] floorVertices, int wallHeight)
        {
            Vector3[] ceilingVertices = new Vector3[floorVertices.Length];
            for (int i = 0; i < floorVertices.Length; i++)
            {
                ceilingVertices[i] = floorVertices[i] + Vector3.up * wallHeight;
            }
            return ceilingVertices;
        }

        int[] FloorToCeilingTriangles(int[] floorTriangles)
        {
            int length = floorTriangles.Length;
            int[] ceilingTriangles = new int[length];
            for (int i = 0; i < length; i++)
            {
                ceilingTriangles[i] = floorTriangles[length - 1 - i];
            }
            return ceilingTriangles;
        }
    } 
}
