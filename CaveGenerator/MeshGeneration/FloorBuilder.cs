using UnityEngine;
using System.Linq;
using System.Collections;

namespace CaveGeneration.MeshGeneration
{
    public class FloorBuilder : IMeshBuilder
    {

        MeshData mesh;
        Map map;

        const string name = "Floor Mesh";

        public FloorBuilder(Map map)
        {
            this.map = map;
        }

        public MeshData Build()
        {
            InvertMap();
            TriangulateMap();
            ComputeUV();
            mesh.name = name;
            return mesh;
        }

        void InvertMap()
        {
            map = new Map(map);
            map.Invert();
        }

        void TriangulateMap()
        {
            mesh = new MeshData();
            MapTriangulator mapTriangulator = new MapTriangulator(map);
            mapTriangulator.Triangulate();
            mesh.triangles = mapTriangulator.meshTriangles;
            mesh.vertices = mapTriangulator.meshVertices;
        }

        void ComputeUV()
        {
            Vector3[] vertices = mesh.vertices;
            Vector2[] uv = new Vector2[vertices.Length];
            float textureScale = Map.maxSubmapSize;
            for (int i = 0; i < vertices.Length; i++)
            {
                float percentX = vertices[i].x / textureScale;
                float percentY = vertices[i].z / textureScale;
                uv[i] = new Vector2(percentX, percentY);
            }
            mesh.uv = uv;
        }
    } 
}
