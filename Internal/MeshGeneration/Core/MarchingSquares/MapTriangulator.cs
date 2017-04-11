/* This class turns a WallGrid, which is a grid of 0s and 1s, into vertices and triangles for a Mesh.
 * It does this using the Marching Squares algorithm. A square in this context refers to four points in the grid 
 * in the arrangement (x,y),(x+1,y),(x,y+1),(x+1,y+1). The algorithm iterates over all such squares, and builds triangles
 * based on which of the four corners are walls (giving rise to 16 configurations). The vertices of the triangles are taken
 * from the four corners of the square plus the four midpoints of the square. 
 * 
 * A specialized data structure is used to cache vertices during triangulation to avoid doubling up on vertices. 
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
        public static MeshData Triangulate(WallGrid wallGrid)
        {
            byte[,] configurations = MarchingSquares.ComputeConfigurations(wallGrid);
            byte[][] configurationTable = MarchingSquares.BuildConfigurationTable();
            MeshSizes meshSizes = ComputeMeshSizes(configurations, configurationTable);

            var localVertices = new LocalPosition[meshSizes.NumVertices];
            var triangles = new int[meshSizes.NumTriangles];
            ushort numVertices = 0;
            int numTriangles = 0;

            var vertexIndices = new ushort[MarchingSquares.MAX_VERTICES_IN_TRIANGULATION];
            var vertexCache = new VertexLookup(wallGrid.Length);

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
                        if (!vertexCache.TryGetCachedVertex(x, y, point, out vertexIndex))
                        {
                            vertexIndex = numVertices++;
                            localVertices[vertexIndex] = new LocalPosition(x, y, point);
                        }
                        vertexCache.CacheVertex(x, point, vertexIndex);
                        vertexIndices[i] = vertexIndex;
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
            mesh.vertices = ToGlobalVertices(localVertices, numVertices, wallGrid.Scale, wallGrid.Position);
            mesh.triangles = triangles;
            return mesh;
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
            int totalVertices = 0;
            int totalTriangles = 0;
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    int config = configurations[x, y];
                    int numVertices = configurationTable[config].Length;
                    totalVertices += numVertices;
                    // As an example, the points 0, 1, 5, 6 give us the two triangles
                    // 0,1,5 and 0,5,6. Each triangle contributes three elements to the triangles array. Hence
                    // the following computation:
                    totalTriangles += 3 * Mathf.Max(numVertices - 2, 0);
                }
            }
            return new MeshSizes(totalVertices, totalTriangles);
        }

        // This exists simply to return these two pieces of data from a single function call.
        sealed class MeshSizes
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
