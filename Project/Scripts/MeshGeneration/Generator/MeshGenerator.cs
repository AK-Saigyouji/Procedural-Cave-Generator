using AKSaigyouji.HeightMaps;
using AKSaigyouji.Modules.CaveWalls;
using UnityEngine;
using System.Collections.Generic;

namespace AKSaigyouji.MeshGeneration
{
    /// <summary>
    /// Generates various components of a cave. 
    /// </summary>
    public sealed class MeshGenerator
    {
        public MeshData BuildFloor(WallGrid grid, IHeightMap heightMap)
        {
            WallGrid invertedGrid = grid.Invert();
            return BuildFlatMesh(invertedGrid, heightMap);
        }

        public MeshData BuildWalls(WallGrid grid, IHeightMap floorHeightMap, IHeightMap ceilingHeightMap, CaveWallModule caveWall)
        {
            var outlines = BuildOutlines(grid);
            var wallBuilder = new WallBuilder(outlines, floorHeightMap, ceilingHeightMap, caveWall);
            return wallBuilder.Build();
        }

        public MeshData BuildCeiling(WallGrid grid, IHeightMap heightMap)
        {
            return BuildFlatMesh(grid, heightMap);
        }

        public MeshData BuildEnclosure(WallGrid grid, IHeightMap heightMap)
        {
            WallGrid invertedGrid = grid.Invert();
            MeshData mesh = BuildFlatMesh(invertedGrid, heightMap);
            FlipVisibility(mesh);
            return mesh;
        }

        public List<Vector3[]> BuildOutlines(WallGrid grid)
        {
            var generator = new OutlineGenerator();
            return generator.Generate(grid);
        }

        static MeshData BuildFlatMesh(WallGrid grid, IHeightMap heightMap)
        {
            var triangulator = new MapTriangulator();
            MeshData mesh = triangulator.Triangulate(grid);
            mesh.uv = ComputeFlatUVArray(mesh.vertices);
            ApplyHeightMap(mesh.vertices, heightMap);
            return mesh;
        }

        static Vector2[] ComputeFlatUVArray(Vector3[] vertices)
        {
            const float UVSCALE = 0.02f;
            var uv = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                uv[i].x = vertices[i].x * UVSCALE;
                uv[i].y = vertices[i].z * UVSCALE;
            }
            return uv;
        }

        static void ApplyHeightMap(Vector3[] vertices, IHeightMap heightMap)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertex = vertices[i];
                vertices[i].y = heightMap.GetHeight(vertex.x, vertex.z);
            }
        }

        // The floor is visible from above, but the enclosure should be visible from below. By reversing
        // the triangles we flip the direction of visibility.
        static void FlipVisibility(MeshData mesh)
        {
            System.Array.Reverse(mesh.triangles);
        }
    } 
}