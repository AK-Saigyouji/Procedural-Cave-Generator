using UnityEngine;
using System.Linq;
using System.Collections;

namespace CaveGeneration.MeshGeneration
{
    public sealed class FloorBuilder : IMeshBuilder
    {
        MeshData mesh;
        Map map;
        IHeightMap heightMap;

        const float UVSCALE = 50f;

        public FloorBuilder(Map map, IHeightMap heightMap)
        {
            this.map = map;
            this.heightMap = heightMap;
        }

        public MeshData Build()
        {
            InvertMap();
            TriangulateMap();
            ApplyHeightMap();
            ComputeUV();
            return mesh;
        }

        void InvertMap()
        {
            map = new Map(map);
            map.Invert();
        }

        void TriangulateMap()
        {
            MapTriangulator mapTriangulator = new MapTriangulator(map);
            mesh = mapTriangulator.Triangulate();
        }

        void ApplyHeightMap()
        {
            if (heightMap != null)
            {
                for (int i = 0; i < mesh.vertices.Length; i++)
                {
                    Vector3 vertex = mesh.vertices[i];
                    mesh.vertices[i].y += heightMap.GetHeight(vertex.x, vertex.z);
                }
            }
        }

        void ComputeUV()
        {
            Vector3[] vertices = mesh.vertices;
            Vector2[] uv = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                float percentX = vertices[i].x / UVSCALE;
                float percentY = vertices[i].z / UVSCALE;
                uv[i] = new Vector2(percentX, percentY);
            }
            mesh.uv = uv;
        }
    } 
}
