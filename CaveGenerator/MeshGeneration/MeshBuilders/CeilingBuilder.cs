using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Builds the mesh data for a cave ceiling, in particular the top part of a region of walls. Not to be confused
    /// with an enclosure, which is the part hanging above the floor. 
    /// </summary>
    class CeilingBuilder : IMeshBuilder
    {
        Map map;
        MeshData mesh;
        IHeightMap heightMap;
        int wallHeight;

        const string name = "Ceiling Mesh";

        public CeilingBuilder(Map map, IHeightMap heightMap)
        {
            this.map = map;
            wallHeight = heightMap.BaseHeight;
            this.heightMap = heightMap;
        }

        public MeshData Build()
        {
            TriangulateMap();
            ComputeCeilingUVArray();
            RaiseCeiling();
            ApplyHeightMap();
            return mesh;
        }

        void TriangulateMap()
        {
            MapTriangulator mapTriangulator = new MapTriangulator(map);
            mesh = mapTriangulator.Triangulate();
            mesh.name = name;
        }

        void ComputeCeilingUVArray()
        {
            Vector3[] vertices = mesh.vertices;
            Vector2[] uv = new Vector2[vertices.Length];
            float scale = Map.maxSubmapSize;
            for (int i = 0; i < vertices.Length; i++)
            {
                float percentX = vertices[i].x / scale;
                float percentY = vertices[i].z / scale;
                uv[i] = new Vector2(percentX, percentY);
            }
            mesh.uv = uv;
        }

        void RaiseCeiling()
        {
            if (wallHeight != 0)
            {
                Vector3[] vertices = mesh.vertices;
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i].y = wallHeight;
                } 
            }
        }

        void ApplyHeightMap()
        {
            if (!heightMap.IsSimple)
            {
                Vector3[] vertices = mesh.vertices;
                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 vertex = vertices[i];
                    vertices[i].y += heightMap.GetHeight(vertex.x, vertex.z);
                }
            }
        }
    } 
}
