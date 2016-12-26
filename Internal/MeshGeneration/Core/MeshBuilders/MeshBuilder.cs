using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    static class MeshBuilder
    {
        public static MeshData BuildFloor(WallGrid grid, IHeightMap heightMap)
        {
            WallGrid invertedGrid = grid.Invert();
            return BuildFlatMesh(invertedGrid, heightMap);
        }

        public static MeshData BuildWalls(Outline[] outlines, IHeightMap floorHeightMap, IHeightMap ceilingHeightMap)
        {
            return WallBuilder.Build(outlines, floorHeightMap, ceilingHeightMap);
        }

        public static MeshData BuildCeiling(WallGrid grid, IHeightMap heightMap)
        {
            return BuildFlatMesh(grid, heightMap);
        }

        public static MeshData BuildEnclosure(MeshData floor, IHeightMap heightMap)
        {
            MeshData mesh = floor.Clone();
            ApplyHeightMap(mesh.vertices, heightMap);
            FlipVisibility(mesh);
            return mesh;
        }

        static MeshData BuildFlatMesh(WallGrid grid, IHeightMap heightMap)
        {
            MeshData mesh = MapTriangulator.Triangulate(grid);
            mesh.uv = ComputeFlatUVArray(mesh.vertices);
            ApplyHeightMap(mesh.vertices, heightMap);
            return mesh;
        }

        static Vector2[] ComputeFlatUVArray(Vector3[] vertices)
        {
            const float UVSCALE = 50;
            var uv = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                float percentX = vertices[i].x / UVSCALE;
                float percentY = vertices[i].z / UVSCALE;
                uv[i] = new Vector2(percentX, percentY);
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

        // The floor is visible from above, but the ceiling should be visible from below. By reversing
        // the triangles we flip the direction of visibility.
        static void FlipVisibility(MeshData mesh)
        {
            System.Array.Reverse(mesh.triangles);
        }
    } 
}