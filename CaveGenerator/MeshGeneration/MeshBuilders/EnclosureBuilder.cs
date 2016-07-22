using UnityEngine;
using System.Collections;

namespace CaveGeneration.MeshGeneration
{
    class EnclosureBuilder : IMeshBuilder
    {
        MeshData floorMesh;
        MeshData mesh;

        int wallHeight;
        IHeightMap heightMap;

        const string name = "Enclosure Mesh";

        public EnclosureBuilder(MeshData floorMesh, int wallHeight, IHeightMap heightMap)
        {
            this.floorMesh = floorMesh;
            this.wallHeight = wallHeight;
            this.heightMap = heightMap;
        }

        public MeshData Build()
        {
            mesh = FloorToEnclosureMesh();
            ApplyHeightMap();
            mesh.name = name;
            return mesh;
        }

        MeshData FloorToEnclosureMesh()
        {
            MeshData enclosureMesh = new MeshData();
            enclosureMesh.vertices = FloorToCeilingVertices(floorMesh.vertices);
            enclosureMesh.triangles = FloorToCeilingTriangles(floorMesh.triangles);
            enclosureMesh.uv = floorMesh.uv;
            return enclosureMesh;
        }

        void ApplyHeightMap()
        {
            if (heightMap != null)
            {
                for (int i = 0; i < mesh.vertices.Length; i++)
                {
                    Vector3 vertex = mesh.vertices[i];
                    mesh.vertices[i].y -= heightMap.GetHeight(vertex.x, vertex.z);
                }
            }
        }

        Vector3[] FloorToCeilingVertices(Vector3[] floorVertices)
        {
            Vector3[] ceilingVertices = new Vector3[floorVertices.Length];
            for (int i = 0; i < floorVertices.Length; i++)
            {
                Vector3 floorVertex = floorVertices[i];
                ceilingVertices[i] = new Vector3(floorVertex.x, wallHeight, floorVertex.z);
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
