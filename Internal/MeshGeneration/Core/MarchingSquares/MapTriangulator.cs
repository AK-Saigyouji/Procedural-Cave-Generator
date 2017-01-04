/* This class turns a WallGrid, which is a grid of 0s and 1s, into vertices and triangles for a Mesh.
 * It does this using the Marching Squares algorithm. A square in this context refers to four points in the grid 
 * in the arrangement (x,y),(x+1,y),(x,y+1),(x+1,y+1). The algorithm iterates over all such squares, and builds triangles
 * based on which of the four corners are walls (giving rise to 16 configurations). The corners of the triangles are taken
 * from the four corners of the square plus the four midpoints of the square. 
 * 
 * A specialized data structure is used to cache vertices during triangulation to avoid doubling up on vertices. */

using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Triangulates a Map according to the Marching Squares algorithm, yielding vertices and triangles ready to be
    /// used in a mesh.
    /// </summary>
    static class MapTriangulator
    {
        public static MeshData Triangulate(WallGrid wallGrid)
        {
            Vector3 position = wallGrid.Position;
            int scale = wallGrid.Scale;

            byte[,] configurations = ComputeConfigurations(wallGrid);
            MeshSizes meshSizes = ComputeMeshSizes(configurations);

            // Using lists would be simpler, but this is a performance critical script and using arrays directly
            // like this showed a measurable improvement to runtime.
            var vertices = new Vector3[meshSizes.NumVertices];
            var triangles = new int[meshSizes.NumTriangles];
            int numVertices = 0;
            int numTriangles = 0;

            var vertexIndices = new int[MarchingSquares.MAX_VERTICES_IN_TRIANGULATION];
            var vertexCache = new VertexLookup(wallGrid.Length);

            for (int y = 0; y < configurations.GetLength(1); y++)
            {
                for (int x = 0; x < configurations.GetLength(0); x++)
                {
                    int[] points = MarchingSquares.GetPoints(configurations[x, y]);
                    for (int i = 0; i < points.Length; i++)
                    {
                        var point = new LocalPosition(x, y, points[i]);
                        int index;
                        if (!vertexCache.TryGetCachedVertex(point, out index))
                        {
                            index = numVertices++;
                            vertices[index] = point.ToGlobalPosition(scale, position);
                        }
                        vertexCache.CacheVertex(point, index);
                        vertexIndices[i] = index;
                    }
                    for (int i = 0; i < points.Length - 2; i++)
                    {
                        triangles[numTriangles++] = vertexIndices[0];
                        triangles[numTriangles++] = vertexIndices[i + 1];
                        triangles[numTriangles++] = vertexIndices[i + 2];
                    }
                }
                vertexCache.FinalizeRow();
            }

            MeshData mesh = new MeshData();
            System.Array.Resize(ref vertices, numVertices); 
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            return mesh;
        }

        static MeshSizes ComputeMeshSizes(byte[,] configurations)
        {
            int width = configurations.GetLength(1);
            int length = configurations.GetLength(0);
            int totalVertices = 0;
            int totalTriangles = 0;
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    int numVertices = MarchingSquares.GetPoints(configurations[x, y]).Length;
                    totalVertices += numVertices;
                    // As an example, the points 0, 1, 5, 6 give us the two triangles
                    // 0,1,5 and 0,5,6. Each triangle contributes three elements to the triangles array. Hence
                    // the following computation:
                    totalTriangles += 3 * Mathf.Max(numVertices - 2, 0);
                }
            }
            return new MeshSizes(totalVertices, totalTriangles);
        }

        static byte[,] ComputeConfigurations(WallGrid wallGrid)
        {
            int length = wallGrid.Length - 1;
            int width = wallGrid.Width - 1;
            byte[,] configurations = new byte[length, width];
            byte[,] grid = wallGrid.ToByteArray();
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    configurations[x, y] = (byte)GetConfiguration(grid, x, y);
                }
            }
            return configurations;
        }

        static int GetConfiguration(byte[,] grid, int x, int y)
        {
            byte botLeft = grid[x, y];
            byte botRight = grid[x + 1, y];
            byte topRight = grid[x + 1, y + 1];
            byte topLeft = grid[x, y + 1];
            return MarchingSquares.ComputeConfiguration(botLeft, botRight, topRight, topLeft);
        }

        // This exists simply to return these two pieces of data from a single function call.
        class MeshSizes
        {
            public readonly int NumVertices;
            public readonly int NumTriangles;

            public MeshSizes(int vertices, int triangles)
            {
                NumVertices = vertices;
                NumTriangles = triangles;
            }
        }
    }
}
