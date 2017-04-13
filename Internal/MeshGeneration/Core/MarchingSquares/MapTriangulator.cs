/* This class turns a WallGrid, which is a grid of 0s and 1s, into vertices and triangles for a Mesh.
 * It does this using the Marching Squares algorithm. A square in this context refers to four points in the grid 
 * in the arrangement (x,y),(x+1,y),(x,y+1),(x+1,y+1). The algorithm iterates over all such squares, and builds triangles
 * based on which of the four corners are walls (giving rise to 16 configurations). The vertices of the triangles are taken
 * from the four corners of the square plus the four midpoints of the square. 
 */

using UnityEngine;
using UnityEngine.Assertions;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Triangulates a Map according to the Marching Squares algorithm, yielding vertices and triangles ready to be
    /// used in a mesh.
    /// </summary>
    static class MapTriangulator
    {
        const int perSquareCacheSize = 5;

        public static MeshData Triangulate(WallGrid wallGrid)
        {
            Vector3 basePosition = wallGrid.Position;
            int scale = wallGrid.Scale;

            byte[,] configurations = MarchingSquares.ComputeConfigurations(wallGrid);
            byte[][] configurationTable = MarchingSquares.BuildConfigurationTable();
            MeshSizes meshSizes = ComputeMeshSizes(configurations, configurationTable);

            // Note: meshSizes.NumVertices overcounts shared vertices.
            var localVertices = new LocalPosition[meshSizes.NumVertices];
            var triangles = new int[meshSizes.NumTriangles];
            ushort numVertices = 0;
            int numTriangles = 0;

            // Stores vertex indices for a single square at a time.
            var vertexIndices = new ushort[MarchingSquares.MAX_VERTICES_IN_TRIANGULATION];

            var currentRow  = new ushort[perSquareCacheSize, wallGrid.Length];
            var previousRow = new ushort[perSquareCacheSize, wallGrid.Length];

            var isOnLeftSide = new bool[] { true, false, false, false, false, false, true, true };
            var isOnBottom   = new bool[] { false, false, false, false, true, true, true, false };

            var bottomOffset = new sbyte[] { -1, -1, -1, -1, 2, 1, 0, -1 };
            var leftOffset   = new sbyte[] { 2, -1, -1, -1, -1, -1, 4, 3 };

            int width = configurations.GetLength(1);
            int length = configurations.GetLength(0);
            for (byte y = 0; y < width; y++)
            {
                for (byte x = 0; x < length; x++)
                {
                    int config = configurations[x, y];
                    byte[] points = configurationTable[config];
                    for (int i = 0; i < points.Length; i++)
                    {
                        byte point = points[i];
                        ushort vertexIndex;
                        if (isOnBottom[point] && y > 0)
                        {
                            vertexIndex = previousRow[bottomOffset[point], x];
                        }
                        else if (isOnLeftSide[point] && x > 0)
                        {
                            vertexIndex = currentRow[leftOffset[point], x - 1];
                        }
                        else
                        {
                            vertexIndex = numVertices++;
                            localVertices[vertexIndex] = new LocalPosition(x, y, point);
                        }
                        if (point < perSquareCacheSize)
                        {
                            currentRow[point, x] = vertexIndex;
                        }
                        vertexIndices[i] = vertexIndex;
                    }
                    int numTrianglesToBuild = points.Length - 2;
                    for (int i = 0; i < numTrianglesToBuild; i++)
                    {
                        triangles[numTriangles++] = vertexIndices[0];
                        triangles[numTriangles++] = vertexIndices[i + 1];
                        triangles[numTriangles++] = vertexIndices[i + 2];
                    }
                }
                SwapRows(ref currentRow, ref previousRow);
            }
            MeshData mesh = new MeshData();
            mesh.vertices = ToGlobalVertices(localVertices, numVertices, wallGrid.Scale, wallGrid.Position);
            mesh.triangles = triangles;
            return mesh;
        }

        static void SwapRows(ref ushort[,] currentRow, ref ushort[,] previousRow)
        {
            var temp = currentRow;
            currentRow = previousRow;
            previousRow = temp;
        }

        static Vector3[] ToGlobalVertices(LocalPosition[] localVertices, int numVertices, int scale, Vector3 position)
        {
            var vertices = new Vector3[numVertices];
            for (int i = 0; i < numVertices; i++)
            {
                vertices[i] = localVertices[i].ToGlobalPosition(scale, position);
            }
            return vertices;
        }

        static MeshSizes ComputeMeshSizes(byte[,] configurations, byte[][] configurationTable)
        {
            int width = configurations.GetLength(1);
            int length = configurations.GetLength(0);
            int numVertices = 0;
            int numTriangles = 0;
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    int config = configurations[x, y];
                    if (config > 0)
                    {
                        int numVerticesInSquare = configurationTable[config].Length;
                        numVertices += numVerticesInSquare;
                        // As an example, the points 0, 1, 5, 6 give us the two triangles
                        // 0,1,5 and 0,5,6. Each triangle contributes three elements to the triangles array. Hence
                        // the following computation:
                        numTriangles += 3 * (numVerticesInSquare - 2);
                    }
                }
            }
            return new MeshSizes(numVertices, numTriangles);
        }

        // This exists simply to return these two pieces of data from a single function call.
        struct MeshSizes
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
