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

        public static MeshData BuildWalls(WallGrid grid, IHeightMap floorHeightMap, IHeightMap ceilingHeightMap)
        {
            return WallBuilder.Build(grid, floorHeightMap, ceilingHeightMap);
        }

        public static MeshData BuildCeiling(WallGrid grid, IHeightMap heightMap)
        {
            return BuildFlatMesh(grid, heightMap);
        }

        public static MeshData BuildEnclosure(WallGrid grid, IHeightMap heightMap)
        {
            WallGrid invertedGrid = grid.Invert();
            MeshData mesh = BuildFlatMesh(invertedGrid, heightMap);
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